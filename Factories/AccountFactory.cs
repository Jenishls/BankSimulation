using BankingConsole.Models.Account;
using BankingConsole.Models.AccountCreation;
using BankingConsole.Models.Enums;
using BankingConsole.Models.Product;
using BankingConsole.Services;
using AccountEntity = BankingConsole.Models.Account.Account;

namespace BankingConsole.Factories;

public sealed class AccountFactory : IAccountFactory
{
    private readonly AccountNumberGeneratorService _accountNumberGenerator;

    public AccountFactory(
        AccountNumberGeneratorService accountNumberGenerator)
    {
        _accountNumberGenerator = accountNumberGenerator;
    }

    public AccountEntity Create(
        AccountCreationData data,
        Product? product = null)
    {
        ArgumentNullException.ThrowIfNull(data);
        ValidateCommonData(data, product);

        var branchCode = data.AccountType == AccountType.CUSTOMER
            ? product!.BranchCode
            : data.BranchCode!;

        var accountNumber = _accountNumberGenerator.Generate(
            branchCode,
            data.AccountType,
            product?.ProductType);

        return data.AccountType switch
        {
            AccountType.CUSTOMER => CreateCustomer(
                data,
                product!,
                accountNumber),
            AccountType.OFFICE => CreateOffice(data, accountNumber),
            _ => throw new ArgumentOutOfRangeException(
                nameof(data.AccountType),
                data.AccountType,
                "Unsupported account type.")
        };
    }

    private static CustomerAccount CreateCustomer(
        AccountCreationData data,
        Product product,
        string accountNumber)
    {
        var customerId = data.CustomerId
            ?? throw new ArgumentException(
                "Customer id is required for a customer account.",
                nameof(data.CustomerId));

        if (data.ProductId != product.ProductId)
        {
            throw new ArgumentException(
                "The supplied product does not match the requested product id.",
                nameof(product));
        }

        return CustomerAccount.Create(
            accountNumber,
            data.Name,
            customerId,
            product.ProductId,
            product.ProductType,
            product.BranchCode,
            data.OpeningBalance,
            data.Principal,
            product.TenureInDays,
            data.FundingAccountId,
            data.MaturitySettlementAccountId,
            data.DisbursementAccountId,
            data.RepaymentAccountId,
            data.RepaymentInstallments);
    }

    private static OfficeAccount CreateOffice(
        AccountCreationData data,
        string accountNumber)
    {
        var officeAccountType = data.OfficeAccountType
            ?? throw new ArgumentException(
                "Office account type is required for an office account.",
                nameof(data.OfficeAccountType));

        return OfficeAccount.Create(
            accountNumber,
            data.Name,
            data.BranchCode!,
            officeAccountType,
            data.OpeningBalance);
    }

    private static void ValidateCommonData(
        AccountCreationData data,
        Product? product)
    {
        if (string.IsNullOrWhiteSpace(data.Name))
        {
            throw new ArgumentException(
                "Account name is required.",
                nameof(data.Name));
        }

        if (data.AccountType == AccountType.CUSTOMER &&
            product is null)
        {
            throw new ArgumentException(
                "A product is required to create a customer account.",
                nameof(product));
        }

        if (data.AccountType == AccountType.CUSTOMER &&
            data.OfficeAccountType.HasValue)
        {
            throw new ArgumentException(
                "Customer accounts cannot have an office account type.",
                nameof(data.OfficeAccountType));
        }

        if (data.AccountType == AccountType.OFFICE)
        {
            if (string.IsNullOrWhiteSpace(data.BranchCode))
            {
                throw new ArgumentException(
                    "Branch code is required for an office account.",
                    nameof(data.BranchCode));
            }

            if (product is not null)
            {
                throw new ArgumentException(
                    "Office accounts cannot be associated with a product.",
                    nameof(product));
            }

            if (data.CustomerId.HasValue ||
                data.ProductId.HasValue ||
                data.Principal.HasValue ||
                data.FundingAccountId.HasValue ||
                data.MaturitySettlementAccountId.HasValue ||
                data.DisbursementAccountId.HasValue ||
                data.RepaymentAccountId.HasValue ||
                data.RepaymentInstallments.HasValue)
            {
                throw new ArgumentException(
                    "Office accounts cannot have a customer or product.");
            }
        }
    }
}
