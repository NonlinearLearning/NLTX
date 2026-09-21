using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacySpelunkerTileRegistry
{
  private static readonly IReadOnlySet<ushort> SpelunkerTileTypes = new HashSet<ushort>
  {
    6, 7, 8, 9, 12, 21, 28, 37, 63, 64, 65, 66, 67, 68, 83, 84, 105, 107, 108,
    111, 166, 167, 168, 169, 178, 211, 221, 222, 223, 227, 236, 240, 242, 245,
    246, 337, 349, 404, 407, 441, 467, 468, 506, 531, 566, 639, 702, 751, 752
  }.ToFrozenSet();

  public static IReadOnlySet<ushort> RegisterDefaults()
  {
    return SpelunkerTileTypes;
  }

  public static bool IsSpelunkerTile(ushort tileType)
  {
    return SpelunkerTileTypes.Contains(tileType);
  }
}
