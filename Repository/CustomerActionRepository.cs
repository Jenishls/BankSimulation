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

    public async Task<IReadOnlyList<CustomerAction>> GetByCustomerIdAsync(Guid customerId)
    {
        return await _context.CustomerActions
            .Where(c => c.CustomerId == customerId)
            .ToListAsync();
    }

    public async Task<CustomerAction?> GetByIdempotencyKeyAsync(Guid guid)
    {
        return await _context.CustomerActions
            .SingleOrDefaultAsync(
                c => c.IdempotencyKey == guid
                );
    }
}