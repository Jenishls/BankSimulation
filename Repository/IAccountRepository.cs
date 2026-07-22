using BankingConsole.Models;

namespace BankingConsole.Repository
{
    public interface IAccountRepository
    {
        Task<Account?> GetAccountByIdAsync(Guid accountId);
        Task<Account?> GetAccountByNumberAsync(string accountNumber);
        Task<IEnumerable<Account>> GetAccountsByCustomerIdAsync(Guid customerId);
        void AddAccount(Account account);
    }
}