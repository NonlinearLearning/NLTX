using System;

namespace Terraria.Share.Items;

/// <summary>
/// A value object for an item stack that does not require an ECS entity.
/// </summary>
public readonly record struct ItemStack {
  public ItemStack(ItemId itemId, int quantity, byte prefix = 0) {
    if (!ItemIdRules.IsKnown(itemId)) {
      throw new ArgumentException("An item stack requires a known item id.", nameof(itemId));
    }

    if (quantity <= 0) {
      throw new ArgumentOutOfRangeException(nameof(quantity));
    }

    ItemId = itemId;
    Quantity = quantity;
    Prefix = prefix;
  }

  public ItemId ItemId { get; }

  public int Quantity { get; }

  public byte Prefix { get; }

  public bool IsEmpty => !ItemIdRules.IsKnown(ItemId) || Quantity <= 0;

  public bool CanStackWith(in ItemStack other) {
    return !IsEmpty && !other.IsEmpty && ItemId == other.ItemId && Prefix == other.Prefix;
  }

  public ItemStack WithQuantity(int quantity) {
    if (quantity < 0) {
      throw new ArgumentOutOfRangeException(nameof(quantity));
    }

    return quantity == 0 ? default : new ItemStack(ItemId, quantity, Prefix);
  }
}
