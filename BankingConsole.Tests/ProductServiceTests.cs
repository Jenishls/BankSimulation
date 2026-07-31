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

    [Fact]
    public async Task UpdateProductAsync_ValidatesAndSavesChanges()
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
        var product = new ProductFactory().Create(
            CreateData(
                interestAccount.AccountId,
                taxAccount.AccountId));

        var productRepository = new Mock<IProductRepository>();
        productRepository
            .Setup(repository => repository.GetByIdAsync(
                product.ProductId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var accountRepository = new Mock<IAccountRepository>();
        accountRepository
            .Setup(repository => repository.GetAccountByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid accountId, CancellationToken _) =>
                accountId == interestAccount.AccountId
                    ? interestAccount
                    : taxAccount);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork
            .Setup(work => work.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = CreateService(
            productRepository,
            accountRepository,
            unitOfWork);

        var updated = await service.UpdateProductAsync(
            product.ProductId,
            CreateData(
                interestAccount.AccountId,
                taxAccount.AccountId,
                "Updated Savings"));

        updated.ProductId.Should().Be(product.ProductId);
        updated.ProductName.Should().Be("Updated Savings");
        productRepository.Verify(
            repository => repository.Update(product),
            Times.Once);
        unitOfWork.Verify(
            work => work.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteProductAsync_DeletesProductWithoutAccounts()
    {
        var product = new ProductFactory().Create(
            CreateData(Guid.NewGuid(), Guid.NewGuid()));
        var productRepository = new Mock<IProductRepository>();
        productRepository
            .Setup(repository => repository.GetByIdAsync(
                product.ProductId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var accountRepository = new Mock<IAccountRepository>();
        accountRepository
            .Setup(repository => repository.GetAccountsByProductIdAsync(
                product.ProductId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork
            .Setup(work => work.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = CreateService(
            productRepository,
            accountRepository,
            unitOfWork);

        await service.DeleteProductAsync(product.ProductId);

        productRepository.Verify(
            repository => repository.Delete(product),
            Times.Once);
        unitOfWork.Verify(
            work => work.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteProductAsync_RejectsProductWithAccounts()
    {
        var product = new ProductFactory().Create(
            CreateData(Guid.NewGuid(), Guid.NewGuid()));
        var account = CustomerAccount.Create(
            "BERSA000000000001",
            "Savings",
            Guid.NewGuid(),
            product.ProductId,
            ProductType.SAVING,
            "BER");
        var productRepository = new Mock<IProductRepository>();
        productRepository
            .Setup(repository => repository.GetByIdAsync(
                product.ProductId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var accountRepository = new Mock<IAccountRepository>();
        accountRepository
            .Setup(repository => repository.GetAccountsByProductIdAsync(
                product.ProductId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([account]);

        var service = CreateService(
            productRepository,
            accountRepository,
            new Mock<IUnitOfWork>());

        var action = () => service.DeleteProductAsync(product.ProductId);

        await action.Should().ThrowAsync<ConflictException>()
            .WithMessage("*with accounts*");
        productRepository.Verify(
            repository => repository.Delete(It.IsAny<Product>()),
            Times.Never);
    }

    private static ProductService CreateService(
        Mock<IProductRepository> productRepository,
        Mock<IAccountRepository> accountRepository,
        Mock<IUnitOfWork> unitOfWork)
    {
        return new ProductService(
            new ProductFactory(),
            productRepository.Object,
            accountRepository.Object,
            unitOfWork.Object,
            Mock.Of<ILogger<ProductService>>());
    }

    private static ProductCreationData CreateData(
        Guid interestAccountId,
        Guid taxAccountId,
        string productName = "Service Savings")
    {
        return new ProductCreationData
        {
            ProductCode = "save-service",
            ProductName = productName,
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
