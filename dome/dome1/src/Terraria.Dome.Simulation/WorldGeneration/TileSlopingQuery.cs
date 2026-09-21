using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TileSlopingQuery
{
  private static readonly IReadOnlySet<ushort> NonSlopingTileTypes = new HashSet<ushort>
  {
    21, 26, 77, 88, 235, 237, 441, 467, 468, 470, 475, 488, 597
  }.ToFrozenSet();

  public static IReadOnlySet<ushort> RegisterDefaults()
  {
    return NonSlopingTileTypes;
  }

  public static bool ForbidsSloping(ushort tileType)
  {
    return NonSlopingTileTypes.Contains(tileType);
  }
}
