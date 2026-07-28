using BankingConsole.Models.Enums;
using BankingConsole.Models.Interest;

namespace BankingConsole.Services.Interest.InterestDue;

public sealed class PostDatePolicy : IInterestPostPolicy
{
    public InterestPostPolicy Policy => InterestPostPolicy.POST_ON_DATE;

    public bool IsDue(IsDueResolverData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (data.PostDate is null)
        {
            throw new ArgumentException(
                "Post date is required by the post-date policy.",
                nameof(data));
        }

        return data.PostDate.Value.Date <= data.TodayDate.Date;
    }
}
