using BankingConsole.Models.Enums;
using AccountModel = BankingConsole.Models.Account.Account;

namespace BankingConsole.Models;

public sealed class LedgerEntry
{
    public Guid LedgerEntryId { get; private set; }
    public Guid TransactionId { get; private set; }
    public AccountModel Account { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public Currency Currency { get; private set; }
    public Flow Flow { get; private set; }

    private LedgerEntry()
    {
    }

    public static LedgerEntry Create(
        AccountModel account,
        Flow flow,
        decimal amount,
        Currency currency)
    {
        ArgumentNullException.ThrowIfNull(account);

        if (amount <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                amount,
                "Ledger entry amount must be greater than zero.");

        if (!Enum.IsDefined(flow))
            throw new ArgumentOutOfRangeException(nameof(flow), flow, "Unknown ledger flow.");

        if (!Enum.IsDefined(currency))
            throw new ArgumentOutOfRangeException(nameof(currency), currency, "Unknown currency.");

        return new LedgerEntry
        {
            LedgerEntryId = Guid.NewGuid(),
            Account = account,
            Amount = amount,
            Currency = currency,
            Flow = flow
        };
    }
}
