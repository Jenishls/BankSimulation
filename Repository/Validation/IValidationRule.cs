using BankingConsole.Models;

namespace BankingConsole.Repository.Validation;

public interface IValidationRule<T>
{
    Task ValidateAsync(Account account, LedgerEntry entry);
}