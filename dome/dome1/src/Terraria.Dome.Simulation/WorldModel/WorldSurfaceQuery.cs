using System;

namespace Terraria.Dome.Simulation.WorldModel;

/// <summary>Server-only queries for world surface availability.</summary>
public static class WorldSurfaceQuery
{
  public const double MinimumFunctionalSurface = 50.0;

  public static bool IsThereAWorldSurface(WorldMetadata metadata)
  {
    ArgumentNullException.ThrowIfNull(metadata);
    return metadata.WorldSurface is double worldSurface &&
      double.IsFinite(worldSurface) &&
      worldSurface > MinimumFunctionalSurface;
  }
}
