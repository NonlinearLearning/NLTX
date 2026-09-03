using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyNoFailTileRegistry
{
  private static readonly IReadOnlySet<ushort> NoFailTileTypes = new HashSet<ushort>
  {
    3, 4, 24, 32, 50, 51, 52, 61, 62, 69, 73, 74, 81, 82, 83, 84, 110, 113, 115,
    129, 162, 165, 184, 185, 186, 187, 192, 201, 205, 227, 233, 254, 324, 330, 331,
    332, 333, 352, 373, 374, 375, 382, 384, 461, 481, 482, 483, 484, 485, 518, 519,
    528, 529, 530, 549, 624, 636, 637, 638, 654, 655, 656, 666, 697, 700, 701, 705,
    709
  }.ToFrozenSet();

  public static IReadOnlySet<ushort> RegisterDefaults()
  {
    return NoFailTileTypes;
  }

  public static bool IsNoFail(ushort tileType)
  {
    return NoFailTileTypes.Contains(tileType);
  }
}
