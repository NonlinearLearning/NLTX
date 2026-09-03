using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyWorldBorderPolicy
{
  public const int OffLimitBorderTiles = WorldBoundaryPolicy.OffLimitBorderTiles;

  public static bool IsInsideGameplayBounds(int x, int y, int width, int height)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
    return WorldBoundaryPolicy.IsInside(x, y, width, height);
  }
}
