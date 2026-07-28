using BankingConsole.Models.Interest;

namespace BankingConsole.Services.Interest.InterestCalculation;

public class SimpleInterestCalculator : IInterestCalculator
{
    public decimal Calculate(InterestCalculatorData data)
    {
        decimal interest =
            data.Balance * (data.Rate / 100m) / 365m;
        interest = Math.Round(interest,2, MidpointRounding.AwayFromZero);
        return interest;
    }
}
