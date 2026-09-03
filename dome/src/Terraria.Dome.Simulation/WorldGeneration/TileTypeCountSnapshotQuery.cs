using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TileTypeCountSnapshotQuery
{
  public static TileTypeCountSnapshot Count(
    WorldGridSnapshot snapshot,
    int startX,
    int endX,
    int startY,
    int endY)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    return new TileTypeCountSnapshot(
      TileTypeCountAreaQuery.Count(snapshot, startX, endX, startY, endY));
  }
}
