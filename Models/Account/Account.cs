using System.ComponentModel.DataAnnotations;
using BankingConsole.Models.Enums;
using BankingConsole.Models.Product;

namespace BankingConsole.Models.Account;

public abstract class Account
{
    public Guid AccountId { get; private set; }
    public string AccountNumber { get; private set; } = null!;
    public string BranchCode {get; private set; } = null!;
    public Guid CustomerId { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal Balance { get; private  set; }
    public AccountState State { get; private set; }
    public DateTime AccountOpenDate { get; private set; } = DateTime.UtcNow;
    public decimal? InterestAccured { get; private set; }
    public DateTime? InterestPostedOn { get; private set; }

    [Timestamp]
    public byte[] RowVersion { get; private set; } = [];
    protected Account()
    {
    }

    protected Account(
        string accountNumber,
        Guid customerId,
        Guid productId,
        string branchCode,
        decimal balance,
        AccountState accountState,
        DateTime accountOpenDate,
        decimal? interestAccrued = null,
        DateTime? interestPostedOn = null)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
            throw new ArgumentException(
                "Account number is required.",
                nameof(accountNumber));

        if (customerId == Guid.Empty)
            throw new ArgumentException(
                "Customer id is required.",
                nameof(customerId));
                
        if (branchCode == null)
            throw new ArgumentException(
                "Branch id is required.",
                nameof(branchCode));

        if (productId == Guid.Empty)
            throw new ArgumentException(
                "Product id is required.",
                nameof(productId));

        if (interestAccrued < 0)
            throw new ArgumentOutOfRangeException(nameof(interestAccrued));

        if (interestPostedOn.HasValue &&
            interestPostedOn.Value < accountOpenDate)
        {
            throw new ArgumentOutOfRangeException(
                nameof(interestPostedOn),
                "Interest posting date cannot be before the account opening date.");
        }

        AccountId = Guid.NewGuid();
        AccountNumber = accountNumber.Trim();
        CustomerId = customerId;
        BranchCode = branchCode;
        ProductId = productId;
        Balance = balance;
        State = accountState;
        AccountOpenDate = accountOpenDate;
        InterestAccured = interestAccrued;
        InterestPostedOn = interestPostedOn;
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
