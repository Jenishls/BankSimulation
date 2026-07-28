using BankingConsole.Models.Product;
using BankingConsole.Models.ProductCreation;

namespace BankingConsole.Factories;

public interface IProductFactory
{
    Product Create(ProductCreationData product);
}
