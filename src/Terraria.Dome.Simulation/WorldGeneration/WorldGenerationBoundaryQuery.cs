using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class WorldGenerationBoundaryQuery
{
  public static bool IsInOceanBand(
    WorldBoundsComponent bounds,
    int x,
    WorldGenerationDistanceDefaults distances)
  {
    ValidateX(bounds, x);
    return x < distances.OceanDistance || x > bounds.Width - distances.OceanDistance;
  }

  public static bool IsInBeachBand(
    WorldBoundsComponent bounds,
    int x,
    WorldGenerationDistanceDefaults distances)
  {
    ValidateX(bounds, x);
    return x < distances.BeachDistance || x > bounds.Width - distances.BeachDistance;
  }

  private static void ValidateX(WorldBoundsComponent bounds, int x)
  {
    if (x < 0 || x >= bounds.Width)
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }
  }
}
