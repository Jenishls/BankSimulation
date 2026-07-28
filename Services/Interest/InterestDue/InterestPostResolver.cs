using BankingConsole.Models.Enums;

namespace BankingConsole.Services.Interest.InterestDue;

public sealed class InterestPostResolver : IInterestPostResolver
{
    private readonly IReadOnlyDictionary<
        InterestPostPolicy,
        IInterestPostPolicy> _policies;

    public InterestPostResolver(
        IEnumerable<IInterestPostPolicy> policies)
    {
        ArgumentNullException.ThrowIfNull(policies);

        _policies = policies.ToDictionary(
            policy => policy.Policy);
    }

    public IReadOnlyList<IInterestPostPolicy> Resolve(
        IEnumerable<InterestPostPolicy> policies)
    {
        ArgumentNullException.ThrowIfNull(policies);

        return policies
            .Distinct()
            .Select(policy =>
                _policies.TryGetValue(policy, out var implementation)
                    ? implementation
                    : throw new ArgumentOutOfRangeException(
                        nameof(policies),
                        policy,
                        "No implementation is registered for this interest-post policy."))
            .ToList();
    }
}
