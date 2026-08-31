using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyHammerTileRegistry
{
  private static readonly IReadOnlySet<ushort> HammerTileTypes = new HashSet<ushort>
  {
    26, 31, 695, 696
  }.ToFrozenSet();

  public static IReadOnlySet<ushort> RegisterDefaults()
  {
    return HammerTileTypes;
  }

  public static bool IsHammerTarget(ushort tileType)
  {
    return HammerTileTypes.Contains(tileType);
  }
}
