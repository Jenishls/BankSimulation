using BankingConsole.Models.Enums;

namespace BankingConsole.Models.AccountCreation;

public sealed class AccountCreationData
{
    public required Guid CustomerId { get; init; }
    public required Guid ProductId { get; init; }
    public required string BranchCode {get; init;}
    public required ProductType ProductType{get; init;}
    public SavingAccountCreationData? Saving { get; init; }
    public TermAccountCreationData? Term { get; init; }
    public LoanAccountCreationData? Loan { get; init; }
}