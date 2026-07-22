using BankingConsole.Models;
using BankingConsole.Models.Enums;

namespace BankingConsole.Repository;

public interface ITransactionRepository
{
    void Add(Transaction transaction);
    Task<Transaction?> GetByIdAsync(Guid transactionId);
    Task<Transaction?> GetByIdempotencyKeyAsync(Guid idempotencyKey);
    void Update(Transaction transaction);
}