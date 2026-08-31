using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class OceanLevelQuery
{
  private const double SurfaceOffset = 40.0;

  public static double Evaluate(double worldSurface, double rockLayer)
  {
    if (double.IsNaN(worldSurface) || double.IsInfinity(worldSurface))
    {
      throw new ArgumentOutOfRangeException(nameof(worldSurface));
    }

    if (double.IsNaN(rockLayer) || double.IsInfinity(rockLayer))
    {
      throw new ArgumentOutOfRangeException(nameof(rockLayer));
    }

    return (worldSurface + rockLayer) / 2.0 + SurfaceOffset;
  }

  public static double Evaluate(TerrainProfileComponent profile)
  {
    return Evaluate(profile.SurfaceY, profile.RockLayerY);
  }
}
