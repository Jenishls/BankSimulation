using System.ComponentModel.DataAnnotations;
using BankingConsole.Models.Enums;
using BankingConsole.Models.Product;

namespace BankingConsole.Models.Account;

public abstract class Account
{
    public Guid AccountId { get; private set; }
    public string AccountNumber { get; private set; }
    public string Name {get; private set;}
    public string BranchCode {get; private set; }
    public decimal Balance { get; private  set; } = 0m;
    public AccountState State { get; private set; }
    public DateTime AccountOpenDate { get; private set; } = DateTime.UtcNow;
    [Timestamp]
    public byte[] RowVersion { get; private set; } = [];
    protected Account(){}

    protected abstract string GenerateAccountNumber();

    protected Account(
        string accountNumber,
        string name,
        string branchCode,
        decimal balance,
        AccountState accountState,
        DateTime accountOpenDate)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
            throw new ArgumentException(
                "Account number is required.",
                nameof(accountNumber));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Account Name is required.",
                nameof(accountNumber));

        if (branchCode is null)
            throw new ArgumentException(
                "Branch id is required.",
                nameof(branchCode));


        AccountId = Guid.NewGuid();
        AccountNumber = accountNumber.Trim();
        Name = name.Trim();
        BranchCode = branchCode;
        Balance = balance;
        State = accountState;
        AccountOpenDate = accountOpenDate;
    }

    public bool Deposit(decimal amount)
    {
        if (!CanDeposit(amount)) return false;

        Balance += amount;
        return true;
    }
    public bool Withdraw(decimal amount)
    {
        if (!CanWithdraw(amount)) return false;

        Balance -= amount;
        return true;
    }
    public bool CanDeposit(decimal amount)
    {
        var stateAllowedForDeposit = State == AccountState.ACTIVE || State == AccountState.DEBITFREEZE;
        return stateAllowedForDeposit;
    }
    public bool CanWithdraw(decimal amount)
    {
        var stateAllowedForWithdraw = State == AccountState.ACTIVE || State == AccountState.CREDITFREEZE;
        return stateAllowedForWithdraw && amount > 0;
    }
    
    public decimal GetBalance()
    {
        return Balance;
    }
}
