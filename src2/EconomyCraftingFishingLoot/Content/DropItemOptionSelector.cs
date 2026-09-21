using System.Collections.Immutable;
using NLTX.EconomyCraftingFishingLoot.Loot;

namespace NLTX.EconomyCraftingFishingLoot.Content;

public sealed class DropItemOptionSelector
{
  public DropItemOptionSelector(
    IEnumerable<int> itemIds,
    int chanceDenominator,
    int chanceNumerator)
  {
    ArgumentNullException.ThrowIfNull(itemIds);
    if (chanceDenominator <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(chanceDenominator));
    }

    if (chanceNumerator < 0 || chanceNumerator > chanceDenominator)
    {
      throw new ArgumentOutOfRangeException(nameof(chanceNumerator));
    }

    ItemIds = itemIds.ToImmutableArray();
    if (ItemIds.Any(itemId => itemId < 0))
    {
      throw new ArgumentException(
        "Drop option item IDs cannot be negative.",
        nameof(itemIds));
    }

    ChanceDenominator = chanceDenominator;
    ChanceNumerator = chanceNumerator;
  }

  public ImmutableArray<int> ItemIds { get; }

  public int ChanceDenominator { get; }

  public int ChanceNumerator { get; }

  public float PersonalDropRate => (float)ChanceNumerator / ChanceDenominator;

  public bool TrySelect(DropResolutionContext context, out int itemId)
  {
    ArgumentNullException.ThrowIfNull(context);
    itemId = default;
    if (ItemIds.Length == 0)
    {
      return false;
    }

    if (context.Random.Next(ChanceDenominator) >= ChanceNumerator)
    {
      return false;
    }

    int selectedIndex = context.Random.Next(ItemIds.Length);
    if ((uint)selectedIndex >= (uint)ItemIds.Length)
    {
      throw new InvalidOperationException(
        "The drop random source returned an index outside the option range.");
    }

    itemId = ItemIds[selectedIndex];
    return true;
  }
}
