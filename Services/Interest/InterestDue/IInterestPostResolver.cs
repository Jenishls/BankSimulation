using BankingConsole.Models.Enums;

namespace BankingConsole.Services.Interest.InterestDue;

public interface IInterestPostResolver
{
    IReadOnlyList<IInterestPostPolicy> Resolve(
        IEnumerable<InterestPostPolicy> policies);
}
