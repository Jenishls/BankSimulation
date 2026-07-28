using BankingConsole.Models.Enums;

namespace BankingConsole.Models.Account;
public sealed class CustomerAccount : Account
{
    public Guid CustomerId { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal InterestAccured { get; private set; } = 0m;
    public DateTime InterestPostedOn { get; private set; }

    public override AccountType AccountType => AccountType.CUSTOMER;


    private CustomerAccount(){}

    private CustomerAccount(
        string accountNumber,
        string name,
        string branchCode,
        Guid customerId,
        Guid productId,
        AccountState state,
        DateTime accountOpenDate,
        decimal interestAccrued,
        DateTime interestPostedOn,
        decimal balance
        )
        :base(
            accountNumber,
            name,
            branchCode,
            balance,
            state,
            accountOpenDate
        )
    {
        if (customerId == Guid.Empty)
        throw new ArgumentException(
            "Customer id cannot be empty when supplied.",
            nameof(customerId));

        if (productId == Guid.Empty)
        throw new ArgumentException(
            "Product id is required.",
            nameof(productId));

        if (interestAccrued < 0)
            throw new ArgumentOutOfRangeException(nameof(interestAccrued));
        
        if (interestPostedOn < accountOpenDate)
        {
            throw new ArgumentOutOfRangeException(
                nameof(interestPostedOn),
                "Interest posting date cannot be before the account opening date.");
        } 

    }

    public static CustomerAccount Create(

        string name,
        Guid customerId,
        Guid productId,
        string branchCode,
        AccountState state,
        decimal interestAccured,
        DateTime interestPostedOn
    )
    {
        var account = new CustomerAccount();
        return new CustomerAccount
        (
            account.GenerateAccountNumber(),
            name,
            branchCode,
            customerId,
            productId,
            state,
            DateTime.UtcNow,
            interestAccured,
            interestPostedOn,
            0
        );
    }

    protected override string GenerateAccountNumber()
    {
        string acc = BranchCode + ProductId + new Random().ToString();
        return acc;
    }
}