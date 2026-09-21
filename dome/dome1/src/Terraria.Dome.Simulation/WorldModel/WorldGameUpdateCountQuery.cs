using System;

namespace Terraria.Dome.Simulation.WorldModel;

/// <summary>Server-only access to the simulation update cursor.</summary>
public static class WorldGameUpdateCountQuery
{
  public static uint Value(WorldGameUpdateCountProjection projection)
  {
    ArgumentNullException.ThrowIfNull(projection);
    return projection.Value;
  }
}
