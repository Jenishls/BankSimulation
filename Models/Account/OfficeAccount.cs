using BankingConsole.Models.Enums;

namespace BankingConsole.Models.Account;

public sealed class OfficeAccount : Account
{
    private OfficeAccountType OfficeAccountType{get; set;}
    public override AccountType AccountType => AccountType.OFFICE;

    private OfficeAccount()
    {
    }

    private OfficeAccount(
        string accountNumber,
        string name,
        string branchCode,
        AccountState state,
        DateTime accountOpenDate,
        OfficeAccountType officeAccountType,
        decimal balance
        )
        : base(
            accountNumber,
            name,
            branchCode,
            balance,
            state,
            accountOpenDate
            )
    {}

    public static OfficeAccount Create(

        string name,
        string branchCode,
        AccountState state,
        OfficeAccountType officeAcountType
        )
    {
        var account = new OfficeAccount();
            
        return new OfficeAccount(
            account.GenerateAccountNumber(),
            name,
            branchCode,
            state,
            DateTime.UtcNow,
            officeAcountType,
            0
            );
    }

    protected override string GenerateAccountNumber()
    {
        string acc = "O" + new Random().ToString();
        return acc;
    }
}
