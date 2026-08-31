using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration.Definitions;

public static class LegacyTileBlendAllRegistry
{
  private static readonly FrozenSet<int> _tileTypes = CreateDefaults();

  public static IReadOnlySet<int> RegisterDefaults()
  {
    return _tileTypes;
  }

  public static bool IsBlendAll(int tileType)
  {
    return _tileTypes.Contains(tileType);
  }

  private static FrozenSet<int> CreateDefaults()
  {
    return new HashSet<int> { 357 }.ToFrozenSet();
  }
}
