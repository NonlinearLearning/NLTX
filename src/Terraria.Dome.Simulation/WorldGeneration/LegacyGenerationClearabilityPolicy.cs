using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyGenerationClearabilityPolicy
{
  private static readonly IReadOnlySet<int> NonClearableTileTypes = new HashSet<int>
  {
    41,
    43,
    44,
    226,
    237,
    367,
    368,
    396,
    397,
    398,
    399,
    400,
    401,
    404,
    481,
    482,
    483
  };

  public static bool CanClear(int tileType, bool isProtectedDungeonTile)
  {
    return tileType >= 0 && !isProtectedDungeonTile &&
      !NonClearableTileTypes.Contains(tileType);
  }
}
