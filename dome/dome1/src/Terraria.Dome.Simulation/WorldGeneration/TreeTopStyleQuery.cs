using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TreeTopStyleQuery
{
  public static int GetStyle(TreeTopStyleSnapshot snapshot, int areaId)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    if (areaId < 0 || areaId >= TreeTopStyleSnapshot.AreaCount)
    {
      throw new ArgumentOutOfRangeException(nameof(areaId));
    }

    return snapshot.Styles[areaId];
  }
}
