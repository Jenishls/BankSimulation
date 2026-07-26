namespace BankingConsole.Models.AccountCreation;
public sealed class TermAccountCreationData
{
    public required decimal Principal { get; init; }
    public required DateTime MaturityDate{get; init;}
    public required Guid FundingAccountId { get; init; }
}