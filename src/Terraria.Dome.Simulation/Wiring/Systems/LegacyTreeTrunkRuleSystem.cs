using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldGeneration;

namespace Terraria.Dome.Simulation.Wiring.Systems;

public static class LegacyTreeTrunkRuleSystem
{
  public static IReadOnlySet<ushort> RegisterDefaults()
  {
    return TreeTrunkTileRegistry.RegisterDefaults();
  }

  public static bool IsTreeTrunk(ushort tileType)
  {
    return TreeTrunkTileRegistry.RegisterDefaults().Contains(tileType);
  }
}
