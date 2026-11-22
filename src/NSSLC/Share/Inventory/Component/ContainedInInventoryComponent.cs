using System;
using Terraria.Relationships;

namespace Terraria.Inventory;

/// <summary>Relationship state identifying the inventory slot that currently owns an item entity.</summary>
public readonly record struct ContainedInInventoryComponent {
  public ContainedInInventoryComponent(EntityUuid inventoryId, int slotIndex) {
    if (!inventoryId.IsValid) {
      throw new ArgumentException("An owning inventory identity is required.", nameof(inventoryId));
    }

    if (slotIndex < 0) {
      throw new ArgumentOutOfRangeException(nameof(slotIndex));
    }

    InventoryId = inventoryId;
    SlotIndex = slotIndex;
  }

  public EntityUuid InventoryId { get; }

  public int SlotIndex { get; }
}
