using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TreeTrunkTileRegistry
{
  private static readonly IReadOnlySet<ushort> DefaultTileTypes = new HashSet<ushort>
  {
    5,
    72,
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
  private static readonly IReadOnlySet<int> DefaultTileTypesAsInt =
    DefaultTileTypes.Select(static tileType => (int)tileType).ToFrozenSet();

  public static IReadOnlySet<ushort> RegisterDefaults()
  {
    return DefaultTileTypes;
  }

  public static IReadOnlySet<int> RegisterIntDefaults()
  {
    return DefaultTileTypesAsInt;
  }
}
