using BankingConsole.Factories;
using BankingConsole.Models.Account;
using BankingConsole.Models.AccountCreation;
using BankingConsole.Models.Enums;
using BankingConsole.Models.Product;
using BankingConsole.Services;
using FluentAssertions;

namespace BankingConsole.Tests;

public sealed class AccountNumberGeneratorServiceTests
{
    private readonly AccountNumberGeneratorService _generator = new();

    [Fact]
    public void Generate_CustomerAccount_UsesBranchAndProductPrefixes()
    {
        var accountNumber = _generator.Generate(
            "ber",
            AccountType.CUSTOMER,
            ProductType.SAVING);

        accountNumber.Should().StartWith("BERSA");
        accountNumber.Should().HaveLength(17);
    }

    [Fact]
    public void Generate_OfficeAccount_UsesOfficePrefix()
    {
        var accountNumber = _generator.Generate(
            "ber",
            AccountType.OFFICE);

        accountNumber.Should().StartWith("BEROF");
        accountNumber.Should().HaveLength(17);
    }

    [Fact]
    public void Generate_CustomerAccountWithoutProductType_Throws()
    {
        var action = () => _generator.Generate(
            "ber",
            AccountType.CUSTOMER);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Factory_CreatesCustomerWithGeneratedAccountNumber()
    {
        var factory = new AccountFactory(_generator);
        var customerId = Guid.NewGuid();
        var product = CreateProduct(ProductType.SAVING);

        var account = factory.Create(new AccountCreationData
        {
            AccountType = AccountType.CUSTOMER,
            Name = "Everyday Savings",
            CustomerId = customerId,
            ProductId = product.ProductId,
            OpeningBalance = 100m
        }, product);

        var customerAccount = account.Should()
            .BeOfType<CustomerAccount>()
            .Subject;

        customerAccount.AccountNumber.Should().StartWith("BERSA");
        customerAccount.CustomerId.Should().Be(customerId);
        customerAccount.ProductId.Should().Be(product.ProductId);
        customerAccount.Balance.Should().Be(100m);
    }

    [Fact]
    public void Factory_CreatesOfficeWithGeneratedAccountNumber()
    {
        var factory = new AccountFactory(_generator);

        var account = factory.Create(new AccountCreationData
        {
            AccountType = AccountType.OFFICE,
            Name = "Cash Till",
            BranchCode = "ber",
            OfficeAccountType = OfficeAccountType.CASH,
            OpeningBalance = 100m
        });

        var officeAccount = account.Should()
            .BeOfType<OfficeAccount>()
            .Subject;

        officeAccount.AccountNumber.Should().StartWith("BEROF");
        officeAccount.OfficeAccountType.Should().Be(OfficeAccountType.CASH);
        officeAccount.Balance.Should().Be(100m);
    }

    [Fact]
    public void Factory_CreatesFixedDepositUsingCustomerAccount()
    {
        var factory = new AccountFactory(_generator);
        var fundingAccountId = Guid.NewGuid();
        var maturitySettlementAccountId = Guid.NewGuid();
        var product = CreateProduct(ProductType.TERM, 365);

        var account = factory.Create(new AccountCreationData
        {
            AccountType = AccountType.CUSTOMER,
            Name = "One Year Fixed Deposit",
            CustomerId = Guid.NewGuid(),
            ProductId = product.ProductId,
            Principal = 5_000m,
            FundingAccountId = fundingAccountId,
            MaturitySettlementAccountId = maturitySettlementAccountId
        }, product);

        var customerAccount = account.Should()
            .BeOfType<CustomerAccount>()
            .Subject;

        customerAccount.AccountNumber.Should().StartWith("BERTD");
        customerAccount.ProductType.Should().Be(ProductType.TERM);
        customerAccount.Principal.Should().Be(5_000m);
        customerAccount.Balance.Should().Be(5_000m);
        customerAccount.MaturityDate.Should().BeCloseTo(
            DateTime.UtcNow.AddDays(365),
            TimeSpan.FromSeconds(2));
        customerAccount.FundingAccountId.Should().Be(fundingAccountId);
        customerAccount.MaturitySettlementAccountId.Should()
            .Be(maturitySettlementAccountId);
    }

    [Fact]
    public void Factory_CreatesLoanUsingCustomerAccount()
    {
        var factory = new AccountFactory(_generator);
        var disbursementAccountId = Guid.NewGuid();
        var repaymentAccountId = Guid.NewGuid();
        var product = CreateProduct(ProductType.LOAN, 365);

        var account = factory.Create(new AccountCreationData
        {
            AccountType = AccountType.CUSTOMER,
            Name = "Personal Loan",
            CustomerId = Guid.NewGuid(),
            ProductId = product.ProductId,
            Principal = 10_000m,
            DisbursementAccountId = disbursementAccountId,
            RepaymentAccountId = repaymentAccountId,
            RepaymentInstallments = 12
        }, product);

        var customerAccount = account.Should()
            .BeOfType<CustomerAccount>()
            .Subject;

        customerAccount.AccountNumber.Should().StartWith("BERLA");
        customerAccount.ProductType.Should().Be(ProductType.LOAN);
        customerAccount.Principal.Should().Be(10_000m);
        customerAccount.OutstandingPrincipal.Should().Be(10_000m);
        customerAccount.Balance.Should().Be(10_000m);
        customerAccount.DisbursementAccountId.Should()
            .Be(disbursementAccountId);
        customerAccount.RepaymentAccountId.Should().Be(repaymentAccountId);
        customerAccount.RepaymentInstallments.Should().Be(12);
        customerAccount.MaturityDate.Should().NotBeNull();
    }

    private static Product CreateProduct(
        ProductType productType,
        int? tenureInDays = null)
    {
        return Product.Create(
            productCode: $"product-{productType}",
            productName: $"{productType} Product",
            branchCode: "ber",
            currency: Currency.EUR,
            allowedCustomerType: CustomerType.ALL,
            interestRate: 0,
            interestPostPolicies: [],
            interestOfficeAccountId: Guid.NewGuid(),
            taxOfficeAccountId: Guid.NewGuid(),
            productType: productType,
            tenureInDays: tenureInDays);
    }
}
