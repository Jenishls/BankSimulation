using BankingConsole.Common;
using BankingConsole.DB;
using BankingConsole.Factories;
using BankingConsole.Models.Account;
using BankingConsole.Models.Enums;
using BankingConsole.Models.Product;
using BankingConsole.Models.ProductCreation;
using BankingConsole.Repository;
using BankingConsole.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BankingConsole.Tests;

public sealed class ProductServiceTests
{
    [Fact]
    public async Task CreateProductAsync_ValidatesAndSavesPostingAccounts()
    {
        var interestAccount = OfficeAccount.Create(
            "BEROFINTEREST",
            "Interest Expense",
            "BER",
            OfficeAccountType.EXPENSE);
        var taxAccount = OfficeAccount.Create(
            "BEROFTAX",
            "Tax Payable",
            "BER",
            OfficeAccountType.TAX_PAYABLE);

        var accountRepository = new Mock<IAccountRepository>();
        accountRepository
            .Setup(repository => repository.GetAccountByIdAsync(
                interestAccount.AccountId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(interestAccount);
        accountRepository
            .Setup(repository => repository.GetAccountByIdAsync(
                taxAccount.AccountId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(taxAccount);

        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork
            .Setup(work => work.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = new ProductService(
            new ProductFactory(),
            productRepository.Object,
            accountRepository.Object,
            unitOfWork.Object,
            Mock.Of<ILogger<ProductService>>());

        var product = await service.CreateProductAsync(
            CreateData(
                interestAccount.AccountId,
                taxAccount.AccountId));

        product.InterestOfficeAccountId.Should()
            .Be(interestAccount.AccountId);
        product.TaxOfficeAccountId.Should().Be(taxAccount.AccountId);
        productRepository.Verify(
            repository => repository.Add(product),
            Times.Once);
        unitOfWork.Verify(
            work => work.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateProductAsync_RejectsWrongTaxAccountType()
    {
        var interestAccount = OfficeAccount.Create(
            "BEROFINTEREST",
            "Interest Expense",
            "BER",
            OfficeAccountType.EXPENSE);
        var invalidTaxAccount = OfficeAccount.Create(
            "BEROFCASH",
            "Cash",
            "BER",
            OfficeAccountType.CASH);

        var accountRepository = new Mock<IAccountRepository>();
        accountRepository
            .Setup(repository => repository.GetAccountByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid accountId, CancellationToken _) =>
                accountId == interestAccount.AccountId
                    ? interestAccount
                    : invalidTaxAccount);

        var service = new ProductService(
            new ProductFactory(),
            Mock.Of<IProductRepository>(),
            accountRepository.Object,
            Mock.Of<IUnitOfWork>(),
            Mock.Of<ILogger<ProductService>>());

        var action = () => service.CreateProductAsync(
            CreateData(
                interestAccount.AccountId,
                invalidTaxAccount.AccountId));

        await action.Should().ThrowAsync<ValidationException>()
            .WithMessage("*TAX_PAYABLE*");
    }

    private static ProductCreationData CreateData(
        Guid interestAccountId,
        Guid taxAccountId)
    {
        return new ProductCreationData
        {
            ProductCode = "save-service",
            ProductName = "Service Savings",
            BranchCode = "BER",
            ProductType = ProductType.SAVING,
            Currency = Currency.EUR,
            AllowedCustomerType = CustomerType.ALL,
            InterestRate = 2m,
            InterestOfficeAccountId = interestAccountId,
            TaxOfficeAccountId = taxAccountId,
            InterestPostPolicies =
                [InterestPostPolicy.LAST_DAY_OFF_FREQUENCY],
            InterestPostingFrequency = Frequency.MONTHLY
        };
    }
}
