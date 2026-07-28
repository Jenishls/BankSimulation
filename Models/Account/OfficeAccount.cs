using BankingConsole.Models.Enums;

namespace BankingConsole.Models.Account;

public sealed class OfficeAccount : Account
{
    private OfficeAccount()
    {
    }

    private OfficeAccount(
        string accountNumber,
        Guid productId,
        string branchCode,
        decimal openingBalance,
        DateTime accountOpenDate)
        : base(
            accountNumber,
            customerId: null,
            productId,
            branchCode,
            openingBalance,
            AccountState.ACTIVE,
            accountOpenDate,
            interestAccrued: null,
            interestPostedOn: null)
    {
    }

    public static OfficeAccount Create(
        string accountNumber,
        Guid productId,
        string branchCode,
        decimal openingBalance = 0)
    {
        if (openingBalance < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(openingBalance),
                "An office account cannot be opened with a negative balance.");
        }

        return new OfficeAccount(
            accountNumber,
            productId,
            branchCode,
            openingBalance,
            DateTime.UtcNow);
    }
}
