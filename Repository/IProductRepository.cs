using BankingConsole.Models.Product;

namespace BankingConsole.Repository;

public interface IProductRepository
{
    void Add(Product product);
    void Update(Product product);
    void Delete(Product product);

    Task<IReadOnlyList<Product>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<Product?> GetByIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default);
}
