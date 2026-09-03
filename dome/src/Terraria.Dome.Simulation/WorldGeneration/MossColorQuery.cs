using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class MossColorQuery
{
  private static readonly IReadOnlyDictionary<int, int> DefaultColors =
    new Dictionary<int, int>
    {
      [179] = 0, [512] = 0, [180] = 1, [513] = 1, [181] = 2, [514] = 2,
      [182] = 3, [515] = 3, [183] = 4, [516] = 4, [381] = 5, [517] = 5,
      [534] = 6, [535] = 6, [536] = 7, [537] = 7, [539] = 8, [540] = 8,
      [625] = 9, [626] = 9, [627] = 10, [628] = 10
    }.ToFrozenDictionary();

  public static IReadOnlyDictionary<int, int> RegisterDefaults()
  {
    return DefaultColors;
  }

  public static int GetColor(int tileType)
  {
    return DefaultColors.TryGetValue(tileType, out int color) ? color : -1;
  }
}
