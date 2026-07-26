using BankingConsole.Models.AccountCreation;
using BankingConsole.Models;
using BankingConsole.Models.Product;
using AccountEntity = BankingConsole.Models.Account.Account;

namespace BankingConsole.Factories;

public interface IAccountFactory
{
    AccountEntity Create(AccountCreationData data);
}
