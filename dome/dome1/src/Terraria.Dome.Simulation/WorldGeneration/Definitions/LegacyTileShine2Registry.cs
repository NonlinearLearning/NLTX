using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration.Definitions;

public static class LegacyTileShine2Registry
{
  private static readonly FrozenSet<int> _tileTypes = CreateDefaults();

  public static IReadOnlySet<int> RegisterDefaults()
  {
    return _tileTypes;
  }

  public static bool HasShine(int tileType)
  {
    return _tileTypes.Contains(tileType);
  }

  private static FrozenSet<int> CreateDefaults()
  {
    HashSet<int> types = new()
    {
      6, 7, 8, 9, 12, 21, 22, 25, 45, 46, 47, 63, 64, 65, 66, 67, 68,
      107, 108, 111, 117, 121, 122, 147, 161, 163, 164, 166, 167, 168, 169,
      178, 204, 211, 221, 222, 223, 346, 347, 348, 370, 407, 441, 467, 468,
      566, 639, 680, 681, 682, 685, 686
    };
    for (int tileType = 262; tileType <= 268; tileType++)
    {
      types.Add(tileType);
    }

    return types.ToFrozenSet();
  }
}
