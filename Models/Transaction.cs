using System.ComponentModel.DataAnnotations;
using BankingConsole.Common;
using BankingConsole.Models.Enums;
using ValidationException = BankingConsole.Common.ValidationException;

namespace BankingConsole.Models;

public class Transaction
{
    public Guid TransactionId { get; set; }
    public Guid IdempotencyKey{ get; set; }
    public TransactionState State { get; set; }
    public EntryType Type { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime Timestamp { get; set; }
    public required List<LedgerEntry> Entries { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; private set; } = [];

    public Transaction() {}

    public static Transaction Create(
        IReadOnlyList<LedgerEntry> Entries,
        EntryType EntryType,
        string Description,
        Guid idempotencyKey)
    {
        if(Entries is null || !Entries.Any())
            throw new ArgumentException("Entries cannot be null or empty.", nameof(Entries));
        
        if(Entries.Count < 2)
            throw new ArgumentException("A transaction must have at least two entries.", nameof(Entries));
        
        if(Entries.Where(e => e.Flow == Flow.DEBIT).Sum(e => e.Amount) != Entries.Where(e => e.Flow == Flow.CREDIT).Sum(e => e.Amount))
            throw new ArgumentException("Transaction is not balanced. Total debits must equal total credits.", nameof(Entries));

        return new Transaction
        {
            TransactionId = Guid.NewGuid(),
            State = TransactionState.PENDING,
            Type = EntryType,
            Description = Description,
            Amount = Entries.Where(e => e.Flow == Flow.DEBIT).Sum(e => e.Amount), // Assuming the total amount is the debit total
            Timestamp = DateTime.UtcNow,
            Entries = Entries.ToList(),
            IdempotencyKey = idempotencyKey
        };
    }

    public void ValidatePosting()
    {
        if (State != TransactionState.PENDING)
            throw new ValidationException($"Only pending transactions can be posted. Current state: {State}");

        var failures = new List<string>();

        foreach (var group in Entries.GroupBy(e => e.Account))
        {
            var account = group.Key;
            var totalDebit = group.Where(e => e.Flow == Flow.DEBIT).Sum(e => e.Amount);
            var totalCredit = group.Where(e => e.Flow == Flow.CREDIT).Sum(e => e.Amount);
            var net = totalCredit - totalDebit;

            var canApply = net switch
            {
                < 0 => account.CanWithdraw(-net),
                > 0 => account.CanDeposit(net),
                _ => true
            };

            if (!canApply)
                failures.Add($"Account {account.AccountNumber}: insufficient balance or limit exceeded");
        }

        if (failures.Count > 0)
            throw new ValidationException("Transaction failed account validation.",failures);
    }

    public void Post(string description)
    {
        ValidatePosting();

        foreach (var entry in Entries.Where(e => e.Flow == Flow.DEBIT)
            .Concat(Entries.Where(e => e.Flow == Flow.CREDIT)))
        {
            var succeeded = entry.Flow == Flow.DEBIT
                ? entry.Account.Withdraw(entry.Amount)
                : entry.Account.Deposit(entry.Amount);

            if (!succeeded)
                throw new ConflictException($"Account operation failed unexpectedly for {entry.Account.AccountNumber}");
        }

        State = TransactionState.POSTED;
        Description = description;
    }
}