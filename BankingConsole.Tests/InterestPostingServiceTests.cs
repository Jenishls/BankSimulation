using BankingConsole.DB;
using BankingConsole.Models;
using BankingConsole.Models.Account;
using BankingConsole.Models.Enums;
using BankingConsole.Models.Product;
using BankingConsole.Repository;
using BankingConsole.Services;
using BankingConsole.Services.Interest.InterestDue;
using BankingConsole.Services.Interest.InterestPosting;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BankingConsole.Tests;

public sealed class InterestPostingServiceTests
{
    [Fact]
    public async Task PostAccountInterestIfDue_PostsToSourceAccount()
    {
        var customerId = Guid.NewGuid();
        var interestOfficeAccount = OfficeAccount.Create(
            "BEROFINTEREST",
            "Interest Expense",
            "BER",
            OfficeAccountType.EXPENSE);
        var product = CreateProduct(
            ProductType.SAVING,
            interestOfficeAccount.AccountId);
        var account = CustomerAccount.Create(
            "BERSA0001",
            "Savings",
            customerId,
            product.ProductId,
            ProductType.SAVING,
            "BER",
            openingBalance: 1_000m);
        account.IncreaseInterestAccured(10m);

        var accountRepository = new Mock<IAccountRepository>();
        accountRepository
            .Setup(repository => repository.GetAccountByIdAsync(
                interestOfficeAccount.AccountId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(interestOfficeAccount);

        var service = CreateService(
            accountRepository,
            product,
            [new LastDayOfFrequencyPolicy()]);

        var postingDate = account.InterestPostedOn.Date.AddDays(1);
        var transaction =
            await service.PostInterestIfDueAsync(
                account,
                postingDate);

        transaction.Should().NotBeNull();
        transaction!.State.Should().Be(TransactionState.POSTED);
        transaction.Type.Should().Be(EntryType.INTEREST);
        transaction.Entries.Should().ContainSingle(entry =>
            entry.Account == account &&
            entry.Flow == Flow.CREDIT &&
            entry.Amount == 10m);
        transaction.Entries.Should().ContainSingle(entry =>
            entry.Account == interestOfficeAccount &&
            entry.Flow == Flow.DEBIT &&
            entry.Amount == 10m);
        account.Balance.Should().Be(1_010m);
        account.InterestAccured.Should().Be(0);
        account.InterestPostedOn.Should().Be(postingDate);
    }

    [Fact]
    public async Task PostAccountInterestIfDue_PostsToLinkedAccount()
    {
        var customerId = Guid.NewGuid();
        var settlementProductId = Guid.NewGuid();
        var settlementAccount = CustomerAccount.Create(
            "BERSA0002",
            "Settlement Savings",
            customerId,
            settlementProductId,
            ProductType.SAVING,
            "BER",
            openingBalance: 100m);
        var interestOfficeAccount = OfficeAccount.Create(
            "BEROFINTEREST",
            "Interest Expense",
            "BER",
            OfficeAccountType.EXPENSE);
        var product = CreateProduct(
            ProductType.TERM,
            interestOfficeAccount.AccountId,
            postInterestToLinkedAccount: true,
            tenureInDays: 30);
        var termAccount = CustomerAccount.Create(
            "BERTD0001",
            "Term Deposit",
            customerId,
            product.ProductId,
            ProductType.TERM,
            "BER",
            principal: 5_000m,
            tenureInDays: 30,
            fundingAccountId: settlementAccount.AccountId,
            maturitySettlementAccountId:
                settlementAccount.AccountId);
        termAccount.IncreaseInterestAccured(25m);

        var accountRepository = new Mock<IAccountRepository>();
        accountRepository
            .Setup(repository => repository.GetAccountByIdAsync(
                settlementAccount.AccountId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(settlementAccount);
        accountRepository
            .Setup(repository => repository.GetAccountByIdAsync(
                interestOfficeAccount.AccountId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(interestOfficeAccount);

        var service = CreateService(
            accountRepository,
            product,
            [new MaturityPolicy()]);

        var postingDate = termAccount.MaturityDate!.Value;
        var transaction =
            await service.PostInterestIfDueAsync(
                termAccount,
                postingDate);

        transaction.Should().NotBeNull();
        transaction!.Entries.Should().ContainSingle(entry =>
            entry.Account == settlementAccount &&
            entry.Flow == Flow.CREDIT &&
            entry.Amount == 25m);
        termAccount.Balance.Should().Be(5_000m);
        settlementAccount.Balance.Should().Be(125m);
        termAccount.InterestAccured.Should().Be(0);
        termAccount.InterestPostedOn.Should().Be(postingDate);
    }

    [Fact]
    public async Task PostAccountInterestIfDue_WhenNotDue_DoesNothing()
    {
        var interestOfficeAccount = OfficeAccount.Create(
            "BEROFINTEREST",
            "Interest Expense",
            "BER",
            OfficeAccountType.EXPENSE);
        var product = CreateProduct(
            ProductType.SAVING,
            interestOfficeAccount.AccountId,
            frequency: Frequency.MONTHLY);
        var account = CustomerAccount.Create(
            "BERSA0003",
            "Savings",
            Guid.NewGuid(),
            product.ProductId,
            ProductType.SAVING,
            "BER",
            openingBalance: 1_000m);
        account.IncreaseInterestAccured(10m);

        var service = CreateService(
            new Mock<IAccountRepository>(),
            product,
            [new LastDayOfFrequencyPolicy()]);

        var postingDate = account.InterestPostedOn.Date.AddDays(1);
        if (postingDate.AddDays(1).Day == 1)
            postingDate = postingDate.AddDays(1);

        var transaction =
            await service.PostInterestIfDueAsync(
                account,
                postingDate);

        transaction.Should().BeNull();
        account.InterestAccured.Should().Be(10m);
        account.Balance.Should().Be(1_000m);
    }

    private static InterestPostingService CreateService(
        Mock<IAccountRepository> accountRepository,
        Product product,
        IEnumerable<IInterestPostPolicy> policies)
    {
        Transaction? storedTransaction = null;
        var transactionRepository =
            new Mock<ITransactionRepository>();
        transactionRepository
            .Setup(repository => repository
                .GetByIdempotencyKeyAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Transaction?)null);
        transactionRepository
            .Setup(repository => repository.Add(
                It.IsAny<Transaction>()))
            .Callback<Transaction>(transaction =>
                storedTransaction = transaction);
        transactionRepository
            .Setup(repository => repository.GetByIdAsync(
                It.IsAny<Guid>()))
            .ReturnsAsync(() => storedTransaction);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork
            .Setup(work => work.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var transactionService = new TransactionService(
            transactionRepository.Object,
            accountRepository.Object,
            Mock.Of<ITransactionActionRepository>(),
            unitOfWork.Object,
            Mock.Of<ILogger<TransactionService>>());

        var productRepository = new Mock<IProductRepository>();
        productRepository
            .Setup(repository => repository.GetByIdAsync(
                product.ProductId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        return new InterestPostingService(
            accountRepository.Object,
            productRepository.Object,
            new InterestPostResolver(policies),
            transactionService,
            unitOfWork.Object,
            Mock.Of<ILogger<InterestPostingService>>());
    }

    private static Product CreateProduct(
        ProductType productType,
        Guid interestOfficeAccountId,
        bool postInterestToLinkedAccount = false,
        int? tenureInDays = null,
        Frequency frequency = Frequency.DAILY)
    {
        var policies = productType == ProductType.TERM
            ? new[] { InterestPostPolicy.ON_MATURITY }
            : new[]
            {
                InterestPostPolicy.LAST_DAY_OFF_FREQUENCY
            };

        return Product.Create(
            productCode: $"interest-{productType}",
            productName: $"{productType} Interest",
            branchCode: "BER",
            currency: Currency.EUR,
            allowedCustomerType: CustomerType.ALL,
            interestRate: 2m,
            interestPostPolicies: policies,
            interestOfficeAccountId: interestOfficeAccountId,
            taxOfficeAccountId: Guid.NewGuid(),
            productType: productType,
            postInterestToLinkedAccount:
                postInterestToLinkedAccount,
            interestPostingFrequency:
                productType == ProductType.TERM
                    ? null
                    : frequency,
            tenureInDays: tenureInDays);
    }
}
