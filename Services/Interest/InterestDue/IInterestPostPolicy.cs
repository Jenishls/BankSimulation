using BankingConsole.Models.Enums;
using BankingConsole.Models.Interest;

namespace BankingConsole.Services.Interest.InterestDue;

public interface IInterestPostPolicy
{
    InterestPostPolicy Policy { get; }
    bool IsDue(IsDueResolverData data);
}
