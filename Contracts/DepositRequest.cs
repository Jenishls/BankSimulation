using BankingConsole.Models.Enums;

namespace BankingConsole.Contracts;

public class DepositRequest
{
    public Guid SourceAccountId { get; set; }
    public Guid DestinationAccountId { get; set; }
    public Currency Currency { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
}