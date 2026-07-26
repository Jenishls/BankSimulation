using BankingConsole.Models.Product;
using BankingConsole.Models.ProductCreation;

namespace BankingConsole.Factories;

public interface IProductFactory
{
    public Product Create(ProductCreationData product);
}