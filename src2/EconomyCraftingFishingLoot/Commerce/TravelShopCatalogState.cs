using System.Collections.Immutable;

namespace NLTX.EconomyCraftingFishingLoot.Commerce;

public sealed class TravelShopCatalogState
{
  public const int SourceMaxSlots = 40;

  public TravelShopCatalogState(IEnumerable<int> itemTypeIds)
  {
    ArgumentNullException.ThrowIfNull(itemTypeIds);
    ImmutableArray<int> snapshot = itemTypeIds.ToImmutableArray();
    if (snapshot.Length > SourceMaxSlots)
    {
      throw new ArgumentException(
        "Travel-shop contents cannot exceed the source slot limit.",
        nameof(itemTypeIds));
    }

    if (snapshot.Any(itemTypeId => itemTypeId < 0))
    {
      throw new ArgumentException(
        "Travel-shop item type IDs cannot be negative.",
        nameof(itemTypeIds));
    }

    ItemTypeIds = snapshot;
  }

  public int MaxSlots => SourceMaxSlots;

  public ImmutableArray<int> ItemTypeIds { get; }

  public bool TryGetItemTypeId(int slotIndex, out int itemTypeId)
  {
    if ((uint)slotIndex >= (uint)ItemTypeIds.Length)
    {
      itemTypeId = 0;
      return false;
    }

    itemTypeId = ItemTypeIds[slotIndex];
    return itemTypeId != 0;
  }
}
