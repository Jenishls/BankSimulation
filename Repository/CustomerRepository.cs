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

    public void DeleteCustomer(Customer customer)
    {
        _context.Customers.Remove(customer);
    }

    public async Task<IReadOnlyList<Customer>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await CustomerQuery()
            .ToListAsync(cancellationToken);
    }

    public async Task<Customer?> GetByIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        return await CustomerQuery()
            .SingleOrDefaultAsync(
                customer => customer.CustomerId == customerId,
                cancellationToken);
    }

    private IQueryable<Customer> CustomerQuery()
    {
        return _context.Customers
            .Include(c => c.Address)
            .Include(c => c.Contact)
            .Include(c => c.Identity)
            .Include(c => ((IndividualCustomer)c).Nominee)
            .Include(c => ((InstitutionalCustomer)c).Roles);
    }
}
