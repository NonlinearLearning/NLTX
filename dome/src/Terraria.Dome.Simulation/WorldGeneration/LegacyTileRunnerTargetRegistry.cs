using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyTileRunnerTargetRegistry
{
  private static readonly IReadOnlySet<int> StoneTileTypes = new HashSet<int>
  {
    63,
    64,
    65,
    66,
    67,
    68,
    130,
    131,
    566
  }.ToFrozenSet();

  private static readonly IReadOnlySet<int> OreTileTypes = new HashSet<int>
  {
    6,
    7,
    8,
    9,
    22,
    37,
    58,
    107,
    108,
    111,
    166,
    167,
    168,
    169,
    204,
    211,
    221,
    222,
  223
  }.ToFrozenSet();

  public static int OreCount => OreTileTypes.Count;

  public static bool IsStone(int tileType)
  {
    return StoneTileTypes.Contains(tileType);
  }

  public static bool IsOre(int tileType)
  {
    return OreTileTypes.Contains(tileType);
  }
}
