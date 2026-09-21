using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

/// <summary>Owns the server-side playable boundary derived from Main.offLimitBorderTiles.</summary>
public static class WorldBoundaryPolicy
{
  public const int OffLimitBorderTiles = 40;

  public static bool IsInside(int x, int y, int width, int height)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
    return x >= OffLimitBorderTiles && x < width - OffLimitBorderTiles &&
      y >= OffLimitBorderTiles && y < height - OffLimitBorderTiles;
  }
}
