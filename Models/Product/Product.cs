using BankingConsole.Models.Enums;

namespace BankingConsole.Models.Product;

public class Product
{
    public Guid ProductId { get; protected set; }
    public string ProductCode { get; protected set; } = null!;
    public string ProductName { get; protected set; } = null!;
    public Currency Currency { get; protected set; }
    public CustomerType AllowedCustomerType { get; protected set; }
    public decimal MinimumAmount { get; protected set; }
    public ProductType ProductType { get; protected set; }

    public bool DebitInterestCalculation { get; protected set; }
    public decimal? DebitInterestRate { get; protected set; }
    public Frequency? DebitCalculationFrequency { get; protected set; }
    public Frequency? DebitPostingFrequency { get; protected set; }
    public bool CreditInterestCalculation { get; protected set; }
    public decimal? CreditInterestRate { get; protected set; }
    public Frequency? CreditCalculationFrequency { get; protected set; }
    public Frequency? CreditPostingFrequency { get; protected set; }
    public decimal? TaxPercentage { get; protected set; }

    public int WithdrawalLimitCount { get; protected set; }
    public Frequency? WithdrawalLimitFrequency { get; protected set; }
    public decimal WithdrawalLimitAmount { get; protected set; }
    public Frequency? WithdrawalLimitAmountFrequency { get; protected set; }

    protected Product()
    {
    }

    public static Product Create(
        string productCode,
        string productName,
        CustomerType allowedCustomerType,
        Currency currency,
        decimal minimumAmount = 0,
        bool debitInterestCalculation = false,
        decimal? debitInterestRate = null,
        Frequency? debitCalculationFrequency = null,
        Frequency? debitPostingFrequency = null,
        bool creditInterestCalculation = false,
        decimal? creditInterestRate = null,
        Frequency? creditCalculationFrequency = null,
        Frequency? creditPostingFrequency = null,
        int withdrawalLimitCount = 0,
        Frequency? withdrawalLimitFrequency = null,
        decimal withdrawalLimitAmount = 0,
        Frequency? withdrawalLimitAmountFrequency = null,
        decimal? taxPercentage = null)
    {
        return Populate(
            new Product(),
            productCode,
            productName,
            allowedCustomerType,
            currency,
            minimumAmount,
            ProductType.SAVING,
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
    }

    protected static void Validate(
        string productCode,
        string productName,
        decimal minimumAmount,
        bool debitInterestCalculation,
        decimal? debitInterestRate,
        Frequency? debitCalculationFrequency,
        Frequency? debitPostingFrequency,
        bool creditInterestCalculation,
        decimal? creditInterestRate,
        Frequency? creditCalculationFrequency,
        Frequency? creditPostingFrequency,
        int withdrawalLimitCount,
        Frequency? withdrawalLimitFrequency,
        decimal withdrawalLimitAmount,
        Frequency? withdrawalLimitAmountFrequency,
        decimal? taxPercentage)
    {
        if (string.IsNullOrWhiteSpace(productCode))
            throw new ArgumentException("Product code is required.", nameof(productCode));

        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name is required.", nameof(productName));

        if (minimumAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(minimumAmount));

        ValidateInterestConfiguration(
            debitInterestCalculation,
            debitInterestRate,
            debitCalculationFrequency,
            debitPostingFrequency,
            "debit");

        ValidateInterestConfiguration(
            creditInterestCalculation,
            creditInterestRate,
            creditCalculationFrequency,
            creditPostingFrequency,
            "credit");

        if (withdrawalLimitCount < 0)
            throw new ArgumentOutOfRangeException(nameof(withdrawalLimitCount));

        if ((withdrawalLimitCount > 0) != withdrawalLimitFrequency.HasValue)
            throw new ArgumentException(
                "Withdrawal limit count and frequency must be supplied together.");

        if (withdrawalLimitAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(withdrawalLimitAmount));

        if ((withdrawalLimitAmount > 0) != withdrawalLimitAmountFrequency.HasValue)
            throw new ArgumentException(
                "Withdrawal limit amount and amount frequency must be supplied together.");

        if (creditPostingFrequency.HasValue && !taxPercentage.HasValue)
            throw new ArgumentException(
                "Tax percentage is required when credit interest posting frequency is supplied.",
                nameof(taxPercentage));

        if (taxPercentage is < 0 or > 100)
            throw new ArgumentOutOfRangeException(
                nameof(taxPercentage),
                "Tax percentage must be between 0 and 100.");
    }

    private static void ValidateInterestConfiguration(
        bool calculationEnabled,
        decimal? rate,
        Frequency? calculationFrequency,
        Frequency? postingFrequency,
        string interestType)
    {
        if (calculationEnabled &&
            (!rate.HasValue ||
             !calculationFrequency.HasValue ||
             !postingFrequency.HasValue))
        {
            throw new ArgumentException(
                $"{interestType} interest rate, calculation frequency, and posting frequency " +
                $"are required when {interestType} interest calculation is enabled.");
        }

        if (rate < 0)
            throw new ArgumentOutOfRangeException($"{interestType}InterestRate");

    }

    protected static T Populate<T>(
        T product,
        string productCode,
        string productName,
        CustomerType allowedCustomerType,
        Currency currency,
        decimal minimumAmount = 0,
        ProductType productType = ProductType.SAVING,
        bool debitInterestCalculation = false,
        decimal? debitInterestRate = null,
        Frequency? debitCalculationFrequency = null,
        Frequency? debitPostingFrequency = null,
        bool creditInterestCalculation = false,
        decimal? creditInterestRate = null,
        Frequency? creditCalculationFrequency = null,
        Frequency? creditPostingFrequency = null,
        int withdrawalLimitCount = 0,
        Frequency? withdrawalLimitFrequency = null,
        decimal withdrawalLimitAmount = 0,
        Frequency? withdrawalLimitAmountFrequency = null,
        decimal? taxPercentage = null)
        where T : Product
    {
        ArgumentNullException.ThrowIfNull(product);

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

        product.ProductId = Guid.NewGuid();
        product.ProductCode = productCode.Trim().ToUpperInvariant();
        product.ProductName = productName.Trim();
        product.Currency = currency;
        product.AllowedCustomerType = allowedCustomerType;
        product.MinimumAmount = minimumAmount;
        product.ProductType = productType;
        product.DebitInterestCalculation = debitInterestCalculation;
        product.DebitInterestRate = debitInterestCalculation ? debitInterestRate : null;
        product.DebitCalculationFrequency =
            debitInterestCalculation ? debitCalculationFrequency : null;
        product.DebitPostingFrequency =
            debitInterestCalculation ? debitPostingFrequency : null;
        product.CreditInterestCalculation = creditInterestCalculation;
        product.CreditInterestRate = creditInterestCalculation ? creditInterestRate : null;
        product.CreditCalculationFrequency =
            creditInterestCalculation ? creditCalculationFrequency : null;
        product.CreditPostingFrequency =
            creditInterestCalculation ? creditPostingFrequency : null;
        product.WithdrawalLimitCount = withdrawalLimitCount;
        product.WithdrawalLimitFrequency = withdrawalLimitFrequency;
        product.WithdrawalLimitAmount = withdrawalLimitAmount;
        product.WithdrawalLimitAmountFrequency = withdrawalLimitAmountFrequency;
        product.TaxPercentage = taxPercentage;

        return product;
    }
}
