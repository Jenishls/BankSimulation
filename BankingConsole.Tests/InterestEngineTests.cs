using BankingConsole.DB;
using BankingConsole.Models;
using BankingConsole.Models.Account;
using BankingConsole.Models.Enums;
using BankingConsole.Models.Product;
using BankingConsole.Repository;
using BankingConsole.Services;
using BankingConsole.Services.Interest;
using BankingConsole.Services.Interest.InterestCalculation;
using BankingConsole.Services.Interest.InterestDue;
using BankingConsole.Services.Interest.InterestPosting;
using BankingConsole.Services.Interest.InterestTax;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BankingConsole.Tests;

public sealed class InterestEngineTests
{
    [Fact]
    public async Task ExecuteAsync_CalculatesTaxesAndPostsInterest()
    {
        var interestOfficeAccount = OfficeAccount.Create(
            "BEROFINTEREST",
            "Interest Expense",
            "BER",
            OfficeAccountType.EXPENSE);
        var taxOfficeAccount = OfficeAccount.Create(
            "BEROFTAX",
            "Tax Payable",
            "BER",
            OfficeAccountType.TAX_PAYABLE);
        var product = Product.Create(
            productCode: "engine-saving",
            productName: "Engine Savings",
            branchCode: "BER",
            currency: Currency.EUR,
            allowedCustomerType: CustomerType.ALL,
            interestRate: 36.5m,
            interestPostPolicies:
                [InterestPostPolicy.LAST_DAY_OFF_FREQUENCY],
            interestOfficeAccountId:
                interestOfficeAccount.AccountId,
            taxOfficeAccountId: taxOfficeAccount.AccountId,
            productType: ProductType.SAVING,
            taxRate: 10m,
            interestPostingFrequency: Frequency.DAILY);
        var account = CustomerAccount.Create(
            "BERSAENGINE",
            "Engine Savings Account",
            Guid.NewGuid(),
            product.ProductId,
            ProductType.SAVING,
            "BER",
            openingBalance: 1_000m);

        var productRepository = new Mock<IProductRepository>();
        productRepository
            .Setup(repository => repository.GetAllAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([product]);
        productRepository
            .Setup(repository => repository.GetByIdAsync(
                product.ProductId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var accountRepository = new Mock<IAccountRepository>();
        accountRepository
            .Setup(repository => repository
                .GetActiveCustomerAccountsByProductIdAsync(
                    product.ProductId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync([account]);
        accountRepository
            .Setup(repository => repository.GetAccountByIdAsync(
                interestOfficeAccount.AccountId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(interestOfficeAccount);
        accountRepository
            .Setup(repository => repository.GetAccountByIdAsync(
                taxOfficeAccount.AccountId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(taxOfficeAccount);

        var storedTransactions = new List<Transaction>();
        var transactionRepository =
            new Mock<ITransactionRepository>();
        transactionRepository
            .Setup(repository => repository
                .GetByIdempotencyKeyAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync((Transaction?)null);
        transactionRepository
            .Setup(repository => repository.Add(
                It.IsAny<Transaction>()))
            .Callback<Transaction>(transaction =>
                storedTransactions.Add(transaction));
        transactionRepository
            .Setup(repository => repository.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((
                Guid transactionId,
                CancellationToken _) =>
                storedTransactions.Single(transaction =>
                    transaction.TransactionId == transactionId));

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork
            .Setup(work => work.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var transactionService = new TransactionService(
            transactionRepository.Object,
            Mock.Of<ITransactionActionRepository>(),
            unitOfWork.Object,
            Mock.Of<ILogger<TransactionService>>());
        var postingService = new InterestPostingService(
            accountRepository.Object,
            productRepository.Object,
            new InterestPostResolver(
                [new LastDayOfFrequencyPolicy()]),
            transactionService,
            unitOfWork.Object,
            Mock.Of<ILogger<InterestPostingService>>());
        var calculationService =
            new InterestCalculationService(
                new SimpleInterestCalculator(),
                Mock.Of<ILogger<InterestCalculationService>>());
        var engine = new InterestEngine(
            productRepository.Object,
            accountRepository.Object,
            calculationService,
            new InterestTaxCalculator(),
            postingService,
            unitOfWork.Object,
            Mock.Of<ILogger<InterestEngine>>());

        var processingDate =
            account.AccountOpenDate.Date.AddDays(1);
        var results = await engine.ExecuteAsync(processingDate);

        results.Should().ContainSingle();
        var result = results.Single();
        result.DailyInterest.Should().Be(1m);
        result.AccruedInterestBeforePosting.Should().Be(1m);
        result.TaxAmount.Should().Be(0.10m);
        result.TaxTransactionId.Should().NotBeNull();
        result.TaxTransactionState.Should()
            .Be(TransactionState.POSTED);
        result.InterestTransactionId.Should().NotBeNull();
        result.InterestTransactionState.Should()
            .Be(TransactionState.POSTED);

        account.Balance.Should().Be(1_000.90m);
        account.InterestAccured.Should().Be(0);
        account.InterestCalculatedOn.Should().Be(processingDate);
        account.InterestPostedOn.Should().Be(processingDate);
        interestOfficeAccount.Balance.Should().Be(-1m);
        taxOfficeAccount.Balance.Should().Be(0.10m);

        storedTransactions.Should().HaveCount(2);
        storedTransactions[0].Type.Should().Be(EntryType.TAX);
        storedTransactions[0].Entries.Should().ContainSingle(entry =>
            entry.Account == account &&
            entry.Flow == Flow.DEBIT &&
            entry.Amount == 0.10m);
        storedTransactions[0].Entries.Should().ContainSingle(entry =>
            entry.Account == taxOfficeAccount &&
            entry.Flow == Flow.CREDIT &&
            entry.Amount == 0.10m);
        storedTransactions[1].Type.Should().Be(EntryType.INTEREST);
        storedTransactions[1].Entries.Should().ContainSingle(entry =>
            entry.Account == account &&
            entry.Flow == Flow.CREDIT &&
            entry.Amount == 1m);

        var secondRun = await engine.ExecuteAsync(processingDate);

        secondRun.Should().ContainSingle();
        secondRun.Single().DailyInterest.Should().Be(0);
        secondRun.Single().TaxAmount.Should().Be(0);
        secondRun.Single().TaxTransactionId.Should().BeNull();
        secondRun.Single().InterestTransactionId.Should().BeNull();
        account.Balance.Should().Be(1_000.90m);
        taxOfficeAccount.Balance.Should().Be(0.10m);
    }
}
