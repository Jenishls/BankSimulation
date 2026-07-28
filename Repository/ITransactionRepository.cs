using BankingConsole.Models;
using BankingConsole.Models.Enums;

namespace BankingConsole.Repository;

public interface ITransactionRepository
{
    void Add(Transaction transaction);
    Task<Transaction?> GetByIdAsync(
        Guid transactionId,
        CancellationToken cancellationToken = default);
    Task<Transaction?> GetByIdempotencyKeyAsync(
        Guid idempotencyKey,
        CancellationToken cancellationToken = default);
}
