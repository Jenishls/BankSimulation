using BankingConsole.Models.Enums;

namespace BankingConsole.Models.Product;

public sealed class Product
{
    public Guid ProductId { get; private set; }
    public string ProductCode { get; private set; } = null!;
    public string ProductName { get; private set; } = null!;
    public string BranchCode { get; private set; } = null!;
    public Currency Currency { get; private set; }
    public CustomerType AllowedCustomerType { get; private set; }
    public decimal MinimumAmount { get; private set; }
    public ProductType ProductType { get; private set; }

    public decimal InterestRate { get; private set; }
    public Flow InterestFlow { get; private set; }
    public Guid InterestOfficeAccountId { get; private set; }
    public Guid TaxOfficeAccountId { get; private set; }
    public decimal TaxRate { get; private set; }
    public bool PostInterestToLinkedAccount { get; private set; }
    public List<InterestPostPolicy> InterestPostPolicies { get; private set; } = [];
    public DateTime? PostDate { get; private set; }
    public Frequency? InterestPostingFrequency { get; private set; }

    public bool IsMaturityProduct { get; private set; }
    public int? TenureInDays { get; private set; }
    public int? TransferCount { get; private set; }
    public Flow? TransferFlow { get; private set; }
    public decimal? TransferPenaltyRate { get; private set; }
    public bool? AllowPremature { get; private set; }

    private Product()
    {
    }

    private Product(
        string productCode,
        string productName,
        string branchCode,
        Currency currency,
        CustomerType allowedCustomerType,
        decimal minimumAmount,
        decimal interestRate,
        Flow interestFlow,
        Guid interestOfficeAccountId,
        Guid taxOfficeAccountId,
        decimal taxRate,
        bool postInterestToLinkedAccount,
        List<InterestPostPolicy> interestPostPolicies,
        DateTime? postDate,
        Frequency? interestPostingFrequency,
        bool isMaturityProduct,
        ProductType productType,
        int? tenureInDays,
        int? transferCount,
        Flow? transferFlow,
        decimal? transferPenaltyRate,
        bool? allowPremature)
    {
        ProductId = Guid.NewGuid();
        ProductCode = productCode;
        ProductName = productName;
        BranchCode = branchCode;
        Currency = currency;
        AllowedCustomerType = allowedCustomerType;
        MinimumAmount = minimumAmount;
        ProductType = productType;
        InterestRate = interestRate;
        InterestFlow = interestFlow;
        InterestOfficeAccountId = interestOfficeAccountId;
        TaxOfficeAccountId = taxOfficeAccountId;
        TaxRate = taxRate;
        PostInterestToLinkedAccount = postInterestToLinkedAccount;
        InterestPostPolicies = interestPostPolicies;
        PostDate = postDate;
        InterestPostingFrequency = interestPostingFrequency;
        IsMaturityProduct = isMaturityProduct;
        TenureInDays = tenureInDays;
        TransferCount = transferCount;
        TransferFlow = transferFlow;
        TransferPenaltyRate = transferPenaltyRate;
        AllowPremature = allowPremature;
    }

    public static Product Create(
        string productCode,
        string productName,
        string branchCode,
        Currency currency,
        CustomerType allowedCustomerType,
        decimal interestRate,
        IEnumerable<InterestPostPolicy> interestPostPolicies,
        Guid interestOfficeAccountId,
        Guid taxOfficeAccountId,
        ProductType productType,
        decimal taxRate = 0,
        bool postInterestToLinkedAccount = false,
        decimal minimumAmount = 0,
        DateTime? postDate = null,
        Frequency? interestPostingFrequency = null,
        int? tenureInDays = null,
        int? transferCount = null,
        Flow? transferFlow = null,
        decimal? transferPenaltyRate = null,
        bool? allowPremature = null)
    {
        if (string.IsNullOrWhiteSpace(productCode))
        {
            throw new ArgumentException(
                "Product code is required.",
                nameof(productCode));
        }

        if (string.IsNullOrWhiteSpace(productName))
        {
            throw new ArgumentException(
                "Product name is required.",
                nameof(productName));
        }

        if (string.IsNullOrWhiteSpace(branchCode))
        {
            throw new ArgumentException(
                "Branch code is required.",
                nameof(branchCode));
        }

        if (minimumAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(minimumAmount));

        if (interestRate < 0)
            throw new ArgumentOutOfRangeException(nameof(interestRate));

        if (interestOfficeAccountId == Guid.Empty)
        {
            throw new ArgumentException(
                "An interest office account is required.",
                nameof(interestOfficeAccountId));
        }

        if (taxOfficeAccountId == Guid.Empty)
        {
            throw new ArgumentException(
                "A tax office account is required.",
                nameof(taxOfficeAccountId));
        }

        if (taxRate is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(taxRate),
                "Tax rate must be between 0 and 100 percent.");
        }

        if (productType == ProductType.LOAN && taxRate > 0)
        {
            throw new ArgumentException(
                "Withholding tax cannot be configured for debit-interest products.",
                nameof(taxRate));
        }

