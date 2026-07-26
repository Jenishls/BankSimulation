using BankingConsole.Models.Enums;

namespace BankingConsole.Models.Product;

public sealed class TermProduct : Product
{
    public int TenureInDays { get; private set; }
    public int TransferCount { get; private set; }
    public Frequency TransferFrequency { get; private set; }

    public bool AllowPrematureWithdrawal {get; private set;}

    private TermProduct(){}

    public static TermProduct Create(
        string productCode,
        string productName,
        CustomerType allowedCustomerType,
        Currency currency,
        int tenureInDays,
        int transferCount,
        Frequency transferFrequency,
        decimal minimumAmount = 0,
        bool debitInterestCalculation = false,
        decimal? debitInterestRate = null,
        Frequency? debitCalculationFrequency = null,
        Frequency? debitPostingFrequency = null,
        bool creditInterestCalculation = true,
        decimal? creditInterestRate = null,
        Frequency? creditCalculationFrequency = null,
        Frequency? creditPostingFrequency = null,
        int withdrawalLimitCount = 0,
        Frequency? withdrawalLimitFrequency = null,
        decimal withdrawalLimitAmount = 0,
        Frequency? withdrawalLimitAmountFrequency = null,
        decimal? taxPercentage = null)
    {
        Validate(
            productCode,
            productName,
            minimumAmount,
            debitInterestCalculation,
            debitInterestRate,
            debitCalculationFrequency,
            debitPostingFrequency,
            creditInterestCalculation,
            creditInterestRate,
            creditCalculationFrequency,
            creditPostingFrequency,
            withdrawalLimitCount,
            withdrawalLimitFrequency,
            withdrawalLimitAmount,
            withdrawalLimitAmountFrequency,
            taxPercentage);

        if (tenureInDays <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(tenureInDays),
                "Term-product tenure must be greater than zero.");

        if (transferCount <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(transferCount),
                "Transfer count must be greater than zero.");

        return new TermProduct
        {
            ProductId = Guid.NewGuid(),
            ProductCode = productCode.Trim().ToUpperInvariant(),
            ProductName = productName.Trim(),
            AllowedCustomerType = allowedCustomerType,
            Currency = currency,
            MinimumAmount = minimumAmount,
            ProductType = ProductType.TERM,
            DebitInterestCalculation = debitInterestCalculation,
            DebitInterestRate = debitInterestCalculation
                ? debitInterestRate
                : null,
            DebitCalculationFrequency = debitInterestCalculation
                ? debitCalculationFrequency
                : null,
            DebitPostingFrequency = debitInterestCalculation
                ? debitPostingFrequency
                : null,
            CreditInterestCalculation = creditInterestCalculation,
            CreditInterestRate = creditInterestCalculation
                ? creditInterestRate
                : null,
            CreditCalculationFrequency = creditInterestCalculation
                ? creditCalculationFrequency
                : null,
            CreditPostingFrequency = creditInterestCalculation
                ? creditPostingFrequency
                : null,
            WithdrawalLimitCount = withdrawalLimitCount,
            WithdrawalLimitFrequency = withdrawalLimitFrequency,
            WithdrawalLimitAmount = withdrawalLimitAmount,
            WithdrawalLimitAmountFrequency =
                withdrawalLimitAmountFrequency,
            TaxPercentage = taxPercentage,
            TenureInDays = tenureInDays,
            TransferCount = transferCount,
            TransferFrequency = transferFrequency
        };
    }
}
