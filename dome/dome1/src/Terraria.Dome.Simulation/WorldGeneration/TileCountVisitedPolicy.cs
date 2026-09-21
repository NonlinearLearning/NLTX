using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TileCountVisitedPolicy
{
  public static TileCountVisitedResult Visit(
    TileCountVisitedSnapshot state,
    int x,
    int y)
  {
    bool alreadyVisited = state.Contains(x, y);
    if (alreadyVisited)
    {
      return new TileCountVisitedResult(state, true);
    }

    HashSet<(int X, int Y)> coordinates = new(state.Coordinates)
    {
      (x, y)
    };
    return new TileCountVisitedResult(new TileCountVisitedSnapshot(coordinates), false);
  }
}
