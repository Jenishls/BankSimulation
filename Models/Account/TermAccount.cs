using BankingConsole.Models.Enums;

namespace BankingConsole.Models.Account;

public sealed class TermAccount : Account
{
    public decimal Principal { get; private set; }
    public DateTime MaturityDate { get; private set; }
    public Guid FundingAccountId { get; private set; }

    private TermAccount()
    {
    }

    private TermAccount(
        string accountNumber,
        Guid customerId,
        Guid productId,
        string branchCode,
        DateTime accountOpenDate,
        decimal principal,
        DateTime maturityDate,
        Guid fundingAccountId)
        : base(
            accountNumber,
            customerId,
            productId,
            branchCode,
            balance: 0,
            accountState: AccountState.ACTIVE,
            accountOpenDate: accountOpenDate,
            interestAccrued: null,
            interestPostedOn: null)
    {
        Principal = principal;
        MaturityDate = maturityDate;
        FundingAccountId = fundingAccountId;
    }

    public static TermAccount Create(
        string accountNumber,
        Guid customerId,
        Guid productId,
        string branchCode,
        decimal principal,
        DateTime maturityDate,
        Guid fundingAccountId)
    {
        if (principal <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(principal),
                "Term-account principal must be greater than zero.");

        var accountOpenDate = DateTime.UtcNow;

        if (maturityDate <= accountOpenDate)
            throw new ArgumentOutOfRangeException(
                nameof(maturityDate),
                "Maturity date must be after the account opening date.");

        if (fundingAccountId == Guid.Empty)
            throw new ArgumentException(
                "Funding account id is required.",
                nameof(fundingAccountId));

        return new TermAccount(
            accountNumber,
            customerId,
            productId,
            branchCode,
            accountOpenDate,
            principal,
            maturityDate,
            fundingAccountId);
    }
}
