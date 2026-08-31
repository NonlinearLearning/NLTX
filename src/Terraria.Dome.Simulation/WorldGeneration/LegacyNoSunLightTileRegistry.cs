using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyNoSunLightTileRegistry
{
  private static readonly IReadOnlySet<ushort> NoSunLightTileTypes = new HashSet<ushort>
  {
    11, 197, 386, 389, 630, 631
  }.ToFrozenSet();

  public static IReadOnlySet<ushort> RegisterDefaults()
  {
    return NoSunLightTileTypes;
  }

  public static bool IsNoSunLight(ushort tileType)
  {
    return NoSunLightTileTypes.Contains(tileType);
  }
}
