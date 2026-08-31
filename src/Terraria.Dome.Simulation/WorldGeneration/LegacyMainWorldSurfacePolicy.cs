using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyMainWorldSurfacePolicy
{
  private const int TerrainSurfacePadding = 25;

  public static int Resolve(
    LegacyTerrainRuntimeProfile profile,
    WorldMetadata metadata)
  {
    ArgumentNullException.ThrowIfNull(profile);
    ArgumentNullException.ThrowIfNull(metadata);
    profile.Validate(metadata);
    int worldSurface = (int)(profile.WorldSurfaceHigh + TerrainSurfacePadding);
    return Math.Clamp(worldSurface, 0, metadata.Height - 1);
  }
}
