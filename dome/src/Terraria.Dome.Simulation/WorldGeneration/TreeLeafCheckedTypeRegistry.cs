using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TreeLeafCheckedTypeRegistry
{
  private static readonly IReadOnlySet<ushort> DefaultTileTypes = new HashSet<ushort>
  {
    5,
    72,
    323,
    583,
    584,
    585,
    586,
    587,
    588,
    589,
    596,
    616,
    634
  }.ToFrozenSet();

  public static IReadOnlySet<ushort> RegisterDefaults()
  {
    return DefaultTileTypes;
  }
}
