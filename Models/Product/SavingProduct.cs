using BankingConsole.Models.Enums;

namespace BankingConsole.Models.Product;

public sealed class SavingProduct : Product
{
    public override ProductType ProductType => ProductType.SAVING;

    private SavingProduct()
    {
    }

    private SavingProduct(
        string productCode,
        string productName,
        CustomerType allowedCustomerType,
        Currency currency,
        decimal interestRate,
        IEnumerable<InterestPostPolicy> interestPostPolicies,
        Frequency? interestPostingFrequency,
        decimal minimumAmount,
        decimal? taxPercentage,
        int withdrawalLimitCount,
        Frequency? withdrawalLimitFrequency,
        decimal withdrawalLimitAmount,
        Frequency? withdrawalLimitAmountFrequency)
        : base(
            productCode,
            productName,
            currency,
            allowedCustomerType,
            Flow.CREDIT,
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
    }

    public static SavingProduct Create(
        string productCode,
        string productName,
        CustomerType allowedCustomerType,
        Currency currency,
        decimal interestRate,
        IEnumerable<InterestPostPolicy> interestPostPolicies,
        Frequency? interestPostingFrequency = null,
        decimal minimumAmount = 0,
        decimal? taxPercentage = null,
        int withdrawalLimitCount = 0,
        Frequency? withdrawalLimitFrequency = null,
        decimal withdrawalLimitAmount = 0,
        Frequency? withdrawalLimitAmountFrequency = null)
    {
        return new SavingProduct(
            productCode,
            productName,
            allowedCustomerType,
            currency,
            interestRate,
            interestPostPolicies,
            interestPostingFrequency,
            minimumAmount,
            taxPercentage,
            withdrawalLimitCount,
            withdrawalLimitFrequency,
            withdrawalLimitAmount,
            withdrawalLimitAmountFrequency);
    }
}
