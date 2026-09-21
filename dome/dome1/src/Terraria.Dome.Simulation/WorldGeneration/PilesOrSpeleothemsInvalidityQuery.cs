using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class PilesOrSpeleothemsInvalidityQuery
{
  public static bool Evaluate(WorldGridSnapshot snapshot, int x, int y)
  {
    return Evaluate(snapshot, x, y, BoulderTileRegistry.RegisterDefaults());
  }

  public static bool Evaluate(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    IReadOnlySet<ushort> boulderTileTypes)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(boulderTileTypes);
    if (x < 2 || y < 2 || x >= snapshot.Metadata.Width - 2 ||
        y >= snapshot.Metadata.Height - 2)
    {
      return false;
    }

    WorldTile tile = snapshot.GetTile(x, y);
    return tile.IsActive && boulderTileTypes.Contains(tile.Type);
  }
}
