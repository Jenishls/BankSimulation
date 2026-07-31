using BankingConsole.Models.Customer;

namespace BankingConsole.Repository;

public interface ICustomerActionRepository
{
    void Add(CustomerAction customerAction);
    void RemoveRange(IEnumerable<CustomerAction> customerActions);
    Task<CustomerAction?> GetByIdempotencyKeyAsync(
        Guid idempotencyKey,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomerAction>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default);
}
