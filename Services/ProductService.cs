using BankingConsole.Common;
using BankingConsole.DB;
using BankingConsole.Factories;
using BankingConsole.Models.Account;
using BankingConsole.Models.Enums;
using BankingConsole.Models.Product;
using BankingConsole.Models.ProductCreation;
using BankingConsole.Repository;

namespace BankingConsole.Services;

public sealed class ProductService
{
    private readonly IProductFactory _productFactory;
    private readonly IProductRepository _productRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductFactory productFactory,
        IProductRepository productRepository,
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        ILogger<ProductService> logger)
    {
        _productFactory = productFactory;
        _productRepository = productRepository;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Product> CreateProductAsync(
        ProductCreationData data,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);

        var product = _productFactory.Create(data);

        var expectedInterestType = product.ProductType == ProductType.LOAN
            ? OfficeAccountType.INCOME
            : OfficeAccountType.EXPENSE;

        await ValidateOfficeAccountAsync(
            product.InterestOfficeAccountId,
            expectedInterestType,
            product.BranchCode,
            "interest",
            cancellationToken);

        await ValidateOfficeAccountAsync(
            product.TaxOfficeAccountId,
            OfficeAccountType.TAX_PAYABLE,
            product.BranchCode,
            "tax",
            cancellationToken);

        _productRepository.Add(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created {ProductType} product {ProductId}",
            product.ProductType,
            product.ProductId);

        return product;
    }

    private async Task ValidateOfficeAccountAsync(
        Guid accountId,
        OfficeAccountType expectedType,
        string branchCode,
        string purpose,
        CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetAccountByIdAsync(
            accountId,
            cancellationToken);

        if (account is not OfficeAccount officeAccount)
        {
            throw new NotFoundException(
                $"The {purpose} office account {accountId} was not found.");
        }

        if (officeAccount.OfficeAccountType != expectedType)
        {
            throw new ValidationException(
                $"The {purpose} office account must be of type {expectedType}.");
        }

        if (!string.Equals(
                officeAccount.BranchCode,
                branchCode.Trim(),
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ValidationException(
                $"The {purpose} office account must belong to product branch {branchCode}.");
        }
    }
}
