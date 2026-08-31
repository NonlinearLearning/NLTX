using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class CrimsonHeartPositionPolicy
{
  public static bool TryAppend(
    CrimsonHeartPositionSnapshot current,
    int x,
    int y,
    out CrimsonHeartPositionSnapshot next)
  {
    if (current.Positions.Count >= CrimsonHeartPositionSnapshot.MaximumCount)
    {
      next = current;
      return false;
    }

    var positions = new List<(int X, int Y)>(current.Positions)
    {
      (x, y)
    };
    next = new CrimsonHeartPositionSnapshot(positions);
    return true;
  }
}
