using BankingConsole.Models.Product;
using BankingConsole.Models.ProductCreation;

namespace BankingConsole.Factories;

public sealed class ProductFactory : IProductFactory
{
    public Product Create(ProductCreationData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        return Product.Create(
            productCode: data.ProductCode,
            productName: data.ProductName,
            branchCode: data.BranchCode,
            currency: data.Currency,
            allowedCustomerType: data.AllowedCustomerType,
            interestRate: data.InterestRate,
            interestPostPolicies: data.InterestPostPolicies,
            interestOfficeAccountId: data.InterestOfficeAccountId,
            taxOfficeAccountId: data.TaxOfficeAccountId,
            productType: data.ProductType,
            taxRate: data.TaxRate,
            postInterestToLinkedAccount:
                data.PostInterestToLinkedAccount,
            minimumAmount: data.MinimumAmount,
            postDate: data.PostDate,
            interestPostingFrequency: data.InterestPostingFrequency,
            tenureInDays: data.TenureInDays,
            transferCount: data.TransferCount,
            transferFlow: data.TransferFlow,
            transferPenaltyRate: data.TransferPenaltyRate,
            allowPremature: data.AllowPremature);
    }
}
