using BankingConsole.Models.Account;

namespace BankingConsole.Repository
{
    public interface IAccountRepository
    {
        Task<Account?> GetAccountByIdAsync(
            Guid accountId,
            CancellationToken cancellationToken = default);
        Task<Account?> GetAccountByNumberAsync(
            string accountNumber,
            CancellationToken cancellationToken = default);
        Task<IEnumerable<Account>> GetAccountsByCustomerIdAsync(
            Guid customerId,
            CancellationToken cancellationToken = default);
        void AddAccount(Account account);
    }
}
