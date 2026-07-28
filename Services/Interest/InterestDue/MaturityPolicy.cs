using BankingConsole.Models.Enums;
using BankingConsole.Models.Interest;

namespace BankingConsole.Services.Interest.InterestDue;

public sealed class MaturityPolicy : IInterestPostPolicy
{
    public InterestPostPolicy Policy => InterestPostPolicy.Maturity;

    public bool IsDue(IsDueResolverData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (data.MaturityDate is null)
        {
            throw new ArgumentException(
                "Maturity date is required by the maturity policy.",
                nameof(data));
        }

        return data.MaturityDate.Value.Date <= data.TodayDate.Date;
    }
}
