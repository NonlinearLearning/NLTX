using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyPileTileRegistry
{
  private static readonly IReadOnlySet<ushort> PileTileTypes = new HashSet<ushort>
  {
    330, 331, 332, 333
  }.ToFrozenSet();

  public static IReadOnlySet<ushort> RegisterDefaults()
  {
    return PileTileTypes;
  }

  public static bool IsPileTile(ushort tileType)
  {
    return PileTileTypes.Contains(tileType);
  }
}
