using BankingConsole.Models;

class Program
{
    private static void Main(string[] args)
    {
        Customer customer1 = new Customer("Jane Doe", "jane@example.com", "1234567890");
        Account account1 = new Account(customer1, Account.AccountType.Savings);

        Customer customer2 = new Customer("John Smith", "john@example.com", "0987654321");
        Account account2 = new Account(customer2, Account.AccountType.Current);

        account1.Deposit(100.50m);
        account1.Withdraw(20.00m);

        account2.Deposit(50.00m);

    }
}