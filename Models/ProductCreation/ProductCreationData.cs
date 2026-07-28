using BankingConsole.Models.Enums;

namespace BankingConsole.Models.ProductCreation;

public sealed class ProductCreationData
{
    public required string ProductCode { get; init; }
    public required string ProductName { get; init; }
    public required string BranchCode { get; init; }
    public Currency Currency { get; init; }
    public CustomerType AllowedCustomerType { get; init; }
    public decimal MinimumAmount { get; init; }
    public ProductType ProductType { get; init; }

    public decimal InterestRate { get; init; }
    public required Guid InterestOfficeAccountId { get; init; }
    public required Guid TaxOfficeAccountId { get; init; }
    public decimal TaxRate { get; init; }
    public bool PostInterestToLinkedAccount { get; init; }
    public List<InterestPostPolicy> InterestPostPolicies { get; init; } = [];
    public DateTime? PostDate { get; init; }
    public Frequency? InterestPostingFrequency { get; init; }

    public int? TenureInDays { get; init; }
    public int? TransferCount { get; init; }
    public Flow? TransferFlow { get; init; }
    public decimal? TransferPenaltyRate { get; init; }
    public bool? AllowPremature { get; init; }
}
