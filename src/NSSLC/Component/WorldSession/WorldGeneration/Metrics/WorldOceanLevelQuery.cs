using System;

namespace Terraria.WorldGeneration.Metrics;

public static class WorldOceanLevelQuery
{
  private const double SurfaceOffset = 40.0;

  public static double Evaluate(double worldSurface, double rockLayer)
  {
    if (!double.IsFinite(worldSurface))
    {
      throw new ArgumentOutOfRangeException(nameof(worldSurface));
    }

    if (!double.IsFinite(rockLayer))
    {
      throw new ArgumentOutOfRangeException(nameof(rockLayer));
    }

    return (worldSurface + rockLayer) / 2.0 + SurfaceOffset;
  }
}
