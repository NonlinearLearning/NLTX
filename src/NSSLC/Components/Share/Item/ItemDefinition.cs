using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Terraria.Attributes;

namespace Terraria.Item;

/// <summary>
/// Immutable rules and defaults shared by all stacks of an item kind.
/// </summary>
public sealed class ItemDefinition {
  public ItemDefinition(
      ItemId id,
      int maxStack,
      ItemCategoryMask categories = ItemCategoryMask.General,
      IEnumerable<ItemAttributeModifierDefinition>? attributeModifiers = null) {
    if (!ItemIdRules.IsKnown(id)) {
      throw new ArgumentException("An item definition requires a known item id.", nameof(id));
    }

    if (maxStack <= 0) {
      throw new ArgumentOutOfRangeException(nameof(maxStack));
    }

    Id = id;
    MaxStack = maxStack;
    Categories = categories;

    ItemAttributeModifierDefinition[] modifiers =
        (attributeModifiers ?? Array.Empty<ItemAttributeModifierDefinition>()).ToArray();
    foreach (ItemAttributeModifierDefinition modifier in modifiers) {
      if (!modifier.IsInventorySource) {
        throw new ArgumentException(
            "Item attribute contributions must use an inventory source category.",
            nameof(attributeModifiers));
      }
    }

    AttributeModifiers = new ReadOnlyCollection<ItemAttributeModifierDefinition>(modifiers);
  }

  public ItemId Id { get; }

  public int MaxStack { get; }

  public ItemCategoryMask Categories { get; }

  /// <summary>
  /// Numeric contributions rebuilt from an inventory relation by the attribute contribution system.
  /// </summary>
  public IReadOnlyList<ItemAttributeModifierDefinition> AttributeModifiers { get; }

  public bool IsStackable => MaxStack > 1;

  public bool AcceptsQuantity(int quantity) {
    return quantity > 0 && quantity <= MaxStack;
  }

  public bool HasCategory(ItemCategoryMask category) {
    if (category == ItemCategoryMask.Any) {
      return Categories != ItemCategoryMask.None;
    }

    return category != ItemCategoryMask.None && (Categories & category) == category;
  }
}
