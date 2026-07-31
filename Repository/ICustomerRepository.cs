using BankingConsole.Models.Customer;
namespace BankingConsole.Repository;

public interface ICustomerRepository
{
    void AddCustomer(Customer customer);
    void UpdateCustomer(Customer customer);
    void DeleteCustomer(Customer customer);
    Task<Customer?> GetByIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Customer>> GetAllAsync(
        CancellationToken cancellationToken = default);
}
