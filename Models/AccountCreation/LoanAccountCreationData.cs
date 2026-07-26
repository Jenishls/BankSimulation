namespace BankingConsole.Models.AccountCreation;
public sealed class LoanAccountCreationData
{
    public required decimal Principal { get; init; }
    public required Guid DisbursementAccountId { get; init; }
    public int RepaymentInstallments{get; init;}

}