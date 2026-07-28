using BankingConsole.Models.Enums;

namespace BankingConsole.Models.Product;

public sealed class OfficeProduct : Product
{
    public override ProductType ProductType => ProductType.OFFICE;

    private OfficeProduct()
    {
    }

    private OfficeProduct(
        string productCode,
        string productName,
        Currency currency,
        Flow interestFlow,
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
            CustomerType.ALL,
            interestFlow,
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

    public static OfficeProduct Create(
        string productCode,
        string productName,
        Currency currency,
        Flow interestFlow,
        decimal interestRate = 0,
        IEnumerable<InterestPostPolicy>? interestPostPolicies = null,
        Frequency? interestPostingFrequency = null,
        decimal minimumAmount = 0,
        decimal? taxPercentage = null,
        int withdrawalLimitCount = 0,
        Frequency? withdrawalLimitFrequency = null,
        decimal withdrawalLimitAmount = 0,
        Frequency? withdrawalLimitAmountFrequency = null)
    {
        return new OfficeProduct(
            productCode,
            productName,
            currency,
            interestFlow,
            interestRate,
            interestPostPolicies ?? [],
            interestPostingFrequency,
            minimumAmount,
            taxPercentage,
            withdrawalLimitCount,
            withdrawalLimitFrequency,
            withdrawalLimitAmount,
            withdrawalLimitAmountFrequency);
    }
}
