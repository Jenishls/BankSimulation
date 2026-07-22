using BankingConsole.Models.Enums;

namespace BankingConsole.Models;
public class LedgerEntry
{
    public Guid LedgerEntryId { get; set;}
    public Guid TransactionId { get; set; }
    public required Account Account { get; set; }
    public decimal Amount { get; set; }
    public Currency Currency { get; set; }
    public Flow Flow { get; set; }
    public LedgerEntry() {}
}