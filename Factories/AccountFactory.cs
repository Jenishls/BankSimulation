using BankingConsole.Models;
using BankingConsole.Models.Account;
using BankingConsole.Models.AccountCreation;
using BankingConsole.Models.Enums;
using BankingConsole.Models.Product;
using BankingConsole.Services;
using AccountEntity = BankingConsole.Models.Account.Account;

namespace BankingConsole.Factories;

public sealed class AccountFactory : IAccountFactory
{
    public AccountEntity Create(AccountCreationData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        ValidateCommonData(data);

        var accountNumber = data.BranchCode + data.ProductType.ToString().Substring(0,2) + Guid.NewGuid().ToString("N")[..12];
        return data.ProductType switch
        {
            ProductType.TERM =>
                CreateTerm(data, accountNumber),

            ProductType.LOAN =>
                CreateLoan(data, accountNumber),

            ProductType.SAVING =>
                CreateSaving(data, accountNumber),

            _ => throw new ArgumentOutOfRangeException(
                nameof(data.ProductType),
                "Unsupported product type.")
        };
    }

    private static TermAccount CreateTerm(
        AccountCreationData data,
        string accountNumber)
    {
        var details = data.Term
            ?? throw new ArgumentException(
                "Term-account details are required.",
                nameof(data));

        return TermAccount.Create(
            accountNumber,
            data.CustomerId,
            data.ProductId,
            data.BranchCode,
            details.Principal,
            details.MaturityDate,
            details.FundingAccountId);
    }

    private static SavingAccount CreateSaving(
        AccountCreationData data,
        string accountNumber)
    {
        var details = data.Saving
            ?? throw new ArgumentException(
                "Saving-account details are required.",
                nameof(data));

        return SavingAccount.Create(
            accountNumber,
            data.CustomerId,
            data.ProductId,
            data.BranchCode,
            details.OpeningBalance
            );
    }

    private static LoanAccount CreateLoan(
        AccountCreationData data,
        string accountNumber)
    {
        var details = data.Loan
            ?? throw new ArgumentException(
                "Loan-account details are required.",
                nameof(data));

        return LoanAccount.Create(
            accountNumber,
            data.CustomerId,
            data.ProductId,
            data.BranchCode,
            details.Principal,
            details.DisbursementAccountId,
            details.RepaymentInstallments);
    }

    private static void ValidateCommonData( AccountCreationData data){
        if (data.CustomerId == Guid.Empty)
            throw new ArgumentException(
                "Customer id is required.",
                nameof(data.CustomerId));

        if (data.ProductId == Guid.Empty)
            throw new ArgumentException(
                "Product id is required.",
                nameof(data.ProductId));

    }
}
