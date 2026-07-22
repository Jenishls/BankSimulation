using BankingConsole.Models;
using Microsoft.EntityFrameworkCore;

namespace BankingConsole.Repository;

public class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _context;

    public TransactionRepository(AppDbContext context)
    {
        _context = context;
    }
    public void Add(Transaction transaction)
    {
        _context.Transactions.Add(transaction);
    }

    public async Task<Transaction?> GetByIdAsync(Guid transactionId)
    {
        return await _context.Transactions
        .Include(t => t.Entries)
        .ThenInclude(entry => entry.Account)
        .FirstOrDefaultAsync(t => t.TransactionId == transactionId);
    }

    public async Task<Transaction?> GetByIdempotencyKeyAsync(Guid idempotencyKey)
    {
        return await _context.Transactions
        .AsNoTracking()
        .SingleOrDefaultAsync(
            transaction =>
            transaction.IdempotencyKey == idempotencyKey
        );
        
    }

    public void Update(Transaction transaction)
    {
        _context.Transactions.Update(transaction);
    }
}