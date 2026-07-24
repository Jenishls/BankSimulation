using BankingConsole.Models;
using BankingConsole.Services;
using BankingConsole.Models.Enums;
using CustomerEntity = BankingConsole.Models.Customer.Customer;

namespace BankingConsole.Models.Factories;

public class AccountFactory
{
    public Account Create(AccountType accountType, CustomerEntity customer)
    {
        var accountId = Guid.NewGuid();
        return accountType switch
        {
            AccountType.SAVINGS => new SavingAccount
            {
                AccountId = accountId,
                AccountNumber = BankingConsole.Services.AccountNumberGeneratorService.GenerateAccountNumber(accountId, accountType),
                CustomerId = customer.CustomerId,
                Type = accountType,
                State = AccountState.ACTIVE,
                MinimumBalance = 100m,
                DailyWithdrawalLimit = 1000m,
                InterestRate = 0.04m
            },
            AccountType.CURRENT => new CurrentAccount
            {
                AccountId = accountId,
                AccountNumber = BankingConsole.Services.AccountNumberGeneratorService.GenerateAccountNumber(accountId, accountType),
                CustomerId = customer.CustomerId,
                Type = accountType,
                State = AccountState.ACTIVE,
                MinimumBalance = -1000m,
                DailyWithdrawalLimit = 5000m,
                InterestRate = 0m
            },
            _ => throw new ArgumentOutOfRangeException(nameof(accountType), accountType, "Unsupported account type")
        };
    }
}
