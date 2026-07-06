namespace BankingConsole.Models;

public class Account
{
    public Guid AccountId { get; set; }
    public string AccountNumber { get; set; }
    public Guid CustomerId { get; set; }
    public decimal Balance { get; set; }
    public AccountType Type { get; set; }
    public AccountState State { get; set; }
    public enum AccountState { DebitFreeze, CreditFreeze, TotalFreeze, Active }
    public enum AccountType { Savings, Current, FixedDeposit, RecurringDeposit }

    public Account(Guid accountId, string accountNumber, Guid customerId, decimal balance, AccountType type, AccountState state)
    {
        AccountId = accountId;
        AccountNumber = accountNumber;
        CustomerId = customerId;
        Balance = balance;
        Type = type;
        State = state;
    }

    public Account(Customer customer, AccountType type)
    {
        AccountId = Guid.NewGuid();
        AccountNumber = GenerateAccountNumber(AccountId, type);
        CustomerId = customer.CustomerId;
        Balance = 0;
        Type = type;
        State = AccountState.Active;

        Console.WriteLine($"Account {AccountNumber} has been created for customer {customer.Name} \n");
    }

    public string GenerateAccountNumber(Guid accountId, AccountType accountType)
    {
        Random random = new Random();
        string accountNumber = accountType.ToString().Substring(0, 2).ToUpper() + accountId.ToString().Substring(0, 10);
        return accountNumber;
    }
    public void Deposit(decimal amount)
    {
        if (!CheckAccountState())
        {
            Console.WriteLine($"Cannot deposit to account {AccountNumber} as it is not active.");
            return;
        }

        if (amount <= 0)
        {
            Console.WriteLine("Deposit amount must be positive");
            return;
        }

        Balance += amount;
        Console.WriteLine($"Amount {amount} deposited into account {AccountNumber}. New balance is {Balance}");
    }
    public void Withdraw(decimal amount)
    {
        if (!CheckAccountState())
        {
            Console.WriteLine($"Cannot withdraw from account {AccountNumber} as it is not active.");
            return;
        }

        if (amount <= 0)
        {
            Console.WriteLine("Withdraw amount should be greater than Zero");
            return;
        }

        if (amount > Balance)
        {
            Console.WriteLine("Insufficient funds");
            return;
        }

        Balance -= amount;
        Console.WriteLine($"Amount {amount} has been withdrawn from account {AccountNumber}. New balance is {Balance}");
    }

    public bool CheckAccountState()
    {
        if (State != AccountState.Active)
        {
            return false;
        }
        return true;
    }

}