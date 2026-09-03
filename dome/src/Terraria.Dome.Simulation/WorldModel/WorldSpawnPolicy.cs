using System;

namespace Terraria.Dome.Simulation.WorldModel;

public static class WorldSpawnPolicy
{
  public static bool IsValid(WorldMetadata metadata, int x, int y)
  {
    ArgumentNullException.ThrowIfNull(metadata);
    return metadata.IsInside(x, y);
  }

  public static (int X, int Y) Resolve(WorldMetadata metadata)
  {
    ArgumentNullException.ThrowIfNull(metadata);
    return (metadata.SpawnX, metadata.SpawnY);
  }
}
