using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyAxeTileRegistry
{
  private static readonly IReadOnlySet<ushort> AxeTileTypes = new HashSet<ushort>
  {
    5, 72, 80, 323, 488, 583, 584, 585, 586, 587, 588, 589, 596, 616, 634, 704
  }.ToFrozenSet();

  public static IReadOnlySet<ushort> RegisterDefaults()
  {
    return AxeTileTypes;
  }

  public static bool IsAxeTarget(ushort tileType)
  {
    return AxeTileTypes.Contains(tileType);
  }
}
