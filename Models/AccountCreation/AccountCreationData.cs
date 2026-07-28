using BankingConsole.Models.Enums;

namespace BankingConsole.Models.AccountCreation;

public sealed class AccountCreationData
{
    public required AccountType AccountType { get; init; }
    public required string Name { get; init; }
    public string? BranchCode { get; init; }
    public Guid? CustomerId { get; init; }
    public Guid? ProductId { get; init; }
    public OfficeAccountType? OfficeAccountType { get; init; }
    public decimal OpeningBalance { get; init; }
    public decimal? Principal { get; init; }
    public Guid? FundingAccountId { get; init; }
    public Guid? MaturitySettlementAccountId { get; init; }
    public Guid? DisbursementAccountId { get; init; }
    public Guid? RepaymentAccountId { get; init; }
    public int? RepaymentInstallments { get; init; }
}
