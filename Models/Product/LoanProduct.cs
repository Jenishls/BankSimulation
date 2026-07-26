using BankingConsole.Models.Enums;

namespace BankingConsole.Models.Product;

public sealed class LoanProduct : Product
{
    public int TenureInDays { get; private set; }
    public int RepaymentCount { get; private set; }
    public Frequency RepaymentFrequency { get; private set; }
    public decimal? PenaltyInterestRate { get; private set; }

    private LoanProduct()
    {
    }

    public static LoanProduct Create(
        string productCode,
        string productName,
        CustomerType allowedCustomerType,
        Currency currency,
        decimal debitInterestRate,
        Frequency debitCalculationFrequency,
        Frequency debitPostingFrequency,
        int tenureInDays,
        int repaymentCount,
        Frequency repaymentFrequency,
        decimal minimumAmount = 0,
        decimal? penaltyInterestRate = null,
        bool creditInterestCalculation = false,
        decimal? creditInterestRate = null,
        Frequency? creditCalculationFrequency = null,
        Frequency? creditPostingFrequency = null,
        decimal? taxPercentage = null)
    {
        if (tenureInDays <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(tenureInDays),
                "Loan tenure must be greater than zero.");

        if (repaymentCount <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(repaymentCount),
                "Repayment count must be greater than zero.");

        if (penaltyInterestRate < 0)
            throw new ArgumentOutOfRangeException(nameof(penaltyInterestRate));

        var product = Populate(
            product: new LoanProduct(),
            productCode: productCode,
            productName: productName,
            allowedCustomerType: allowedCustomerType,
            currency: currency,
            minimumAmount: minimumAmount,
            productType: ProductType.LOAN,
            debitInterestCalculation: true,
            debitInterestRate: debitInterestRate,
            debitCalculationFrequency: debitCalculationFrequency,
            debitPostingFrequency: debitPostingFrequency,
            creditInterestCalculation: creditInterestCalculation,
            creditInterestRate: creditInterestRate,
            creditCalculationFrequency: creditCalculationFrequency,
            creditPostingFrequency: creditPostingFrequency,
            withdrawalLimitCount: 0,
            withdrawalLimitFrequency: null,
            withdrawalLimitAmount: 0,
            withdrawalLimitAmountFrequency: null,
            taxPercentage: taxPercentage);

        product.TenureInDays = tenureInDays;
        product.RepaymentCount = repaymentCount;
        product.RepaymentFrequency = repaymentFrequency;
        product.PenaltyInterestRate = penaltyInterestRate;

        return product;
    }
}
