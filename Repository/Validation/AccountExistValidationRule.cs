using BankingConsole.Models.Account;
using BankingConsole.Models;

namespace BankingConsole.Repository.Validation;

public class AccountExistValidationRule : IValidationRule<LedgerEntry>
{
    public async Task ValidateAsync(Account account, LedgerEntry entry)
    {
        if (account == null)
        {
            throw new ArgumentException("Account does not exist.");
        }
    }
}