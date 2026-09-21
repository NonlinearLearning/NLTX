using System.Collections.Immutable;
using NLTX.EconomyCraftingFishingLoot.Content;

namespace NLTX.EconomyCraftingFishingLoot.Loot;

public sealed class DropRateProjection
{
  public DropRateProjection(
    int itemId,
    int stackMinimum,
    int stackMaximum,
    float dropRate,
    IEnumerable<DropConditionDefinition> conditions)
  {
    if (itemId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(itemId));
    }

    if (stackMinimum < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(stackMinimum));
    }

    if (stackMaximum < stackMinimum)
    {
      throw new ArgumentOutOfRangeException(nameof(stackMaximum));
    }

    if (!float.IsFinite(dropRate) || dropRate < 0 || dropRate > 1)
    {
      throw new ArgumentOutOfRangeException(nameof(dropRate));
    }

    ArgumentNullException.ThrowIfNull(conditions);
    ImmutableArray<DropConditionDefinition> conditionSnapshot =
      conditions.ToImmutableArray();
    if (conditionSnapshot.Any(condition => condition is null))
    {
      throw new ArgumentException(
        "Drop-rate conditions cannot contain null entries.",
        nameof(conditions));
    }

    ItemId = itemId;
    StackMinimum = stackMinimum;
    StackMaximum = stackMaximum;
    DropRate = dropRate;
    Conditions = conditionSnapshot;
  }

  public int ItemId { get; }

  public int StackMinimum { get; }

  public int StackMaximum { get; }

  public float DropRate { get; }

  public ImmutableArray<DropConditionDefinition> Conditions { get; }
}
