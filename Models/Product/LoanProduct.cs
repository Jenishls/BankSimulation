using BankingConsole.Models.Enums;

namespace BankingConsole.Models.Product;

public sealed class LoanProduct : Product
{
    public override ProductType ProductType => ProductType.LOAN;

    public int TenureInDays { get; private set; }
    public int RepaymentCount { get; private set; }
    public Frequency RepaymentFrequency { get; private set; }
    public decimal? PenaltyInterestRate { get; private set; }
    public DateTime InterestPostDate {get; private set;}
    private LoanProduct()
    {
    }

    private LoanProduct(
        string productCode,
        string productName,
        CustomerType allowedCustomerType,
        Currency currency,
        decimal interestRate,
        IEnumerable<InterestPostPolicy> interestPostPolicies,
        Frequency? interestPostingFrequency,
        int tenureInDays,
        int repaymentCount,
        Frequency repaymentFrequency,
        decimal? penaltyInterestRate,
        decimal minimumAmount,
        decimal? taxPercentage,
        int withdrawalLimitCount,
        Frequency? withdrawalLimitFrequency,
        decimal withdrawalLimitAmount,
        DateTime interestPostDate,
        Frequency? withdrawalLimitAmountFrequency)
        : base(
            productCode,
            productName,
            currency,
            allowedCustomerType,
            Flow.DEBIT,
            interestRate,
            interestPostPolicies,
            interestPostingFrequency,
            minimumAmount,
            taxPercentage,
            withdrawalLimitCount,
            withdrawalLimitFrequency,
            withdrawalLimitAmount,
            withdrawalLimitAmountFrequency)
    {
        TenureInDays = tenureInDays;
        RepaymentCount = repaymentCount;
        RepaymentFrequency = repaymentFrequency;
        PenaltyInterestRate = penaltyInterestRate;
        InterestPostDate = interestPostDate;
    }

    public static LoanProduct Create(
        string productCode,
        string productName,
        CustomerType allowedCustomerType,
        Currency currency,
        decimal interestRate,
        IEnumerable<InterestPostPolicy> interestPostPolicies,
        Frequency? interestPostingFrequency,
        int tenureInDays,
        int repaymentCount,
        Frequency repaymentFrequency,
        DateTime interestPostDate,
        decimal? penaltyInterestRate = null,
        decimal minimumAmount = 0,
        decimal? taxPercentage = null,
        int withdrawalLimitCount = 0,
        Frequency? withdrawalLimitFrequency = null,
        decimal withdrawalLimitAmount = 0,
        Frequency? withdrawalLimitAmountFrequency = null
        )
    {
        if (tenureInDays <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(tenureInDays),
                "Loan tenure must be greater than zero.");
        }

        if (repaymentCount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(repaymentCount),
                "Repayment count must be greater than zero.");
        }

        if (penaltyInterestRate < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(penaltyInterestRate));
        }

        return new LoanProduct(
            productCode,
            productName,
            allowedCustomerType,
            currency,
            interestRate,
            interestPostPolicies,
            interestPostingFrequency,
            tenureInDays,
            repaymentCount,
            repaymentFrequency,
            penaltyInterestRate,
            minimumAmount,
            taxPercentage,
            withdrawalLimitCount,
            withdrawalLimitFrequency,
            withdrawalLimitAmount,
            interestPostDate,
            withdrawalLimitAmountFrequency
            );
    }
}
