using BankingConsole.Models.Enums;

namespace BankingConsole.Models.Account;

public sealed class OfficeAccount : Account
{
    public OfficeAccountType OfficeAccountType { get; private set; }

    private OfficeAccount()
    {
    }

    private OfficeAccount(
        string accountNumber,
        string name,
        string branchCode,
        OfficeAccountType officeAccountType,
        decimal openingBalance)
        : base(
            accountNumber,
            name,
            branchCode,
            openingBalance,
            AccountState.ACTIVE,
            DateTime.UtcNow,
            AccountType.OFFICE)
    {
        OfficeAccountType = officeAccountType;
    }

    public static OfficeAccount Create(
        string accountNumber,
        string name,
        string branchCode,
        OfficeAccountType officeAccountType,
        decimal openingBalance = 0)
    {
        return new OfficeAccount(
            accountNumber,
            name,
            branchCode,
            officeAccountType,
            openingBalance);
    }
}
