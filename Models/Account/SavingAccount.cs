using BankingConsole.Models.Enums;

namespace BankingConsole.Models.Account;

public sealed class SavingAccount : Account
{
    private SavingAccount()
    {
    }

    private SavingAccount(string accountNumber,Guid customerId, Guid productId, string branchCode, decimal openingBalance, AccountState accountState, DateTime accountOpenDate)
        : base(
            accountNumber,
            customerId,
            productId,
            branchCode,
            openingBalance,
            AccountState.ACTIVE,
            accountOpenDate,
            interestAccrued: null,
            interestPostedOn: null)
    {
    }

    public static SavingAccount Create(
        string accountNumber,
        Guid customerId,
        Guid productId,
        string branchCode,
        decimal openingBalance = 0)
    {
        if (openingBalance < 0)
            throw new ArgumentOutOfRangeException(
                nameof(openingBalance),
                "A savings account cannot be opened with a negative balance.");

        return new SavingAccount(
            accountNumber,
            customerId,
            productId,
            branchCode,
            openingBalance,
            AccountState.ACTIVE,
            DateTime.UtcNow);
    }
}
