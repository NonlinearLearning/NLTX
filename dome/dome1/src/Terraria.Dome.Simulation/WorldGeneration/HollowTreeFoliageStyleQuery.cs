using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class HollowTreeFoliageStyleQuery
{
  private static readonly IReadOnlyDictionary<int, int> DefaultStyles =
    new Dictionary<int, int>
    {
      [2] = 20,
      [3] = 20,
      [4] = 19
    }.ToFrozenDictionary();

  public static IReadOnlyDictionary<int, int> RegisterDefaults()
  {
    return DefaultStyles;
  }

  public static int GetStyle(int hallowBackgroundStyle)
  {
    return DefaultStyles.TryGetValue(hallowBackgroundStyle, out int style) ? style : 3;
  }
}
