using BankingConsole.Models.Enums;

namespace BankingConsole.Models.ProductCreation;

public class ProductCreationData
{
    public required string ProductCode {get; init;}
    public required string ProductName {get; init;}
    public Currency Currency {get; init;}
    public CustomerType AllowedCustomerType {get; init;}
    public decimal MinimunAmount {get; init;}
    public ProductType ProductType {get; init;}


    public bool DebitInterestCalculation { get;  init; }
    public bool CreditInterestCalculation { get;  init; }
    public decimal? DebitInterestRate { get;  init; }
    public decimal? CreditInterestRate { get;  init; }
    public Frequency? DebitCalculationFrequency {get;  init;}
    public Frequency? CreditCalculationFrequency {get;  init;}
    public Frequency? DebitPostingFrequency {get;  init;}
    public Frequency? CreditPostingFrequency {get;  init;}
    public decimal? TaxPercentage { get;  init; }

    public int WithdrawalLimitCount { get;  init; }
    public Frequency? WithdrawalLimitFrequency { get;  init; }
    public decimal WithdrawalLimitAmount { get;  init; }
    public Frequency? WithdrawalLimitAmountFrequency { get;  init; }

    public int? TenureInDays { get; init; }

    public int? TransferCount { get; init; }
    public Frequency? TransferFrequency { get; init; }

    public int? RepaymentCount { get; init; }
    public Frequency? RepaymentFrequency { get; init; }
    public decimal? PenaltyInterestRate { get; init; }
}
