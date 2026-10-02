using System.Collections.Immutable;

namespace NLTX.EconomyCraftingFishingLoot.Commerce;

public sealed class ShopInventorySlotsComponent
{
  public ShopInventorySlotsComponent(
    IEnumerable<int> slotContents,
    int slotCapacity = 100)
  {
    ArgumentNullException.ThrowIfNull(slotContents);
    if (slotCapacity <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(slotCapacity));
    }

    ImmutableArray<int> snapshot = slotContents.ToImmutableArray();
    if (snapshot.Length > slotCapacity)
    {
      throw new ArgumentException(
        "Shop contents cannot exceed the configured slot capacity.",
        nameof(slotContents));
    }

    if (snapshot.Any(itemTypeId => itemTypeId < 0))
    {
      throw new ArgumentException(
        "Shop item type IDs cannot be negative.",
        nameof(slotContents));
    }

    SlotContents = snapshot;
    SlotCapacity = slotCapacity;
  }

  public ImmutableArray<int> SlotContents { get; }

  public int SlotCapacity { get; }

  public int OccupiedSlotCount => SlotContents.Count(itemTypeId => itemTypeId != 0);

  public bool TryGetItemTypeId(int slotIndex, out int itemTypeId)
  {
    if ((uint)slotIndex >= (uint)SlotContents.Length)
    {
      itemTypeId = 0;
      return false;
    }

    itemTypeId = SlotContents[slotIndex];
    return itemTypeId != 0;
  }
}
