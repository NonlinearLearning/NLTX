using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyTruncatingWallTileRegistry
{
  private static readonly IReadOnlySet<ushort> _truncatingTileTypes = new HashSet<ushort>
  {
    54,
    328,
    459,
    748
  }.ToFrozenSet();

  public static IReadOnlySet<ushort> RegisterDefaults()
  {
    return _truncatingTileTypes;
  }

  public static bool IsTruncatingTile(ushort tileType)
  {
    return _truncatingTileTypes.Contains(tileType);
  }
}
