using System.Collections.Immutable;

namespace NLTX.EconomyCraftingFishingLoot.Content;

public sealed class FishingDropRuleCatalog
{
  private readonly ImmutableArray<FishingDropRuleDefinition> _rules;

  public FishingDropRuleCatalog(IEnumerable<FishingDropRuleDefinition> rules)
  {
    ArgumentNullException.ThrowIfNull(rules);

    List<FishingDropRuleDefinition> snapshot = rules.ToList();
    if (snapshot.Any(rule => rule is null))
    {
      throw new ArgumentException(
        "Fishing drop rules cannot contain null entries.",
        nameof(rules));
    }

    _rules = snapshot.ToImmutableArray();
  }

  public int Count => _rules.Length;

  public ImmutableArray<FishingDropRuleDefinition> Rules => _rules;
}
