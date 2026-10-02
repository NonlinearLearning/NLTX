using System.Collections.Immutable;
using NLTX.EconomyCraftingFishingLoot.Content;

namespace NLTX.EconomyCraftingFishingLoot.Loot;

public sealed class DropRateChainFeed
{
  public DropRateChainFeed(float parentDropRateChance)
    : this(parentDropRateChance, [])
  {
  }

  private DropRateChainFeed(
    float parentDropRateChance,
    ImmutableArray<DropConditionDefinition> conditions)
  {
    if (!float.IsFinite(parentDropRateChance) ||
      parentDropRateChance < 0 ||
      parentDropRateChance > 1)
    {
      throw new ArgumentOutOfRangeException(nameof(parentDropRateChance));
    }

    ParentDropRateChance = parentDropRateChance;
    Conditions = conditions;
  }

  public float ParentDropRateChance { get; }

  public ImmutableArray<DropConditionDefinition> Conditions { get; }

  public DropRateChainFeed With(float multiplier)
  {
    if (!float.IsFinite(multiplier) || multiplier < 0 || multiplier > 1)
    {
      throw new ArgumentOutOfRangeException(nameof(multiplier));
    }

    return new DropRateChainFeed(
      ParentDropRateChance * multiplier,
      Conditions);
  }

  public DropRateChainFeed AddCondition(DropConditionDefinition condition)
  {
    ArgumentNullException.ThrowIfNull(condition);
    return new DropRateChainFeed(
      ParentDropRateChance,
      Conditions.Add(condition));
  }
}
