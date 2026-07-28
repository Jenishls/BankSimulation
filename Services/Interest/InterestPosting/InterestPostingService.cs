using System.Security.Cryptography;
using System.Text;
using BankingConsole.Common;
using BankingConsole.DB;
using BankingConsole.Models;
using BankingConsole.Models.Account;
using BankingConsole.Models.Enums;
using BankingConsole.Models.Interest;
using BankingConsole.Models.Product;
using BankingConsole.Repository;
using BankingConsole.Services.Interest.InterestDue;

namespace BankingConsole.Services.Interest.InterestPosting;

public sealed class InterestPostingService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IProductRepository _productRepository;
    private readonly IInterestPostResolver _dueResolver;
    private readonly TransactionService _transactionService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InterestPostingService> _logger;

    public InterestPostingService(
        IAccountRepository accountRepository,
        IProductRepository productRepository,
        IInterestPostResolver dueResolver,
        TransactionService transactionService,
        IUnitOfWork unitOfWork,
        ILogger<InterestPostingService> logger)
    {
        _accountRepository = accountRepository;
        _productRepository = productRepository;
        _dueResolver = dueResolver;
        _transactionService = transactionService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Transaction?> PostInterestIfDueAsync(
        CustomerAccount account,
        DateTime postingDate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(account);

        var product = await _productRepository.GetByIdAsync(
            account.ProductId,
            cancellationToken)
            ?? throw new NotFoundException(
                $"Product {account.ProductId} was not found.");

        if (account.InterestAccured <= 0 ||
            account.InterestPostedOn.Date >= postingDate.Date)
        {
            return null;
        }

        var dueData = new IsDueResolverData
        {
            Frequency = product.InterestPostingFrequency,
            MaturityDate = account.MaturityDate,
            PostDate = product.PostDate,
            TodayDate = postingDate,
            LastInterestPostedOn = account.InterestPostedOn
        };

        var isDue = _dueResolver
            .Resolve(product.InterestPostPolicies)
            .Any(policy => policy.IsDue(dueData));

        if (!isDue)
            return null;

        var postingAccount = await ResolvePostingAccountAsync(
            account,
            product,
            cancellationToken);

        var interestOfficeAccount =  await ResolveInterestOfficeAccountAsync(
                product,
                cancellationToken);

        var amount = account.InterestAccured;

        var idempotencyKey = CreateIdempotencyKey(account.AccountId,postingDate);

        var description =
            $"Transaction creation to post interest for account {account.AccountNumber} on {postingDate:yyyy-MM-dd}.";
        var entries = new List<LedgerEntry>
        {
            LedgerEntry.Create(
                postingAccount,
                product.InterestFlow,
                amount,
                product.Currency),

            LedgerEntry.Create(
                interestOfficeAccount,
                Opposite(product.InterestFlow),
                amount,
                product.Currency)
        };

        var transaction = await _transactionService.CreateTransactionAsync(
                entries,
                EntryType.INTEREST,
                description,
                idempotencyKey);

        if (transaction.State != TransactionState.PENDING)
        {
            throw new ConflictException(
                $"Interest transaction {transaction.TransactionId} " +
                $"cannot be posted from state {transaction.State}.");
        }

        if (transaction.State == TransactionState.POSTED)
        {
            account.MarkInterestPosted(postingDate);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return transaction;
        }

        account.MarkInterestPosted(postingDate);

        var postedTransaction = await _transactionService.UpdateTransactionAsync(
                transaction.TransactionId,
                description,
                idempotencyKey
                );

        _logger.LogInformation(
            "Posted {Amount} interest from account {SourceAccountId} " +
            "to customer posting account {PostingAccountId}",
            amount,
            account.AccountId,
            postingAccount.AccountId);

        return postedTransaction;
    }

    private async Task<CustomerAccount> ResolvePostingAccountAsync(
        CustomerAccount sourceAccount,
        Product product,
        CancellationToken cancellationToken)
    {
        if (!product.PostInterestToLinkedAccount)
            return sourceAccount;

        var linkedAccountId = product.ProductType switch
        {
            ProductType.TERM =>
                sourceAccount.MaturitySettlementAccountId,
            ProductType.LOAN =>
                sourceAccount.RepaymentAccountId,
            _ => null
        };

        if (linkedAccountId is null || linkedAccountId == Guid.Empty)
        {
            throw new ValidationException(
                "The product requires linked-account interest posting, " +
                "but the account has no applicable linked account.");
        }

        var linkedAccount = await _accountRepository
            .GetAccountByIdAsync(
                linkedAccountId.Value,
                cancellationToken);

        if (linkedAccount is not CustomerAccount customerAccount)
        {
            throw new NotFoundException(
                $"Linked customer account {linkedAccountId} was not found.");
        }

        if (customerAccount.CustomerId != sourceAccount.CustomerId)
        {
            throw new ValidationException(
                "The linked interest account must belong to the same customer.");
        }

        if (customerAccount.State != AccountState.ACTIVE)
        {
            throw new ValidationException(
                "The linked interest account must be active.");
        }

        return customerAccount;
    }

    private async Task<OfficeAccount> ResolveInterestOfficeAccountAsync(
        Product product,
        CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetAccountByIdAsync(
            product.InterestOfficeAccountId,
            cancellationToken);

        return account as OfficeAccount
            ?? throw new NotFoundException(
                $"Interest office account " +
                $"{product.InterestOfficeAccountId} was not found.");
    }

    private static Flow Opposite(Flow flow)
    {
        return flow switch
        {
            Flow.DEBIT => Flow.CREDIT,
            Flow.CREDIT => Flow.DEBIT,
            _ => throw new ArgumentOutOfRangeException(
                nameof(flow),
                flow,
                "Unsupported interest flow.")
        };
    }

    private static Guid CreateIdempotencyKey(
        Guid accountId,
        DateTime postingDate)
    {
        var input = Encoding.UTF8.GetBytes(
            $"{accountId:N}:interest:{postingDate:yyyyMMdd}");
        var hash = SHA256.HashData(input);

        return new Guid(hash.AsSpan(0, 16));
    }
}
