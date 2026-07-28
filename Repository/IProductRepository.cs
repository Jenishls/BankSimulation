using BankingConsole.Models.Product;

namespace BankingConsole.Repository;

public interface IProductRepository
{
    Product GetByIdAsync(Guid productId);
}