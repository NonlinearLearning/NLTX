using System;
using Terraria.Share.Attributes;

namespace Terraria.Share.Items;

/// <summary>
/// Pairs one attribute contribution with the inventory source category that supplies it.
/// </summary>
/// <remarks>
/// <see cref="AttributeContribution" /> describes what is contributed and deliberately carries no
/// source identity; this type adds only the category an item's contribution is attributed to.
/// </remarks>
public readonly record struct ItemAttributeContribution
{
  public ItemAttributeContribution(
      AttributeSourceCategory sourceCategory,
      AttributeContribution contribution)
  {
    if (!ItemAttributeContributionRules.IsInventorySource(sourceCategory))
    {
      throw new ArgumentOutOfRangeException(
          nameof(sourceCategory),
          sourceCategory,
          "Item attribute contributions must use an inventory source category.");
    }

    SourceCategory = sourceCategory;
    Contribution = contribution;
  }

  public AttributeSourceCategory SourceCategory { get; }

  public AttributeContribution Contribution { get; }
}
