using BankingConsole.Models.Enums;
using BankingConsole.Models.Interest;

namespace BankingConsole.Services.Interest.InterestDue;

public sealed class LastDayOfFrequencyPolicy : IInterestPostPolicy
{
    public InterestPostPolicy Policy => InterestPostPolicy.LAST_DAY_OFF_FREQUENCY;

    public bool IsDue(IsDueResolverData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (data.Frequency is null)
        {
            throw new ArgumentException(
                "Frequency is required by the last-day-of-frequency policy.",
                nameof(data));
        }

        var tomorrow = data.TodayDate.Date.AddDays(1);

        return data.Frequency.Value switch
        {
            Frequency.DAILY => true,
            Frequency.WEEKLY => tomorrow.DayOfWeek is DayOfWeek.Monday,
            Frequency.MONTHLY => tomorrow.Day is 1,
            Frequency.QUATERLY =>
                tomorrow.Day is 1 &&
                tomorrow.Month is 1 or 4 or 7 or 10,
            Frequency.HALF_YEARLY =>
                tomorrow.Day is 1 &&
                tomorrow.Month is 1 or 7,
            Frequency.YEARLY =>
                tomorrow.Day is 1 &&
                tomorrow.Month is 1,
            _ => throw new ArgumentOutOfRangeException(
                nameof(data.Frequency),
                data.Frequency,
                "Unhandled posting frequency.")
        };
    }
}
