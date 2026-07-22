using BankingConsole.Models;

namespace BankingConsole.Repository;

public interface ITransactionActionRepository
{
    Task<TransactionAction?> GetAsync(Guid transactionActionId);
    Task<IEnumerable<TransactionAction>> ListAsync();
    void Add(TransactionAction transactionAction);
}