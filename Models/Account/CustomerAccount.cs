using BankingConsole.Models.Enums;

namespace BankingConsole.Models.Account;

public sealed class CustomerAccount : Account
{
    public Guid CustomerId { get; private set; }
    public Guid ProductId { get; private set; }
    public ProductType ProductType { get; private set; }
    public decimal InterestAccured { get; private set; }
    public DateTime InterestPostedOn { get; private set; }
    public decimal? Principal { get; private set; }
    public decimal? OutstandingPrincipal { get; private set; }
    public DateTime? MaturityDate { get; private set; }
    public Guid? FundingAccountId { get; private set; }
    public Guid? MaturitySettlementAccountId { get; private set; }
    public Guid? DisbursementAccountId { get; private set; }
    public Guid? RepaymentAccountId { get; private set; }
    public int? RepaymentInstallments { get; private set; }

    private CustomerAccount()
    {
    }

    private CustomerAccount(
        string accountNumber,
        string name,
        string branchCode,
        Guid customerId,
        Guid productId,
        ProductType productType,
        AccountState state,
        DateTime accountOpenDate,
        decimal balance,
        decimal? principal,
        decimal? outstandingPrincipal,
        DateTime? maturityDate,
        Guid? fundingAccountId,
        Guid? maturitySettlementAccountId,
        Guid? disbursementAccountId,
        Guid? repaymentAccountId,
        int? repaymentInstallments)
        : base(
            accountNumber,
            name,
            branchCode,
            balance,
            state,
            accountOpenDate,
            AccountType.CUSTOMER)
    {
        if (customerId == Guid.Empty)
        {
            throw new ArgumentException(
                "Customer id is required.",
                nameof(customerId));
        }

        if (productId == Guid.Empty)
        {
            throw new ArgumentException(
                "Product id is required.",
                nameof(productId));
        }

        CustomerId = customerId;
        ProductId = productId;
        ProductType = productType;
        InterestAccured = 0;
        InterestPostedOn = accountOpenDate;
        Principal = principal;
        OutstandingPrincipal = outstandingPrincipal;
        MaturityDate = maturityDate;
        FundingAccountId = fundingAccountId;
        MaturitySettlementAccountId = maturitySettlementAccountId;
        DisbursementAccountId = disbursementAccountId;
        RepaymentAccountId = repaymentAccountId;
        RepaymentInstallments = repaymentInstallments;
    }

    public static CustomerAccount Create(
        string accountNumber,
        string name,
        Guid customerId,
        Guid productId,
        ProductType productType,
        string branchCode,
        decimal openingBalance = 0,
        decimal? principal = null,
        int? tenureInDays = null,
        Guid? fundingAccountId = null,
        Guid? maturitySettlementAccountId = null,
        Guid? disbursementAccountId = null,
        Guid? repaymentAccountId = null,
        int? repaymentInstallments = null)
    {
        if (openingBalance < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(openingBalance),
                "A customer account cannot be opened with a negative balance.");
        }

        var accountOpenDate = DateTime.UtcNow;
        var balance = openingBalance;
        decimal? outstandingPrincipal = null;
        DateTime? maturityDate = null;

        switch (productType)
        {
            case ProductType.SAVING:
                if (principal.HasValue ||
                    tenureInDays.HasValue ||
                    fundingAccountId.HasValue ||
                    maturitySettlementAccountId.HasValue ||
                    disbursementAccountId.HasValue ||
                    repaymentAccountId.HasValue ||
                    repaymentInstallments.HasValue)
                {
                    throw new ArgumentException(
                        "Savings accounts cannot have fixed-deposit or loan terms.");
                }
                break;

            case ProductType.TERM:
                ValidatePrincipal(principal);
                ValidateTenure(tenureInDays);
                ValidateLinkedAccount(fundingAccountId, nameof(fundingAccountId));
                ValidateLinkedAccount(
                    maturitySettlementAccountId,
                    nameof(maturitySettlementAccountId));

                if (disbursementAccountId.HasValue ||
                    repaymentAccountId.HasValue ||
                    repaymentInstallments.HasValue)
                {
                    throw new ArgumentException(
                        "Fixed-deposit accounts cannot have loan settlement details.");
                }

                balance = principal.GetValueOrDefault();
                maturityDate = accountOpenDate.AddDays(
                    tenureInDays.GetValueOrDefault());
                break;

            case ProductType.LOAN:
                ValidatePrincipal(principal);
                ValidateTenure(tenureInDays);
                ValidateLinkedAccount(
                    disbursementAccountId,
                    nameof(disbursementAccountId));
                ValidateLinkedAccount(
                    repaymentAccountId,
                    nameof(repaymentAccountId));

                if (fundingAccountId.HasValue ||
                    maturitySettlementAccountId.HasValue)
                {
                    throw new ArgumentException(
                        "Loan accounts cannot have fixed-deposit settlement details.");
                }

                if (repaymentInstallments is null or <= 0)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(repaymentInstallments),
                        "A loan requires at least one repayment installment.");
                }

                balance = principal.GetValueOrDefault();
                outstandingPrincipal = principal;
                maturityDate = accountOpenDate.AddDays(
                    tenureInDays.GetValueOrDefault());
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(productType),
                    productType,
                    "Unsupported product type.");
        }

        return new CustomerAccount(
            accountNumber,
            name,
            branchCode,
            customerId,
            productId,
            productType,
            AccountState.ACTIVE,
            accountOpenDate,
            balance,
            principal,
            outstandingPrincipal,
            maturityDate,
            fundingAccountId,
            maturitySettlementAccountId,
            disbursementAccountId,
            repaymentAccountId,
            repaymentInstallments);
    }

    public void IncreaseInterestAccured(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        InterestAccured += amount;
    }

    public void MarkInterestPosted(DateTime postedOn)
    {
        if (InterestAccured <= 0)
        {
            throw new InvalidOperationException(
                "There is no accrued interest to post.");
        }

        if (postedOn < InterestPostedOn)
        {
            throw new ArgumentOutOfRangeException(
                nameof(postedOn),
                "Interest cannot be posted before the previous posting date.");
        }

        InterestAccured = 0;
        InterestPostedOn = postedOn;
    }

    private static void ValidatePrincipal(decimal? principal)
    {
        if (principal is null or <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(principal),
                "Principal must be greater than zero.");
        }
    }

    private static void ValidateTenure(int? tenureInDays)
    {
        if (tenureInDays is null or <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(tenureInDays),
                "A term or loan account requires a positive tenure.");
        }
    }

    private static void ValidateLinkedAccount(
        Guid? accountId,
        string parameterName)
    {
        if (accountId is null || accountId == Guid.Empty)
        {
            throw new ArgumentException(
                "A linked customer account is required.",
                parameterName);
        }
    }
}
