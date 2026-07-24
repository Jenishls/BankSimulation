using BankingConsole.Models.Customer;
using Microsoft.EntityFrameworkCore;

namespace BankingConsole.Repository;

public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _context;
    public CustomerRepository(AppDbContext context)
    {
        _context = context;
    }
    public void AddCustomer(Customer customer)
    {
        _context.Customers.Add(customer);
    }
    public void UpdateCustomer(Customer customer)
    {
        _context.Customers.Update(customer);
    }

    public async Task<IReadOnlyList<Customer>> GetAllAsync()
    {
        return await _context.Customers.ToListAsync();
    }

    public async Task<Customer?> GetByIdAsync(Guid customerId)
    {
        return await _context.Customers
            .Include(c => c.Address)
            .Include(c => c.Contact)
            .Include(c => c.Identity)
            .Include(c => ((IndividualCustomer)c).Nominee)
            .Include(c => ((InstitutionalCustomer)c).Roles)
            .SingleOrDefaultAsync(c => c.CustomerId == customerId);
    }

}
