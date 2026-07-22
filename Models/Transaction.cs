using System.ComponentModel.DataAnnotations;
using BankingConsole.Models.Enums;

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
}