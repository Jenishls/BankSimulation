using BankingConsole.Models.Account;
using BankingConsole.Models.Enums;

namespace BankingConsole.Models.Product;

public abstract class Product
{
    public Guid ProductId { get; private set; }
    public string ProductCode { get; protected set; } = null!;
    public string ProductName { get; protected set; } = null!;
    public Currency Currency { get; protected set; }
    public CustomerType AllowedCustomerType { get; protected set; }
    public decimal MinimumAmount { get; protected set; }

    public Flow InterestFlow { get; protected set; }
    public decimal InterestRate { get; protected set; } = 0m;
    public List<InterestPostPolicy> InterestPostPolicies { get; protected set; } = [];
    public Frequency? InterestPostingFrequency { get; protected set; }
    public OfficeAccount? InterestAccount {get; protected set;}

    public decimal? TaxPercentage { get; protected set; }
    public OfficeAccount? TaxAccount {get; protected set;}

    public int WithdrawalLimitCount { get; protected set; }
    public Frequency? WithdrawalLimitFrequency { get; protected set; }
    public decimal WithdrawalLimitAmount { get; protected set; }
    public Frequency? WithdrawalLimitAmountFrequency { get; protected set; }

    public abstract ProductType ProductType { get; }

    protected Product()
    {
    }

    protected Product(
        string productCode,
        string productName,
        Currency currency,
        CustomerType allowedCustomerType,
        Flow interestFlow,
        decimal interestRate,
        IEnumerable<InterestPostPolicy> interestPostPolicies,
        Frequency? interestPostingFrequency,
        decimal minimumAmount = 0,
        decimal? taxPercentage = null,
        int withdrawalLimitCount = 0,
        Frequency? withdrawalLimitFrequency = null,
        decimal withdrawalLimitAmount = 0,
        Frequency? withdrawalLimitAmountFrequency = null)
    {
        if (string.IsNullOrWhiteSpace(productCode))
        {
            throw new ArgumentException(
                "Product code is required.",
                nameof(productCode));
        }

        if (string.IsNullOrWhiteSpace(productName))
        {
            throw new ArgumentException(
                "Product name is required.",
                nameof(productName));
        }

        if (minimumAmount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minimumAmount));
        }

        if (interestRate < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(interestRate));
        }

        ArgumentNullException.ThrowIfNull(interestPostPolicies);

        var postingPolicies = interestPostPolicies
            .Distinct()
            .ToList();

        if (interestRate > 0 && postingPolicies.Count is 0)
        {
            throw new ArgumentException(
                "At least one posting policy is required when interest is configured.",
                nameof(interestPostPolicies));
        }

        var postsByFrequency = postingPolicies.Contains(
            InterestPostPolicy.LastDayOfFrequency);

        if (postsByFrequency != interestPostingFrequency.HasValue)
        {
            throw new ArgumentException(
                "Posting frequency must be supplied only when the last-day-of-frequency policy is configured.",
                nameof(interestPostingFrequency));
        }

        if (withdrawalLimitCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(withdrawalLimitCount));
        }

        if ((withdrawalLimitCount > 0) != withdrawalLimitFrequency.HasValue)
        {
            throw new ArgumentException(
                "Withdrawal limit count and frequency must be supplied together.");
        }

        if (withdrawalLimitAmount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(withdrawalLimitAmount));
        }

        if ((withdrawalLimitAmount > 0) != withdrawalLimitAmountFrequency.HasValue)
        {
            throw new ArgumentException(
                "Withdrawal limit amount and amount frequency must be supplied together.");
        }

        if (taxPercentage is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(taxPercentage),
                "Tax percentage must be between 0 and 100.");
        }

        ProductId = Guid.NewGuid();
        ProductCode = productCode.Trim().ToUpperInvariant();
        ProductName = productName.Trim();
        Currency = currency;
        AllowedCustomerType = allowedCustomerType;
        MinimumAmount = minimumAmount;
        InterestFlow = interestFlow;
        InterestRate = interestRate;
        InterestPostPolicies = postingPolicies;
        InterestPostingFrequency = interestPostingFrequency;
        TaxPercentage = taxPercentage;
        WithdrawalLimitCount = withdrawalLimitCount;
        WithdrawalLimitFrequency = withdrawalLimitFrequency;
        WithdrawalLimitAmount = withdrawalLimitAmount;
        WithdrawalLimitAmountFrequency = withdrawalLimitAmountFrequency;
    }
}
