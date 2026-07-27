using BankingConsole.Models.Interest;

namespace BankingConsole.Services.Interest;

public class SimpleInterestCalculator : IInterestCalculator
{
    public decimal InterestCalculator(InterestCalculatorData data)
    {
        decimal interest = data.Balance * data.Rate / 365m;
        interest = Math.Round(interest,2, MidpointRounding.AwayFromZero);
        return interest;
    }
}