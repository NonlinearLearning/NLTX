using Terraria.Relationships;

namespace Terraria.Items;

public enum ItemInventorySlotKind : byte
{
  MainInventory,
  Trash,
}

public readonly record struct ItemInventoryRelationComponent
{
  public ItemInventoryRelationComponent(
    EntityReference playerReference,
    ItemInventorySlotKind slotKind,
    int slotIndex)
  {
    if (playerReference.IsEmpty || playerReference.Scope != EntityReferenceScope.Player)
    {
      throw new ArgumentException(
        "An item inventory relation requires a Player-scoped reference.",
        nameof(playerReference));
    }

    if (slotKind == ItemInventorySlotKind.MainInventory)
    {
      ArgumentOutOfRangeException.ThrowIfNegative(slotIndex);
    }
    else if (slotKind == ItemInventorySlotKind.Trash && slotIndex != 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(slotIndex),
        "The Trash relation uses slot index zero.");
    }
    else if (!Enum.IsDefined(slotKind))
    {
      throw new ArgumentOutOfRangeException(nameof(slotKind));
    }

    PlayerReference = playerReference;
    SlotKind = slotKind;
    SlotIndex = slotIndex;
  }

  public EntityReference PlayerReference { get; }

  public ItemInventorySlotKind SlotKind { get; }

  public int SlotIndex { get; }
}
