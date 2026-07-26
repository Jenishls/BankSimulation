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

        if (data.MinimunAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(data.MinimunAmount));
    }

    private static Product CreateSaving(ProductCreationData data)
    {
        return Product.Create(
            productCode: data.ProductCode,
            productName: data.ProductName,
            allowedCustomerType: data.AllowedCustomerType,
            currency: data.Currency,
            minimumAmount: data.MinimunAmount,
            debitInterestCalculation: data.DebitInterestCalculation,
            debitInterestRate: data.DebitInterestRate,
            debitCalculationFrequency: data.DebitCalculationFrequency,
            debitPostingFrequency: data.DebitPostingFrequency,
            creditInterestCalculation: data.CreditInterestCalculation,
            creditInterestRate: data.CreditInterestRate,
            creditCalculationFrequency: data.CreditCalculationFrequency,
            creditPostingFrequency: data.CreditPostingFrequency,
            withdrawalLimitCount: data.WithdrawalLimitCount,
            withdrawalLimitFrequency: data.WithdrawalLimitFrequency,
            withdrawalLimitAmount: data.WithdrawalLimitAmount,
            withdrawalLimitAmountFrequency: data.WithdrawalLimitAmountFrequency,
            taxPercentage: data.TaxPercentage);
    }

    private static TermProduct CreateTerm(ProductCreationData data)
    {
        return TermProduct.Create(
            productCode: data.ProductCode,
            productName: data.ProductName,
            allowedCustomerType: data.AllowedCustomerType,
            currency: data.Currency,
            tenureInDays: Required(data.TenureInDays, nameof(data.TenureInDays)),
            transferCount: Required(data.TransferCount, nameof(data.TransferCount)),
            transferFrequency: Required(
                data.TransferFrequency,
                nameof(data.TransferFrequency)),
            minimumAmount: data.MinimunAmount,
            debitInterestCalculation: data.DebitInterestCalculation,
            debitInterestRate: data.DebitInterestRate,
            debitCalculationFrequency: data.DebitCalculationFrequency,
            debitPostingFrequency: data.DebitPostingFrequency,
            creditInterestCalculation: data.CreditInterestCalculation,
            creditInterestRate: data.CreditInterestRate,
            creditCalculationFrequency: data.CreditCalculationFrequency,
            creditPostingFrequency: data.CreditPostingFrequency,
            withdrawalLimitCount: data.WithdrawalLimitCount,
            withdrawalLimitFrequency: data.WithdrawalLimitFrequency,
            withdrawalLimitAmount: data.WithdrawalLimitAmount,
            withdrawalLimitAmountFrequency: data.WithdrawalLimitAmountFrequency,
            taxPercentage: data.TaxPercentage);
    }

    private static LoanProduct CreateLoan(ProductCreationData data)
    {
        return LoanProduct.Create(
            productCode: data.ProductCode,
            productName: data.ProductName,
            allowedCustomerType: data.AllowedCustomerType,
            currency: data.Currency,
            debitInterestRate: Required(
                data.DebitInterestRate,
                nameof(data.DebitInterestRate)),
            debitCalculationFrequency: Required(
                data.DebitCalculationFrequency,
                nameof(data.DebitCalculationFrequency)),
            debitPostingFrequency: Required(
                data.DebitPostingFrequency,
                nameof(data.DebitPostingFrequency)),
            tenureInDays: Required(data.TenureInDays, nameof(data.TenureInDays)),
            repaymentCount: Required(
                data.RepaymentCount,
                nameof(data.RepaymentCount)),
            repaymentFrequency: Required(
                data.RepaymentFrequency,
                nameof(data.RepaymentFrequency)),
            minimumAmount: data.MinimunAmount,
            penaltyInterestRate: data.PenaltyInterestRate,
            creditInterestCalculation: data.CreditInterestCalculation,
            creditInterestRate: data.CreditInterestRate,
            creditCalculationFrequency: data.CreditCalculationFrequency,
            creditPostingFrequency: data.CreditPostingFrequency,
            taxPercentage: data.TaxPercentage);
    }

    private static T Required<T>(T? value, string parameterName)
        where T : struct
    {
        return value ?? throw new ArgumentException(
            $"{parameterName} is required for this product type.",
            parameterName);
    }
}
