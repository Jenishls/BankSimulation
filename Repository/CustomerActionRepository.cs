using BankingConsole.Models.Customer;
using Microsoft.EntityFrameworkCore;

namespace BankingConsole.Repository;

public class CustomerActionRepository : ICustomerActionRepository
{
    private readonly AppDbContext _context;

    public CustomerActionRepository(AppDbContext context)
    {
        _context = context;
    } 

    public void Add(CustomerAction customerAction)
    {
        _context.CustomerActions.Add(customerAction);
    }

    public void RemoveRange(IEnumerable<CustomerAction> customerActions)
    {
        _context.CustomerActions.RemoveRange(customerActions);
    }

    public async Task<IReadOnlyList<CustomerAction>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        return await _context.CustomerActions
            .Where(c => c.CustomerId == customerId)
            .ToListAsync(cancellationToken);
    }

    public async Task<CustomerAction?> GetByIdempotencyKeyAsync(
        Guid idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        return await _context.CustomerActions
            .SingleOrDefaultAsync(
                c => c.IdempotencyKey == idempotencyKey,
                cancellationToken);
    }
}
