using System;
using Terraria.Item;

namespace Terraria.Inventory;

/// <summary>Immutable rule describing what one inventory slot can hold.</summary>
public readonly record struct InventorySlotDefinition {
  public InventorySlotDefinition(
      int index,
      InventorySlotKind kind,
      ItemCategoryMask acceptedCategories,
      int maxStackSize = 1) {
    if (index < 0) {
      throw new ArgumentOutOfRangeException(nameof(index));
    }

    if (!Enum.IsDefined(kind)) {
      throw new ArgumentOutOfRangeException(nameof(kind));
    }

    if (acceptedCategories == ItemCategoryMask.None) {
      throw new ArgumentException(
          "A slot must accept at least one item category.", nameof(acceptedCategories));
    }

    if (maxStackSize <= 0) {
      throw new ArgumentOutOfRangeException(nameof(maxStackSize));
    }

    Index = index;
    Kind = kind;
    AcceptedCategories = acceptedCategories;
    MaxStackSize = maxStackSize;
  }

  public int Index { get; }

  public InventorySlotKind Kind { get; }

  public ItemCategoryMask AcceptedCategories { get; }

  public int MaxStackSize { get; }

  public bool IsValid => Index >= 0 && Enum.IsDefined(Kind) &&
      AcceptedCategories != ItemCategoryMask.None && MaxStackSize > 0;

  public bool Accepts(ItemDefinition definition) {
    return definition.Categories != ItemCategoryMask.None &&
        (AcceptedCategories & definition.Categories) != ItemCategoryMask.None;
  }
}
