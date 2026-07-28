using BankingConsole.Models.Customer;
using BankingConsole.Common;
using BankingConsole.Models.Enums;
using BankingConsole.Factories;
using BankingConsole.Repository;
using BankingConsole.DB;
using BankingConsole.Models.AccountCreation;
using BankingConsole.Models.Account;
using ProductEntity = BankingConsole.Models.Product.Product;

namespace BankingConsole.Services;

public class AccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccountFactory _accountFactory;
    private readonly ILogger<AccountService> _logger;

    public AccountService(
        IAccountRepository accountRepository,
        ICustomerRepository customerRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        IAccountFactory accountFactory,
        ILogger<AccountService> logger
        )
    {
        _accountRepository = accountRepository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _accountFactory = accountFactory;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Account> CreateAccount(
        AccountCreationData data,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);

        _logger.LogInformation(
            "Account creation initiated"
        );

        ProductEntity? product = null;

        if (data.AccountType == AccountType.CUSTOMER)
        {
            product = await ValidateCustomerAccountRequestAsync(
                data,
                cancellationToken);
        }

        var account = _accountFactory.Create(data, product);

        _accountRepository.AddAccount(account);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return account;
    }

    private async Task<ProductEntity>
        ValidateCustomerAccountRequestAsync(
            AccountCreationData data,
            CancellationToken cancellationToken)
    {
        var customerId = data.CustomerId
            ?? throw new ValidationException(
                "Customer id is required for a customer account.");

        var productId = data.ProductId
            ?? throw new ValidationException(
                "Product id is required for a customer account.");

        var customer = await _customerRepository.GetByIdAsync(customerId)
            ?? throw new NotFoundException(
                $"Customer {customerId} was not found.");

        var product = await _productRepository.GetByIdAsync(
            productId,
            cancellationToken)
            ?? throw new NotFoundException(
                $"Product {productId} was not found.");

        if (product.AllowedCustomerType != CustomerType.ALL &&
            product.AllowedCustomerType != customer.CustomerType)
        {
            throw new ValidationException(
                $"Product {product.ProductCode} does not allow " +
                $"{customer.CustomerType} customers.");
        }

        var accountAmount = product.ProductType == ProductType.SAVING
            ? data.OpeningBalance
            : data.Principal.GetValueOrDefault();

        if (accountAmount < product.MinimumAmount)
        {
            throw new ValidationException(
                $"The account amount must be at least " +
                $"{product.MinimumAmount}.");
        }

        foreach (var accountId in GetSettlementAccountIds(
                     data,
                     product.ProductType))
        {
            await ValidateSettlementAccountAsync(
                accountId,
                customerId,
                cancellationToken);
        }

        return product;
    }

    private static IEnumerable<Guid> GetSettlementAccountIds(
        AccountCreationData data,
        ProductType productType)
    {
        return productType switch
        {
            ProductType.SAVING => [],
            ProductType.TERM => RequiredIds(
                (data.FundingAccountId, nameof(data.FundingAccountId)),
                (data.MaturitySettlementAccountId,
                    nameof(data.MaturitySettlementAccountId))),
            ProductType.LOAN => RequiredIds(
                (data.DisbursementAccountId,
                    nameof(data.DisbursementAccountId)),
                (data.RepaymentAccountId,
                    nameof(data.RepaymentAccountId))),
            _ => throw new ValidationException(
                $"Unsupported product type {productType}.")
        };
    }

    private static IEnumerable<Guid> RequiredIds(
        params (Guid? AccountId, string Name)[] accounts)
    {
        foreach (var (accountId, name) in accounts)
        {
            if (accountId is null || accountId == Guid.Empty)
            {
                throw new ValidationException(
                    $"{name} is required.");
            }

            yield return accountId.Value;
        }
    }

    private async Task ValidateSettlementAccountAsync(
        Guid accountId,
        Guid customerId,
        CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetAccountByIdAsync(
            accountId,
            cancellationToken);

        if (account is not CustomerAccount customerAccount)
        {
            throw new NotFoundException(
                $"Settlement account {accountId} was not found.");
        }

        if (customerAccount.CustomerId != customerId)
        {
            throw new ValidationException(
                "Settlement accounts must belong to the same customer.");
        }

        if (customerAccount.ProductType != ProductType.SAVING)
        {
            throw new ValidationException(
                "Settlement accounts must be savings accounts.");
        }

        if (customerAccount.State != AccountState.ACTIVE)
        {
            throw new ValidationException(
                "Settlement accounts must be active.");
        }
    }
}
