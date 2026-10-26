using System;

namespace Terraria.Item;

/// <summary>
/// A value object for an item stack that does not require an ECS entity.
/// </summary>
public readonly record struct ItemStack {
  public ItemStack(ItemDefinitionId definitionId, int quantity, byte prefix = 0) {
    if (!definitionId.IsValid) {
      throw new ArgumentException("An item stack requires a valid definition id.", nameof(definitionId));
    }

    if (quantity <= 0) {
      throw new ArgumentOutOfRangeException(nameof(quantity));
    }

    DefinitionId = definitionId;
    Quantity = quantity;
    Prefix = prefix;
  }

  public ItemDefinitionId DefinitionId { get; }

  public int Quantity { get; }

  public byte Prefix { get; }

  public bool IsEmpty => !DefinitionId.IsValid || Quantity <= 0;

  public bool CanStackWith(in ItemStack other) {
    return !IsEmpty && !other.IsEmpty && DefinitionId == other.DefinitionId &&
        Prefix == other.Prefix;
  }

  public ItemStack WithQuantity(int quantity) {
    if (quantity < 0) {
      throw new ArgumentOutOfRangeException(nameof(quantity));
    }

    return quantity == 0 ? default : new ItemStack(DefinitionId, quantity, Prefix);
  }
}
