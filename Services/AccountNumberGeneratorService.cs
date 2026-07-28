using BankingConsole.Models.Enums;

namespace BankingConsole.Services;

public sealed class AccountNumberGeneratorService
{
    public string Generate(
        string branchCode,
        AccountType accountType,
        ProductType? productType = null)
    {
        if (string.IsNullOrWhiteSpace(branchCode))
        {
            throw new ArgumentException(
                "Branch code is required.",
                nameof(branchCode));
        }

        var typeCode = accountType switch
        {
            AccountType.OFFICE => "OF",
            AccountType.CUSTOMER => productType switch
            {
                ProductType.SAVING => "SA",
                ProductType.TERM => "TD",
                ProductType.LOAN => "LA",
                _ => throw new ArgumentException(
                    "A product type is required for a customer account.",
                    nameof(productType))
            },
            _ => throw new ArgumentOutOfRangeException(
                nameof(accountType),
                accountType,
                "Unsupported account type.")
        };

        var normalizedBranchCode =
            branchCode.Trim().ToUpperInvariant();
        var uniquePart =
            Guid.NewGuid().ToString("N")[..12].ToUpperInvariant();

        return $"{normalizedBranchCode}{typeCode}{uniquePart}";
    }
}
