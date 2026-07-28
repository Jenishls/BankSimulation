namespace BankingConsole.Services.Interest.InterestTax;

public sealed class InterestTaxCalculator : IInterestTaxCalculator
{
    public decimal Calculate(
        decimal accruedInterest,
        decimal taxRate)
    {
        if (accruedInterest < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(accruedInterest));
        }

        if (taxRate is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(taxRate),
                "Tax rate must be between 0 and 100 percent.");
        }

        return Math.Round(
            accruedInterest * (taxRate / 100m),
            2,
            MidpointRounding.AwayFromZero);
    }
}
