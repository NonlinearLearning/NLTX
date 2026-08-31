using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TreeTypeClassificationQuery
{
  public static bool IsTreeType(int tileType)
  {
    return IsTreeType(tileType, TreeTrunkTileRegistry.RegisterIntDefaults());
  }

  public static bool IsTreeType(int tileType, IReadOnlySet<int> treeTrunkTypes)
  {
    return tileType >= 0 && treeTrunkTypes.Contains(tileType);
  }
}
