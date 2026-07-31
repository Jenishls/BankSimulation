using BankingConsole.Common;
using BankingConsole.DB;
using BankingConsole.Factories;
using BankingConsole.Models.Account;
using BankingConsole.Models.Enums;
using BankingConsole.Models.Product;
using BankingConsole.Models.ProductCreation;
using BankingConsole.Repository;
using Microsoft.EntityFrameworkCore;

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

        await ValidatePostingAccountsAsync(product, cancellationToken);

        _productRepository.Add(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created {ProductType} product {ProductId}",
            product.ProductType,
            product.ProductId);

        return product;
    }

    public Task<IReadOnlyList<Product>> GetProductsAsync(
        CancellationToken cancellationToken = default)
    {
        return _productRepository.GetAllAsync(cancellationToken);
    }

    public async Task<Product> GetProductByIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        return await _productRepository.GetByIdAsync(
            productId,
            cancellationToken)
            ?? throw new NotFoundException(
                $"Product {productId} was not found.");
    }

    public async Task<Product> UpdateProductAsync(
        Guid productId,
        ProductCreationData data,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);

        var product = await _productRepository.GetByIdAsync(
            productId,
            cancellationToken)
            ?? throw new NotFoundException(
                $"Product {productId} was not found.");

        var updatedProduct = _productFactory.Create(data);
        await ValidatePostingAccountsAsync(
            updatedProduct,
            cancellationToken);

        if (product.ProductType != updatedProduct.ProductType)
        {
            var accounts = await _accountRepository
                .GetAccountsByProductIdAsync(
                    productId,
                    cancellationToken);

            if (accounts.Count > 0)
            {
                throw new ConflictException(
                    "A product with accounts cannot change its product type.");
            }
        }

        product.ApplyUpdate(updatedProduct);
        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Updated {ProductType} product {ProductId}",
            product.ProductType,
            product.ProductId);

        return product;
    }

    public async Task DeleteProductAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(
            productId,
            cancellationToken)
            ?? throw new NotFoundException(
                $"Product {productId} was not found.");

        var accounts = await _accountRepository.GetAccountsByProductIdAsync(
            productId,
            cancellationToken);

        if (accounts.Count > 0)
        {
            throw new ConflictException(
                "A product with accounts cannot be deleted.");
        }

        _productRepository.Delete(product);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new ConflictException(
                "The product cannot be deleted because it is still in use.");
        }

        _logger.LogInformation(
            "Deleted {ProductType} product {ProductId}",
            product.ProductType,
            product.ProductId);
    }

    private async Task ValidatePostingAccountsAsync(
        Product product,
        CancellationToken cancellationToken)
    {
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
