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

    public async Task<InterestPostingResult?> PostInterestIfDueAsync(
        CustomerAccount account,
        DateTime postingDate,
        decimal taxAmount = 0,
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

        if (!IsPostingDue(account, product, postingDate))
            return null;

        var grossInterest = account.InterestAccured;
        ValidateTax(product.InterestFlow, grossInterest, taxAmount);

        var postingAccount = await ResolvePostingAccountAsync(
            account,
            product,
            cancellationToken);
        var interestOfficeAccount =
            await ResolveOfficeAccountAsync(
                product.InterestOfficeAccountId,
                "Interest",
                cancellationToken);

        Transaction? taxTransaction = null;
        if (taxAmount > 0)
        {
            var taxOfficeAccount = await ResolveOfficeAccountAsync(
                product.TaxOfficeAccountId,
                "Tax",
                cancellationToken);
            var taxDescription =
                $"Interest tax debit for account " +
                $"{postingAccount.AccountNumber} on " +
                $"{postingDate:yyyy-MM-dd}. Tax: {taxAmount}.";

            taxTransaction = await CreateAndPostAsync(
                [
                    LedgerEntry.Create(
                        postingAccount,
                        Flow.DEBIT,
                        taxAmount,
                        product.Currency),
                    LedgerEntry.Create(
                        taxOfficeAccount,
                        Flow.CREDIT,
                        taxAmount,
                        product.Currency)
                ],
                EntryType.TAX,
                taxDescription,
                CreateIdempotencyKey(
                    account.AccountId,
                    postingDate,
                    "interest-tax"),
                cancellationToken);
        }

        var interestDescription =
            $"Gross interest posting for account " +
            $"{postingAccount.AccountNumber} on " +
            $"{postingDate:yyyy-MM-dd}. Interest: {grossInterest}.";
        var interestTransaction = await CreateAndPostAsync(
            [
                LedgerEntry.Create(
                    postingAccount,
                    product.InterestFlow,
                    grossInterest,
                    product.Currency),
                LedgerEntry.Create(
                    interestOfficeAccount,
                    Opposite(product.InterestFlow),
                    grossInterest,
                    product.Currency)
            ],
            EntryType.INTEREST,
            interestDescription,
            CreateIdempotencyKey(
                account.AccountId,
                postingDate,
                "interest"),
            cancellationToken,
            beforePost: () => account.MarkInterestPosted(postingDate));

        _logger.LogInformation(
            "Posted {GrossInterest} gross interest and {TaxAmount} " +
            "tax for source account {SourceAccountId} to posting " +
            "account {PostingAccountId}",
            grossInterest,
            taxAmount,
            account.AccountId,
            postingAccount.AccountId);

        return new InterestPostingResult(
            interestTransaction,
            taxTransaction);
    }

    private bool IsPostingDue(
        CustomerAccount account,
        Product product,
        DateTime postingDate)
    {
        var dueData = new IsDueResolverData
        {
            Frequency = product.InterestPostingFrequency,
            MaturityDate = account.MaturityDate,
            PostDate = product.PostDate,
            TodayDate = postingDate,
            LastInterestPostedOn = account.InterestPostedOn
        };

        return _dueResolver
            .Resolve(product.InterestPostPolicies)
            .Any(policy => policy.IsDue(dueData));
    }

    private async Task<Transaction> CreateAndPostAsync(
        List<LedgerEntry> entries,
        EntryType entryType,
        string description,
        Guid idempotencyKey,
        CancellationToken cancellationToken,
        Action? beforePost = null)
    {
        var transaction =
            await _transactionService.CreateTransactionAsync(
                entries,
                entryType,
                description,
                idempotencyKey,
                cancellationToken);

        if (transaction.State == TransactionState.POSTED)
        {
            beforePost?.Invoke();

            if (beforePost is not null)
                await _unitOfWork.SaveChangesAsync(cancellationToken);

            return transaction;
        }

        if (transaction.State != TransactionState.PENDING)
        {
            throw new ConflictException(
                $"Transaction {transaction.TransactionId} cannot be " +
                $"posted from state {transaction.State}.");
        }

        beforePost?.Invoke();

        return await _transactionService.PostTransactionAsync(
            transaction.TransactionId,
            description,
            cancellationToken);
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

    private async Task<OfficeAccount> ResolveOfficeAccountAsync(
        Guid accountId,
        string purpose,
        CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetAccountByIdAsync(
            accountId,
            cancellationToken);

        return account as OfficeAccount
            ?? throw new NotFoundException(
                $"{purpose} office account {accountId} was not found.");
    }

    private static void ValidateTax(
        Flow interestFlow,
        decimal grossInterest,
        decimal taxAmount)
    {
        if (taxAmount < 0 || taxAmount > grossInterest)
        {
            throw new ValidationException(
                "Tax must be between zero and the accrued interest.");
        }

        if (interestFlow == Flow.DEBIT && taxAmount > 0)
        {
            throw new ValidationException(
                "Withholding tax is only supported for credit interest.");
        }
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
        DateTime postingDate,
        string purpose)
    {
        var input = Encoding.UTF8.GetBytes(
            $"{accountId:N}:{purpose}:{postingDate:yyyyMMdd}");
        var hash = SHA256.HashData(input);

        return new Guid(hash.AsSpan(0, 16));
    }
}
