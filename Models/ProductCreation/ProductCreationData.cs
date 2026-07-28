using BankingConsole.Models.Enums;

namespace BankingConsole.Models.ProductCreation;

public sealed class ProductCreationData
{
    public required string ProductCode { get; init; }
    public required string ProductName { get; init; }
    public Currency Currency { get; init; }
    public CustomerType AllowedCustomerType { get; init; }
    public decimal MinimumAmount { get; init; }
    public ProductType ProductType { get; init; }

    public Flow? OfficeInterestFlow { get; init; }
    public decimal InterestRate { get; init; }
    public List<InterestPostPolicy> InterestPostPolicies { get; init; } = [];
    public Frequency? InterestPostingFrequency { get; init; }
    public decimal? TaxPercentage { get; init; }

    public int WithdrawalLimitCount { get;  init; }
    public Frequency? WithdrawalLimitFrequency { get;  init; }
    public decimal WithdrawalLimitAmount { get;  init; }
    public Frequency? WithdrawalLimitAmountFrequency { get;  init; }

    public int? TenureInDays { get; init; }

    public int? TransferCount { get; init; }
    public Frequency? TransferFrequency { get; init; }
    public bool AllowPrematureWithdrawal { get; init; }

    public int? RepaymentCount { get; init; }
    public Frequency? RepaymentFrequency { get; init; }
    public decimal? PenaltyInterestRate { get; init; }
}
