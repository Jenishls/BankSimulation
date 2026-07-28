using BankingConsole.Models.Enums;
using BankingConsole.Models.Product;
using BankingConsole.Models.ProductCreation;

namespace BankingConsole.Factories;

public sealed class ProductFactory : IProductFactory
{
    public Product Create(ProductCreationData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        ValidateCommonData(data);
        return data.ProductType switch
        {
            ProductType.SAVING => CreateSaving(data),
            ProductType.LOAN => CreateLoan(data),
            ProductType.TERM => CreateTerm(data),
            ProductType.OFFICE => CreateOffice(data),
            _ => throw new ArgumentOutOfRangeException(
                nameof(data.ProductType),
                data.ProductType,
                "Unsupported type of Product")
        };
    }

    private static void ValidateCommonData(ProductCreationData data)
    {
        if (string.IsNullOrWhiteSpace(data.ProductCode))
            throw new ArgumentException(
                "Product code is required.",
                nameof(data.ProductCode));

        if (string.IsNullOrWhiteSpace(data.ProductName))
            throw new ArgumentException(
                "Product name is required.",
                nameof(data.ProductName));

        if (data.MinimumAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(data.MinimumAmount));
    }

    private static SavingProduct CreateSaving(ProductCreationData data)
    {
        return SavingProduct.Create(
            productCode: data.ProductCode,
            productName: data.ProductName,
            allowedCustomerType: data.AllowedCustomerType,
            currency: data.Currency,
            interestRate: data.InterestRate,
            interestPostPolicies: data.InterestPostPolicies,
            interestPostingFrequency: data.InterestPostingFrequency,
            minimumAmount: data.MinimumAmount,
            taxPercentage: data.TaxPercentage,
            withdrawalLimitCount: data.WithdrawalLimitCount,
            withdrawalLimitFrequency: data.WithdrawalLimitFrequency,
            withdrawalLimitAmount: data.WithdrawalLimitAmount,
            withdrawalLimitAmountFrequency: data.WithdrawalLimitAmountFrequency);
    }

    private static TermProduct CreateTerm(ProductCreationData data)
    {
        return TermProduct.Create(
            productCode: data.ProductCode,
            productName: data.ProductName,
            allowedCustomerType: data.AllowedCustomerType,
            currency: data.Currency,
            interestRate: data.InterestRate,
            interestPostPolicies: data.InterestPostPolicies,
            interestPostingFrequency: data.InterestPostingFrequency,
            tenureInDays: Required(data.TenureInDays, nameof(data.TenureInDays)),
            transferCount: Required(data.TransferCount, nameof(data.TransferCount)),
            transferFrequency: Required(
                data.TransferFrequency,
                nameof(data.TransferFrequency)),
            allowPrematureWithdrawal: data.AllowPrematureWithdrawal,
            minimumAmount: data.MinimumAmount,
            taxPercentage: data.TaxPercentage,
            withdrawalLimitCount: data.WithdrawalLimitCount,
            withdrawalLimitFrequency: data.WithdrawalLimitFrequency,
            withdrawalLimitAmount: data.WithdrawalLimitAmount,
            withdrawalLimitAmountFrequency: data.WithdrawalLimitAmountFrequency);
    }

    private static LoanProduct CreateLoan(ProductCreationData data)
    {
        return LoanProduct.Create(
            productCode: data.ProductCode,
            productName: data.ProductName,
            allowedCustomerType: data.AllowedCustomerType,
            currency: data.Currency,
            interestRate: data.InterestRate,
            interestPostPolicies: data.InterestPostPolicies,
            interestPostingFrequency: data.InterestPostingFrequency,
            tenureInDays: Required(data.TenureInDays, nameof(data.TenureInDays)),
            repaymentCount: Required(
                data.RepaymentCount,
                nameof(data.RepaymentCount)),
            repaymentFrequency: Required(
                data.RepaymentFrequency,
                nameof(data.RepaymentFrequency)),
            penaltyInterestRate: data.PenaltyInterestRate,
            minimumAmount: data.MinimumAmount,
            taxPercentage: data.TaxPercentage,
            withdrawalLimitCount: data.WithdrawalLimitCount,
            withdrawalLimitFrequency: data.WithdrawalLimitFrequency,
            withdrawalLimitAmount: data.WithdrawalLimitAmount,
            withdrawalLimitAmountFrequency: data.WithdrawalLimitAmountFrequency);
    }

    private static OfficeProduct CreateOffice(ProductCreationData data)
    {
        return OfficeProduct.Create(
            productCode: data.ProductCode,
            productName: data.ProductName,
            currency: data.Currency,
            interestFlow: Required(
                data.OfficeInterestFlow,
                nameof(data.OfficeInterestFlow)),
            interestRate: data.InterestRate,
            interestPostPolicies: data.InterestPostPolicies,
            interestPostingFrequency: data.InterestPostingFrequency,
            minimumAmount: data.MinimumAmount,
            taxPercentage: data.TaxPercentage,
            withdrawalLimitCount: data.WithdrawalLimitCount,
            withdrawalLimitFrequency: data.WithdrawalLimitFrequency,
            withdrawalLimitAmount: data.WithdrawalLimitAmount,
            withdrawalLimitAmountFrequency: data.WithdrawalLimitAmountFrequency);
    }

    private static T Required<T>(T? value, string parameterName)
        where T : struct
    {
        return value ?? throw new ArgumentException(
            $"{parameterName} is required for this product type.",
            parameterName);
    }
}
