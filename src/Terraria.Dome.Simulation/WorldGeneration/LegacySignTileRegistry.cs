using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacySignTileRegistry
{
  private static readonly IReadOnlySet<ushort> SignTileTypes = new HashSet<ushort>
  {
    55, 85, 425, 573
  }.ToFrozenSet();

  public static IReadOnlySet<ushort> RegisterDefaults()
  {
    return SignTileTypes;
  }

  public static bool IsSign(ushort tileType)
  {
    return SignTileTypes.Contains(tileType);
  }
}
