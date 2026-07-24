using BankingConsole.Models.Customer;
using BankingConsole.Models.CustomerCreation;

namespace BankingConsole.Factories;

public interface ICustomerFactory
{
    Customer Create(CustomerCreationData data);
}