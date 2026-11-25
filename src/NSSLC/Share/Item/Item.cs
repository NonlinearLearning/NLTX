using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Terraria.Share.Attributes;

namespace Terraria.Share.Items;

/// <summary>
/// Immutable rules and defaults shared by all stacks of an item kind.
/// </summary>
public sealed class Item {
  public Item(
      ItemId id,
      int maxStack,
      ItemCategoryMask categories = ItemCategoryMask.General,
      IEnumerable<ItemAttributeContribution>? attributeContributions = null) {
    if (!ItemIdRules.IsKnown(id)) {
      throw new ArgumentException("An item requires a known item id.", nameof(id));
    }

    if (maxStack <= 0) {
      throw new ArgumentOutOfRangeException(nameof(maxStack));
    }

    Id = id;
    MaxStack = maxStack;
    Categories = categories;

    ItemAttributeContribution[] contributions =
        (attributeContributions ?? Array.Empty<ItemAttributeContribution>()).ToArray();
    foreach (ItemAttributeContribution contribution in contributions) {
      if (!ItemAttributeContributionRules.IsInventorySource(contribution.SourceCategory)) {
        throw new ArgumentException(
            "Item attribute contributions must use an inventory source category.",
            nameof(attributeContributions));
      }
    }

    AttributeContributions = new ReadOnlyCollection<ItemAttributeContribution>(contributions);
  }

  public ItemId Id { get; }

  public int MaxStack { get; }

  public ItemCategoryMask Categories { get; }

  /// <summary>
  /// Attribute contributions rebuilt from an inventory relation by the attribute contribution system.
  /// </summary>
  public IReadOnlyList<ItemAttributeContribution> AttributeContributions { get; }

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
