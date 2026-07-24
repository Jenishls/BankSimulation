using BankingConsole.Models.Customer;
namespace BankingConsole.Repository;

public interface ICustomerRepository
{
    public void AddCustomer(Customer customer);
    public void UpdateCustomer(Customer customer);
    public Task<Customer?> GetByIdAsync(Guid customerId);
    public Task<IReadOnlyList<Customer>> GetAllAsync();

}
