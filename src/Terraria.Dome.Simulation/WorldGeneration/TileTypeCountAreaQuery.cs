using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TileTypeCountAreaQuery
{
  public static IReadOnlyList<int> Count(
    WorldGridSnapshot snapshot,
    int startX,
    int endX,
    int startY,
    int endY)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    if (startX < 0 || endX >= snapshot.Metadata.Width || startX > endX ||
        startY < 0 || endY >= snapshot.Metadata.Height || startY > endY)
    {
      throw new ArgumentOutOfRangeException(nameof(startX));
    }

    int[] counts = new int[TileDefinitionRegistry.Version4TileCount];
    for (int x = startX; x <= endX; x++)
    {
      for (int y = startY; y <= endY; y++)
      {
        WorldTile tile = snapshot.GetTile(x, y);
        if (tile.IsActive && tile.Type < counts.Length)
        {
          counts[tile.Type]++;
        }
      }
    }

    return counts;
  }
}
