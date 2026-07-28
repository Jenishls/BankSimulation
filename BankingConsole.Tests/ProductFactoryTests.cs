using BankingConsole.Factories;
using BankingConsole.Models.Enums;
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
            BranchCode = "ber",
            ProductType = ProductType.SAVING,
            Currency = Currency.EUR,
            AllowedCustomerType = CustomerType.ALL,
            InterestRate = 4.5m,
            InterestOfficeAccountId = Guid.NewGuid(),
            TaxOfficeAccountId = Guid.NewGuid(),
            InterestPostPolicies =
                [InterestPostPolicy.LAST_DAY_OFF_FREQUENCY],
            InterestPostingFrequency = Frequency.MONTHLY
        });

        product.ProductCode.Should().Be("SAVE-01");
        product.BranchCode.Should().Be("BER");
        product.ProductType.Should().Be(ProductType.SAVING);
        product.InterestFlow.Should().Be(Flow.CREDIT);
        product.InterestRate.Should().Be(4.5m);
        product.IsMaturityProduct.Should().BeFalse();
    }

    [Fact]
    public void Create_FixedDeposit_SupportsMaturityPosting()
    {
        var product = _factory.Create(new ProductCreationData
        {
            ProductCode = "term-01",
            ProductName = "One Year Fixed Deposit",
            BranchCode = "ber",
            ProductType = ProductType.TERM,
            Currency = Currency.EUR,
            AllowedCustomerType = CustomerType.INDIVIDUAL,
            InterestRate = 5m,
            InterestOfficeAccountId = Guid.NewGuid(),
            TaxOfficeAccountId = Guid.NewGuid(),
            InterestPostPolicies =
                [InterestPostPolicy.ON_MATURITY],
            TenureInDays = 365
        });

        product.ProductType.Should().Be(ProductType.TERM);
        product.InterestFlow.Should().Be(Flow.CREDIT);
        product.InterestPostingFrequency.Should().BeNull();
        product.TenureInDays.Should().Be(365);
        product.IsMaturityProduct.Should().BeTrue();
    }

    [Fact]
    public void Create_LoanProduct_UsesDebitInterestFlow()
    {
        var product = _factory.Create(new ProductCreationData
        {
            ProductCode = "loan-01",
            ProductName = "Personal Loan",
            BranchCode = "ber",
            ProductType = ProductType.LOAN,
            Currency = Currency.EUR,
            AllowedCustomerType = CustomerType.INDIVIDUAL,
            InterestRate = 8m,
            InterestOfficeAccountId = Guid.NewGuid(),
            TaxOfficeAccountId = Guid.NewGuid(),
            InterestPostPolicies =
                [InterestPostPolicy.LAST_DAY_OFF_FREQUENCY],
            InterestPostingFrequency = Frequency.MONTHLY,
            TenureInDays = 365
        });

        product.ProductType.Should().Be(ProductType.LOAN);
        product.InterestFlow.Should().Be(Flow.DEBIT);
        product.IsMaturityProduct.Should().BeTrue();
    }

    [Fact]
    public void Create_Product_KeepsSingleInterestAndTaxOfficeAccounts()
    {
        var interestAccountId = Guid.NewGuid();
        var taxAccountId = Guid.NewGuid();

        var product = _factory.Create(new ProductCreationData
        {
            ProductCode = "save-accounts",
            ProductName = "Savings With Posting Accounts",
            BranchCode = "ber",
            ProductType = ProductType.SAVING,
            Currency = Currency.EUR,
            AllowedCustomerType = CustomerType.ALL,
            InterestRate = 2m,
            InterestOfficeAccountId = interestAccountId,
            TaxOfficeAccountId = taxAccountId,
            TaxRate = 15m,
            InterestPostPolicies =
                [InterestPostPolicy.LAST_DAY_OFF_FREQUENCY],
            InterestPostingFrequency = Frequency.MONTHLY
        });

        product.InterestOfficeAccountId.Should().Be(interestAccountId);
        product.TaxOfficeAccountId.Should().Be(taxAccountId);
        product.TaxRate.Should().Be(15m);
    }

    [Fact]
    public void Create_LoanProduct_RejectsWithholdingTax()
    {
        var action = () => _factory.Create(new ProductCreationData
        {
            ProductCode = "loan-tax",
            ProductName = "Loan With Invalid Withholding Tax",
            BranchCode = "ber",
            ProductType = ProductType.LOAN,
            Currency = Currency.EUR,
            AllowedCustomerType = CustomerType.ALL,
            InterestRate = 8m,
            InterestOfficeAccountId = Guid.NewGuid(),
            TaxOfficeAccountId = Guid.NewGuid(),
            TaxRate = 10m,
            InterestPostPolicies =
                [InterestPostPolicy.LAST_DAY_OFF_FREQUENCY],
            InterestPostingFrequency = Frequency.MONTHLY,
            TenureInDays = 365
        });

        action.Should().Throw<ArgumentException>()
            .WithMessage("*debit-interest*");
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
                InterestPostPolicy.LAST_DAY_OFF_FREQUENCY,
                InterestPostPolicy.ON_MATURITY
            ]);

        policies.Should().ContainSingle(
            policy => policy is LastDayOfFrequencyPolicy);
        policies.Should().ContainSingle(
            policy => policy is MaturityPolicy);
        policies.Should().NotContain(
            policy => policy is PostDatePolicy);
    }
}
