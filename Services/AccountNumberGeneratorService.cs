using BankingConsole.Models;
using BankingConsole.Models.Enums;

namespace BankingConsole.Services;

public class AccountNumberGeneratorService
{
    public static string GenerateAccountNumber(Guid accountId, AccountType accountType)
    {
        Random random = new Random();
        string accountNumber = accountType.ToString().Substring(0, 2).ToUpper() + accountId.ToString().Substring(0, 10);
        return accountNumber;
    }

}