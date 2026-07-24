using System.ComponentModel.DataAnnotations;
using BankingConsole.Models.Enums;

namespace BankingConsole.Models;

public class Account
{
    public Guid AccountId { get; set; }
    public required string AccountNumber { get; set; }
    public Guid CustomerId { get; set; }
    public decimal Balance { get; private set; }
    public AccountType Type { get; set; }
    public AccountState State { get; set; }
    public decimal MinimumBalance { get; set; }
    public decimal DailyWithdrawalLimit { get; set; }
    public decimal InterestRate { get; set; }
    [Timestamp]
    public byte[] RowVersion { get; private set; } = [];
    public Account(){}

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
        return stateAllowedForDeposit && amount > 0;
    }
    public bool CanWithdraw(decimal amount)
    {
        var stateAllowedForWithdraw = State == AccountState.ACTIVE || State == AccountState.CREDITFREEZE;
        var withinDailyLimit = DailyWithdrawalLimit <= 0 || amount <= DailyWithdrawalLimit;
        return stateAllowedForWithdraw && amount > 0 && withinDailyLimit && Balance - amount >= MinimumBalance;
    }
    public decimal GetBalance()
    {
        return Balance;
    }
}