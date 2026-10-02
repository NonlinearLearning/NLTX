using System.Collections.Immutable;
using NLTX.EconomyCraftingFishingLoot.Fishing;

namespace NLTX.EconomyCraftingFishingLoot.Content;

public sealed class FishingDropRuleDefinition
{
  public FishingDropRuleDefinition(
    IEnumerable<int> possibleItems,
    int chanceNumerator,
    int chanceDenominator,
    IEnumerable<IFishingConditionDefinition> conditions,
    FishingRarityConditionDefinition rarity)
  {
    ArgumentNullException.ThrowIfNull(possibleItems);
    ArgumentNullException.ThrowIfNull(conditions);
    ArgumentNullException.ThrowIfNull(rarity);
    if (chanceNumerator < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(chanceNumerator));
    }

    if (chanceDenominator <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(chanceDenominator));
    }

    ImmutableArray<int> itemSnapshot = possibleItems.ToImmutableArray();
    if (itemSnapshot.Any(itemType => itemType < 0))
    {
      throw new ArgumentException(
        "Possible item types cannot be negative.",
        nameof(possibleItems));
    }

    ImmutableArray<IFishingConditionDefinition> conditionSnapshot =
      conditions.ToImmutableArray();
    if (conditionSnapshot.Any(condition => condition is null))
    {
      throw new ArgumentException(
        "Fishing conditions cannot contain null entries.",
        nameof(conditions));
    }

    PossibleItems = itemSnapshot;
    ChanceNumerator = chanceNumerator;
    ChanceDenominator = chanceDenominator;
    Conditions = conditionSnapshot;
    Rarity = rarity;
  }

  public ImmutableArray<int> PossibleItems { get; }

  public int ChanceNumerator { get; }

  public int ChanceDenominator { get; }

  public ImmutableArray<IFishingConditionDefinition> Conditions { get; }

  public FishingRarityConditionDefinition Rarity { get; }

  public bool ConditionsMatch(FishingConditionEvaluationContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    return Conditions.All(condition => condition.Matches(context));
  }
}
