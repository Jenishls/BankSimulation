using BankingConsole.Models;
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

    public async Task<Account?> GetAccountByIdAsync(Guid accountId)
    {
        return await _context.Accounts.FindAsync(accountId);
    }

    public async Task<Account?> GetAccountByNumberAsync(string accountNumber)
    {
        return await _context.Accounts
        .FirstOrDefaultAsync(
            account => account.AccountNumber == accountNumber
            );
    }

    public async Task<IEnumerable<Account>> GetAccountsByCustomerIdAsync(Guid customerId)
    {
        return await _context.Accounts
        .Where(account => account.CustomerId == customerId)
        .ToListAsync();
    }

}