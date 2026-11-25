namespace Terraria.Unshared.World;

/// <summary>Per-tick integration constants for one world item and the medium it stands in.</summary>
/// <remarks>
/// Version4 <c>WorldItem.UpdateItem</c> selects a gravity and fall-speed pair from the liquid the
/// item is standing in and scales the applied displacement by <c>0.5</c> while wet
/// (<c>WorldItem.MoveInWorld</c> / <c>WorldItem.ApplyMovement</c>).
/// </remarks>
public readonly record struct WorldItemMotionProfile(
    float Gravity,
    float MaximumFallSpeed,
    bool Wet)
{
  /// <summary>Version4 air constants: gravity <c>0.1</c>, maximum fall speed <c>7</c>.</summary>
  public static WorldItemMotionProfile Air => new(0.1f, 7f, false);

  /// <summary>Version4 water branch: gravity <c>0.08</c>, maximum fall speed <c>5</c>.</summary>
  public static WorldItemMotionProfile Water => new(0.08f, 5f, true);

  /// <summary>Version4 honey branch: gravity <c>0.05</c>, maximum fall speed <c>3</c>.</summary>
  public static WorldItemMotionProfile Honey => new(0.05f, 3f, true);

  /// <summary>Version4 shimmer branch: gravity <c>0.065</c>, maximum fall speed <c>4</c>.</summary>
  public static WorldItemMotionProfile Shimmer => new(0.065f, 4f, true);

  /// <summary>Lava reuses the water branch: Version4 only overrides shimmer and honey.</summary>
  public static WorldItemMotionProfile Lava => Water;
}
