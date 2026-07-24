using BankingConsole.Models.Customer;

namespace BankingConsole.Repository;

public interface ICustomerActionRepository
{
    void Add(CustomerAction customerAction);
    Task<CustomerAction?> GetByIdempotencyKeyAsync(Guid idempotencyKey);
    Task<IReadOnlyList<CustomerAction>> GetByCustomerIdAsync(Guid customerId);
}
