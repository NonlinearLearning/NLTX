using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class HousingRoomOccupancyQuery
{
  public static bool Contains(
    IReadOnlySet<(int X, int Y)> roomTiles,
    int x,
    int y)
  {
    ArgumentNullException.ThrowIfNull(roomTiles);
    return roomTiles.Contains((x, y));
  }
}
