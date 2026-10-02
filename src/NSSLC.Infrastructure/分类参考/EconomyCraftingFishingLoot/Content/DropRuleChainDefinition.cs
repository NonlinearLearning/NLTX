using System.Collections.Immutable;
using NLTX.EconomyCraftingFishingLoot.Loot;

namespace NLTX.EconomyCraftingFishingLoot.Content;

public sealed class DropRuleChainDefinition
{
  private readonly ImmutableArray<DropChainAttemptContract> _attempts;

  public DropRuleChainDefinition(IEnumerable<DropChainAttemptContract> attempts)
  {
    ArgumentNullException.ThrowIfNull(attempts);
    ImmutableArray<DropChainAttemptContract> snapshot = attempts.ToImmutableArray();
    if (snapshot.Any(attempt => attempt is null))
    {
      throw new ArgumentException(
        "Drop-chain attempts cannot contain null entries.",
        nameof(attempts));
    }

    _attempts = snapshot;
  }

  public ImmutableArray<DropChainAttemptContract> Attempts => _attempts;

  public ImmutableArray<DropChainAttemptContract> GetTriggeredChains(
    DropAttemptResult result)
  {
    ArgumentNullException.ThrowIfNull(result);
    return _attempts.Where(attempt => attempt.Matches(result)).ToImmutableArray();
  }
}
