using BankingConsole.Models.Enums;
namespace BankingConsole.Models.Interest;
public sealed class IsDueResolverData
{
    public Frequency? Frequency{get; init; } = null;
    public DateTime? MaturityDate{get; init; } = null;
    public DateTime? PostDate{get; init;} = null;
    public required DateTime TodayDate{get; init;}
    public required DateTime LastInterestPostedOn {get; init;}
}