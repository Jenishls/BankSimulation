using BankingConsole.Factories;
using BankingConsole.Models.Account;
using BankingConsole.Models.AccountCreation;
using BankingConsole.Models.Enums;
using BankingConsole.Models.Product;
using BankingConsole.Models.ProductCreation;
using BankingConsole.Services.Interest.InterestDue;
using FluentAssertions;

namespace BankingConsole.Tests;

public sealed class ProductFactoryTests
{
    private readonly ProductFactory _factory = new();

    [Fact]
    public void Create_SavingProduct_UsesSharedProductFields()
    {
        var product = _factory.Create(new ProductCreationData
        {
            ProductCode = "save-01",
            ProductName = "Everyday Savings",
            ProductType = ProductType.SAVING,
            Currency = Currency.EUR,
            AllowedCustomerType = CustomerType.ALL,
            InterestRate = 4.5m,
            InterestPostPolicies =
                [InterestPostPolicy.LastDayOfFrequency],
            InterestPostingFrequency = Frequency.Monthly
        });

        var savingProduct = product.Should()
            .BeOfType<SavingProduct>()
            .Subject;

        savingProduct.ProductCode.Should().Be("SAVE-01");
        savingProduct.ProductType.Should().Be(ProductType.SAVING);
        savingProduct.InterestFlow.Should().Be(Flow.CREDIT);
        savingProduct.InterestRate.Should().Be(4.5m);
    }

    [Fact]
    public void Create_TermProduct_SupportsMaturityPosting()
    {
        var product = _factory.Create(new ProductCreationData
        {
            ProductCode = "term-01",
            ProductName = "One Year Deposit",
            ProductType = ProductType.TERM,
            Currency = Currency.EUR,
            AllowedCustomerType = CustomerType.INDIVIDUAL,
            InterestRate = 5m,
            InterestPostPolicies = [InterestPostPolicy.Maturity],
            TenureInDays = 365,
            TransferCount = 1,
            TransferFrequency = Frequency.Yearly
        });

        var termProduct = product.Should()
            .BeOfType<TermProduct>()
            .Subject;

        termProduct.ProductType.Should().Be(ProductType.TERM);
        termProduct.InterestFlow.Should().Be(Flow.CREDIT);
        termProduct.InterestPostingFrequency.Should().BeNull();
        termProduct.TenureInDays.Should().Be(365);
    }

    [Fact]
    public void Create_LoanProduct_UsesDebitInterestFlow()
    {
        var product = _factory.Create(new ProductCreationData
        {
            ProductCode = "loan-01",
            ProductName = "Personal Loan",
            ProductType = ProductType.LOAN,
            Currency = Currency.EUR,
            AllowedCustomerType = CustomerType.INDIVIDUAL,
            InterestRate = 8m,
            InterestPostPolicies =
                [InterestPostPolicy.LastDayOfFrequency],
            InterestPostingFrequency = Frequency.Monthly,
            TenureInDays = 365,
            RepaymentCount = 12,
            RepaymentFrequency = Frequency.Monthly
        });

        var loanProduct = product.Should()
            .BeOfType<LoanProduct>()
            .Subject;

        loanProduct.ProductType.Should().Be(ProductType.LOAN);
        loanProduct.InterestFlow.Should().Be(Flow.DEBIT);
        loanProduct.RepaymentCount.Should().Be(12);
    }

    [Fact]
    public void Create_OfficeProduct_UsesConfiguredInternalFlow()
    {
        var product = _factory.Create(new ProductCreationData
        {
            ProductCode = "office-01",
            ProductName = "Interest Expense",
            ProductType = ProductType.OFFICE,
            Currency = Currency.EUR,
            OfficeInterestFlow = Flow.DEBIT
        });

        var officeProduct = product.Should()
            .BeOfType<OfficeProduct>()
            .Subject;

        officeProduct.ProductType.Should().Be(ProductType.OFFICE);
        officeProduct.InterestFlow.Should().Be(Flow.DEBIT);
        officeProduct.InterestRate.Should().Be(0);
        officeProduct.InterestPostPolicies.Should().BeEmpty();
    }

    [Fact]
    public void AccountFactory_CreateOfficeAccount_HasNoCustomerOwner()
    {
        var factory = new AccountFactory();

        var account = factory.Create(new AccountCreationData
        {
            ProductId = Guid.NewGuid(),
            ProductType = ProductType.OFFICE,
            BranchCode = "BER",
            Office = new OfficeAccountCreationData
            {
                OpeningBalance = 100m
            }
        });

        var officeAccount = account.Should()
            .BeOfType<OfficeAccount>()
            .Subject;

        officeAccount.CustomerId.Should().BeNull();
        officeAccount.Balance.Should().Be(100m);
        officeAccount.AccountNumber.Should().StartWith("BEROF");
    }

    [Fact]
    public void Resolver_ReturnsImplementationsConfiguredByProduct()
    {
        var resolver = new InterestPostResolver(
            [
                new LastDayOfFrequencyPolicy(),
                new MaturityPolicy(),
                new PostDatePolicy()
            ]);

        var policies = resolver.Resolve(
            [
                InterestPostPolicy.LastDayOfFrequency,
                InterestPostPolicy.Maturity
            ]);

        policies.Should().ContainSingle(
            policy => policy is LastDayOfFrequencyPolicy);
        policies.Should().ContainSingle(
            policy => policy is MaturityPolicy);
        policies.Should().NotContain(
            policy => policy is PostDatePolicy);
    }
}
