using BankingConsole.Models.Product;
using Microsoft.EntityFrameworkCore;

namespace BankingConsole.Repository;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;
    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Add(Product product)
    {
        _context.Products.Add(product);
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetByIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Products.FindAsync(
            [productId],
            cancellationToken);
    }
}
