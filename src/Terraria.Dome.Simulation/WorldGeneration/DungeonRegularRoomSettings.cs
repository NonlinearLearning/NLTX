using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct DungeonRegularRoomSettings
{
  public DungeonRegularRoomSettings(int overrideInnerBoundsSize, int overrideOuterBoundsSize)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(overrideInnerBoundsSize);
    ArgumentOutOfRangeException.ThrowIfNegative(overrideOuterBoundsSize);
    OverrideInnerBoundsSize = overrideInnerBoundsSize;
    OverrideOuterBoundsSize = overrideOuterBoundsSize;
  }

  public int OverrideInnerBoundsSize { get; }

  public int OverrideOuterBoundsSize { get; }

  public int GetBoundingRadius()
  {
    return DungeonRoomBoundingRadiusQuery.Regular(
      OverrideInnerBoundsSize,
      OverrideOuterBoundsSize);
  }
}
