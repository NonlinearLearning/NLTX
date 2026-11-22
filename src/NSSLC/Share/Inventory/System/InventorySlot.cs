using System;
using Terraria.Item;
using Terraria.Relationships;

namespace Terraria.Inventory;

/// <summary>A slot owns either a value stack or an item identity, never both.</summary>
public readonly record struct InventorySlot {
  internal InventorySlot(
      InventorySlotDefinition definition,
      ItemStack? content = null,
      EntityUuid itemEntityId = default) {
    if (content.HasValue && (content.Value.IsEmpty || itemEntityId.IsValid)) {
      throw new ArgumentException("A slot requires one nonempty stack or one item identity.");
    }

    Definition = definition;
    Content = content;
    ItemEntityId = itemEntityId;
  }

  public InventorySlotDefinition Definition { get; }

  public ItemStack? Content { get; }

  /// <summary>The referenced entity owns its stack and additional components.</summary>
  public EntityUuid ItemEntityId { get; }

  public bool ContainsEntity => ItemEntityId.IsValid;

  public bool IsEmpty => !Content.HasValue && !ContainsEntity;
}
