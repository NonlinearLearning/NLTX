using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyTileMergeRegistry
{
  public const ushort TileTypeCount = 753;

  private static readonly IReadOnlySet<LegacyTileMergePair> _tileMergePairs = new HashSet<
    LegacyTileMergePair>
  {
    new(426, 727),
    new(727, 426),
    new(430, 728),
    new(728, 430),
    new(431, 729),
    new(729, 431),
    new(432, 730),
    new(730, 432),
    new(433, 731),
    new(731, 433),
    new(434, 732),
    new(732, 434)
  }.ToFrozenSet();

  public static IReadOnlySet<LegacyTileMergePair> RegisterDefaults()
  {
    return _tileMergePairs;
  }

  public static bool CanMerge(ushort tileType, ushort mergedTileType)
  {
    return _tileMergePairs.Contains(new LegacyTileMergePair(tileType, mergedTileType));
  }
}
