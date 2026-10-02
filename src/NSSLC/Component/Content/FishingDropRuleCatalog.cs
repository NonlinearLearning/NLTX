using System.Collections.Frozen;
using System.Collections.Immutable;

namespace Terraria.Content;

public sealed class FishingDropRuleCatalog
{
  public FishingDropRuleCatalog(
    IEnumerable<FishingDropRuleDefinition> rules,
    bool isResolutionPathConfirmed = false)
  {
    ArgumentNullException.ThrowIfNull(rules);
    Rules = rules.ToImmutableArray();
    RulesByRarity = Rules
      .GroupBy(static rule => rule.Rarity)
      .ToFrozenDictionary(
        group => group.Key,
        group => group.ToImmutableArray());
    IsResolutionPathConfirmed = isResolutionPathConfirmed;
  }

  public ImmutableArray<FishingDropRuleDefinition> Rules { get; }

  public FrozenDictionary<FishingRarity, ImmutableArray<FishingDropRuleDefinition>> RulesByRarity { get; }

  public bool IsResolutionPathConfirmed { get; }

  public int Count => Rules.Length;
}
