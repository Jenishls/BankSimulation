using BankingConsole.Models.Account;
using Microsoft.EntityFrameworkCore;

namespace BankingConsole.Repository;

public class AccountRepository : IAccountRepository
{
    private readonly AppDbContext _context;

    public AccountRepository(AppDbContext context)
    {
        _context = context;
    }

    public void AddAccount(Account account)
    {
        _context.Accounts.Add(account);
    }

    public async Task<Account?> GetAccountByIdAsync(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Accounts.FindAsync(
            [accountId],
            cancellationToken);
    }

    public async Task<Account?> GetAccountByNumberAsync(
        string accountNumber,
        CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
        .FirstOrDefaultAsync(
            account => account.AccountNumber == accountNumber,
            cancellationToken);
    }

    public async Task<IEnumerable<Account>> GetAccountsByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
        .OfType<CustomerAccount>()
        .Where(account => account.CustomerId == customerId)
        .ToListAsync(cancellationToken);
    }

}
