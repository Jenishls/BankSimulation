namespace BankingConsole.Services.Interest.InterestTax;

public interface IInterestTaxCalculator
{
    decimal Calculate(decimal accruedInterest, decimal taxRate);
}
