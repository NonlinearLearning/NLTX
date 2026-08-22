using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class RoomNeedsQuery
{
  public static RoomNeedsResult Evaluate(
    IReadOnlySet<int> houseTileTypes,
    IReadOnlySet<int> chairTileTypes,
    IReadOnlySet<int> tableTileTypes,
    IReadOnlySet<int> doorTileTypes,
    IReadOnlySet<int> torchTileTypes)
  {
    ArgumentNullException.ThrowIfNull(houseTileTypes);
    ArgumentNullException.ThrowIfNull(chairTileTypes);
    ArgumentNullException.ThrowIfNull(tableTileTypes);
    ArgumentNullException.ThrowIfNull(doorTileTypes);
    ArgumentNullException.ThrowIfNull(torchTileTypes);
    return new RoomNeedsResult(
      HasAny(houseTileTypes, chairTileTypes),
      HasAny(houseTileTypes, tableTileTypes),
      HasAny(houseTileTypes, doorTileTypes),
      HasAny(houseTileTypes, torchTileTypes));
  }

  private static bool HasAny(IReadOnlySet<int> houseTileTypes, IReadOnlySet<int> candidates)
  {
    foreach (int candidate in candidates)
    {
      if (houseTileTypes.Contains(candidate))
      {
        return true;
      }
    }

    return false;
  }
}
