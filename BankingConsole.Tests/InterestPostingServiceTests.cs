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
        var taxOfficeAccount = OfficeAccount.Create(
            "BEROFTAX",
            "Tax Payable",
            "BER",
            OfficeAccountType.TAX_PAYABLE);
        var product = CreateProduct(
            ProductType.SAVING,
            interestOfficeAccount.AccountId,
            taxOfficeAccountId: taxOfficeAccount.AccountId);
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
        accountRepository
            .Setup(repository => repository.GetAccountByIdAsync(
                taxOfficeAccount.AccountId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(taxOfficeAccount);

        var service = CreateService(
            accountRepository,
            product,
            [new LastDayOfFrequencyPolicy()]);

        var postingDate = account.InterestPostedOn.Date.AddDays(1);
        var result =
            await service.PostInterestIfDueAsync(
                account,
                postingDate,
                taxAmount: 2m);

        result.Should().NotBeNull();
        var interestTransaction = result!.InterestTransaction;
        var taxTransaction = result.TaxTransaction;

        interestTransaction.State.Should()
            .Be(TransactionState.POSTED);
        interestTransaction.Type.Should().Be(EntryType.INTEREST);
        interestTransaction.Entries.Should().ContainSingle(entry =>
            entry.Account == account &&
            entry.Flow == Flow.CREDIT &&
            entry.Amount == 10m);
        interestTransaction.Entries.Should().ContainSingle(entry =>
            entry.Account == interestOfficeAccount &&
            entry.Flow == Flow.DEBIT &&
            entry.Amount == 10m);
        taxTransaction.Should().NotBeNull();
        taxTransaction!.Type.Should().Be(EntryType.TAX);
        taxTransaction.Entries.Should().ContainSingle(entry =>
            entry.Account == account &&
            entry.Flow == Flow.DEBIT &&
            entry.Amount == 2m);
        taxTransaction.Entries.Should().ContainSingle(entry =>
            entry.Account == taxOfficeAccount &&
            entry.Flow == Flow.CREDIT &&
            entry.Amount == 2m);
        account.Balance.Should().Be(1_008m);
        taxOfficeAccount.Balance.Should().Be(2m);
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
        var result =
            await service.PostInterestIfDueAsync(
                termAccount,
                postingDate);

        result.Should().NotBeNull();
        result!.TaxTransaction.Should().BeNull();
        result.InterestTransaction.Entries.Should()
            .ContainSingle(entry =>
            entry.Account == settlementAccount &&
            entry.Flow == Flow.CREDIT &&
            entry.Amount == 25m);
        termAccount.Balance.Should().Be(5_000m);
        settlementAccount.Balance.Should().Be(125m);
        termAccount.InterestAccured.Should().Be(0);
        termAccount.InterestPostedOn.Should().Be(postingDate);
    }

    [Fact]
    public async Task PostInterestIfDue_DebitFlowDoesNotApplyTax()
    {
        var customerId = Guid.NewGuid();
        var settlementAccount = CustomerAccount.Create(
            "BERSA0004",
            "Loan Settlement",
            customerId,
            Guid.NewGuid(),
            ProductType.SAVING,
            "BER",
            openingBalance: 100m);
        var interestOfficeAccount = OfficeAccount.Create(
            "BEROFINCOME",
            "Interest Income",
            "BER",
            OfficeAccountType.INCOME);
        var taxOfficeAccount = OfficeAccount.Create(
            "BEROFTAXLOAN",
            "Tax Payable",
            "BER",
            OfficeAccountType.TAX_PAYABLE);
        var product = CreateProduct(
            ProductType.LOAN,
            interestOfficeAccount.AccountId,
            tenureInDays: 30,
            taxOfficeAccountId: taxOfficeAccount.AccountId);
        var loanAccount = CustomerAccount.Create(
            "BERLA0001",
            "Loan",
            customerId,
            product.ProductId,
            ProductType.LOAN,
            "BER",
            principal: 1_000m,
            tenureInDays: 30,
            disbursementAccountId: settlementAccount.AccountId,
            repaymentAccountId: settlementAccount.AccountId,
            repaymentInstallments: 3);
        loanAccount.IncreaseInterestAccured(10m);

        var accountRepository = new Mock<IAccountRepository>();
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

        var service = CreateService(
            accountRepository,
            product,
            [new LastDayOfFrequencyPolicy()]);
        var postingDate =
            loanAccount.InterestPostedOn.Date.AddDays(1);

        var result = await service.PostInterestIfDueAsync(
            loanAccount,
            postingDate);

        result.Should().NotBeNull();
        result!.TaxTransaction.Should().BeNull();
        result.InterestTransaction.Entries.Should()
            .ContainSingle(entry =>
            entry.Account == loanAccount &&
            entry.Flow == Flow.DEBIT &&
            entry.Amount == 10m);
        result.InterestTransaction.Entries.Should()
            .ContainSingle(entry =>
            entry.Account == interestOfficeAccount &&
            entry.Flow == Flow.CREDIT &&
            entry.Amount == 10m);
        taxOfficeAccount.Balance.Should().Be(0);
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

        var result =
            await service.PostInterestIfDueAsync(
                account,
                postingDate);

        result.Should().BeNull();
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
        Frequency frequency = Frequency.DAILY,
        Guid? taxOfficeAccountId = null)
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
            taxOfficeAccountId:
                taxOfficeAccountId ?? Guid.NewGuid(),
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
