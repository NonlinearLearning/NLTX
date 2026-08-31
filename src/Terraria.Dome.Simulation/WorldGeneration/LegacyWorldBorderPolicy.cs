using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyWorldBorderPolicy
{
  public const int OffLimitBorderTiles = 40;

  public static bool IsInsideGameplayBounds(int x, int y, int width, int height)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
    return x >= OffLimitBorderTiles && x < width - OffLimitBorderTiles &&
      y >= OffLimitBorderTiles && y < height - OffLimitBorderTiles;
  }
}
