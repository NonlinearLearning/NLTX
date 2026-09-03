using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyFlameTileRegistry
{
  private static readonly IReadOnlySet<ushort> FlameTileTypes = new HashSet<ushort>
  {
    4,
    33,
    34,
    35,
    42,
    49,
    93,
    98,
    100,
    173,
    174,
    372,
    646
  }.ToFrozenSet();

  public static IReadOnlySet<ushort> RegisterDefaults()
  {
    return FlameTileTypes;
  }

  public static bool IsFlameTile(ushort tileType)
  {
    return FlameTileTypes.Contains(tileType);
  }
}
