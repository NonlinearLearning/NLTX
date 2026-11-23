using System;
using System.Numerics;

namespace Terraria.World;

/// <summary>
/// Pure stacking arithmetic for world items, ported from Version4
/// <c>WorldItem.TryCombiningIntoNearbyItems</c>.
/// </summary>
/// <remarks>
/// Version4 only merges the item being updated into a later slot, requires the same stack
/// identity (item type plus prefix), the same shimmer state and the same reservation, and refuses
/// pairs whose Manhattan centre distance exceeds <c>30</c>.
/// </remarks>
public static class WorldItemMergeRules
{
  /// <summary>Version4 merge radius in pixels, compared as a Manhattan distance.</summary>
  public const float MaximumMergeDistance = 30f;

  /// <summary>How many units of <paramref name="source"/> fit into <paramref name="target"/>.</summary>
  public static int MergeAmount(
      in WorldItemState target,
      in WorldItemState source,
      int maximumStackSize)
  {
    ArgumentNullException.ThrowIfNull(target);
    ArgumentNullException.ThrowIfNull(source);
    if (maximumStackSize <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumStackSize));
    }

    if (target.Slot == source.Slot ||
        target.Item.IsEmpty || source.Item.IsEmpty ||
        !target.Item.CanStackWith(source.Item) ||
        target.ReservedFor != source.ReservedFor ||
        target.Shimmered != source.Shimmered ||
        target.Item.Quantity >= maximumStackSize ||
        MathF.Abs(target.Position.X - source.Position.X) +
            MathF.Abs(target.Position.Y - source.Position.Y) > MaximumMergeDistance)
    {
      return 0;
    }

    return Math.Min(source.Item.Quantity, maximumStackSize - target.Item.Quantity);
  }
}
