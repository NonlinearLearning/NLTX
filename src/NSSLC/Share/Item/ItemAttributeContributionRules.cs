using System;
using Terraria.Share.Attributes;

namespace Terraria.Share.Items;

/// <summary>
/// Rules for which source categories an item's attribute contribution may use.
/// </summary>
public static class ItemAttributeContributionRules
{
  /// <summary>
  /// True when the category is one an inventory relation can supply.
  /// </summary>
  public static bool IsInventorySource(AttributeSourceCategory sourceCategory) =>
      sourceCategory is
          AttributeSourceCategory.Equipment or
          AttributeSourceCategory.Accessory or
          AttributeSourceCategory.Weapon;
}
