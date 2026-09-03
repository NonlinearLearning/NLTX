using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldGeneration;

namespace Terraria.Dome.Simulation.Wiring.Systems;

public static class LegacyBoulderRuleSystem
{
  public static IReadOnlySet<ushort> RegisterDefaults()
  {
    return BoulderTileRegistry.RegisterDefaults();
  }

  public static bool IsBoulder(ushort tileType)
  {
    return BoulderTileRegistry.RegisterDefaults().Contains(tileType);
  }
}
