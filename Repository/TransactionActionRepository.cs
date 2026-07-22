using BankingConsole.Models;
using Microsoft.EntityFrameworkCore;

namespace BankingConsole.Repository;

public class TransactionActionRepository : ITransactionActionRepository
{
    private readonly AppDbContext _context;

    public TransactionActionRepository(AppDbContext context)
    {
        _context = context;
    }
    public void Add(TransactionAction transactionAction)
    {
        _context.TransactionActions.Add(transactionAction);
    }

    public async Task<TransactionAction?> GetAsync(Guid transactionActionId)
    {
        return await _context.TransactionActions.FindAsync(transactionActionId);
    }

    public async Task<IEnumerable<TransactionAction>> ListAsync()
    {
        return await _context.TransactionActions.AsNoTracking().ToListAsync();
    }
}