        ArgumentNullException.ThrowIfNull(interestPostPolicies);

        var postingPolicies = interestPostPolicies
            .Distinct()
            .ToList();

        if (interestRate > 0 && postingPolicies.Count == 0)
        {
            throw new ArgumentException(
                "At least one posting policy is required when interest is configured.",
                nameof(interestPostPolicies));
        }

        var postsByFrequency = postingPolicies.Contains(
            InterestPostPolicy.LAST_DAY_OFF_FREQUENCY);

        if (postsByFrequency != interestPostingFrequency.HasValue)
        {
            throw new ArgumentException(
                "Posting frequency must be supplied only for the last-day-of-frequency policy.",
                nameof(interestPostingFrequency));
        }

        var postsOnDate = postingPolicies.Contains(
            InterestPostPolicy.POST_ON_DATE);

        if (postsOnDate != postDate.HasValue)
        {
            throw new ArgumentException(
                "Post date must be supplied only for the post-on-date policy.",
                nameof(postDate));
        }

        var isMaturityProduct =
            productType is ProductType.TERM or ProductType.LOAN;

        if (postInterestToLinkedAccount && !isMaturityProduct)
        {
            throw new ArgumentException(
                "Only term and loan products can post interest to a linked account.",
                nameof(postInterestToLinkedAccount));
        }

        if (postingPolicies.Contains(InterestPostPolicy.ON_MATURITY) &&
            !isMaturityProduct)
        {
            throw new ArgumentException(
                "The on-maturity policy can only be used by term or loan products.",
                nameof(interestPostPolicies));
        }

        if (isMaturityProduct)
        {
            if (tenureInDays is null or <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(tenureInDays),
                    "Term and loan products require a positive tenure.");
            }
        }
        else if (tenureInDays.HasValue)
        {
            throw new ArgumentException(
                "A savings product cannot have a tenure.",
                nameof(tenureInDays));
        }

        if (transferCount is < 0)
            throw new ArgumentOutOfRangeException(nameof(transferCount));

        if (transferCount.HasValue != transferFlow.HasValue)
        {
            throw new ArgumentException(
                "Transfer count and transfer flow must be supplied together.");
        }

        if (transferPenaltyRate < 0)
            throw new ArgumentOutOfRangeException(nameof(transferPenaltyRate));

        if (!isMaturityProduct &&
            (transferCount.HasValue ||
             transferFlow.HasValue ||
             transferPenaltyRate.HasValue ||
             allowPremature.HasValue))
        {
            throw new ArgumentException(
                "Transfer and premature-maturity settings are only valid for term or loan products.");
        }

        var interestFlow = productType == ProductType.LOAN
            ? Flow.DEBIT
            : Flow.CREDIT;

        return new Product(
            productCode.Trim().ToUpperInvariant(),
            productName.Trim(),
            branchCode.Trim().ToUpperInvariant(),
            currency,
            allowedCustomerType,
            minimumAmount,
            interestRate,
            interestFlow,
            interestOfficeAccountId,
            taxOfficeAccountId,
            taxRate,
            postInterestToLinkedAccount,
            postingPolicies,
            postDate,
            interestPostingFrequency,
            isMaturityProduct,
            productType,
            tenureInDays,
            transferCount,
            transferFlow,
            transferPenaltyRate,
            isMaturityProduct ? allowPremature ?? false : null);
    }

    public void ApplyUpdate(Product updatedProduct)
    {
        ArgumentNullException.ThrowIfNull(updatedProduct);

        ProductCode = updatedProduct.ProductCode;
        ProductName = updatedProduct.ProductName;
        BranchCode = updatedProduct.BranchCode;
        Currency = updatedProduct.Currency;
        AllowedCustomerType = updatedProduct.AllowedCustomerType;
        MinimumAmount = updatedProduct.MinimumAmount;
        ProductType = updatedProduct.ProductType;
        InterestRate = updatedProduct.InterestRate;
        InterestFlow = updatedProduct.InterestFlow;
        InterestOfficeAccountId = updatedProduct.InterestOfficeAccountId;
        TaxOfficeAccountId = updatedProduct.TaxOfficeAccountId;
        TaxRate = updatedProduct.TaxRate;
        PostInterestToLinkedAccount =
            updatedProduct.PostInterestToLinkedAccount;
        InterestPostPolicies = updatedProduct.InterestPostPolicies.ToList();
        PostDate = updatedProduct.PostDate;
        InterestPostingFrequency = updatedProduct.InterestPostingFrequency;
        IsMaturityProduct = updatedProduct.IsMaturityProduct;
        TenureInDays = updatedProduct.TenureInDays;
        TransferCount = updatedProduct.TransferCount;
        TransferFlow = updatedProduct.TransferFlow;
        TransferPenaltyRate = updatedProduct.TransferPenaltyRate;
        AllowPremature = updatedProduct.AllowPremature;
    }
}
