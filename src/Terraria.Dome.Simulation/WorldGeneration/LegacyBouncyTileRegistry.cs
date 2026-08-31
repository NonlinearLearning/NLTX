using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyBouncyTileRegistry
{
  private static readonly IReadOnlySet<ushort> BouncyTileTypes = new HashSet<ushort>
  {
    371, 446, 447, 448
  }.ToFrozenSet();

  public static IReadOnlySet<ushort> RegisterDefaults()
  {
    return BouncyTileTypes;
  }

  public static bool IsBouncy(ushort tileType)
  {
    return BouncyTileTypes.Contains(tileType);
  }
}
