using BankingConsole.Models.Product;

namespace BankingConsole.Repository;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;
    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }
    public Product GetByIdAsync(Guid productId)
    {
        return _context.Products.FirstOrDefault(productId);
    }
}