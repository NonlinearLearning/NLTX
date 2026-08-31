using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TileCountColumnRangePolicy
{
  private const int Border = 40;

  public static (TileCountColumnRange AboveSurface, TileCountColumnRange BelowSurface) Create(
    int worldHeight,
    int worldSurfaceY)
  {
    if (worldHeight <= Border * 2 || worldSurfaceY < Border ||
        worldSurfaceY + 1 >= worldHeight - Border)
    {
      throw new ArgumentOutOfRangeException(nameof(worldHeight));
    }

    TileCountColumnRange aboveSurface = new(Border, worldSurfaceY + 1, Weight: 5);
    TileCountColumnRange belowSurface = new(worldSurfaceY + 1, worldHeight - Border, Weight: 1);
    return (aboveSurface, belowSurface);
  }
}
