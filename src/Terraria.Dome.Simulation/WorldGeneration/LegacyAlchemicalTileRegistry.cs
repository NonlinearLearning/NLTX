using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyAlchemicalTileRegistry
{
  private static readonly IReadOnlySet<ushort> AlchemicalTileTypes = new HashSet<ushort>
  {
    82, 83, 84
  }.ToFrozenSet();

  public static IReadOnlySet<ushort> RegisterDefaults()
  {
    return AlchemicalTileTypes;
  }

  public static bool IsAlchemical(ushort tileType)
  {
    return AlchemicalTileTypes.Contains(tileType);
  }
}
