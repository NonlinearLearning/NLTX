using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Wiring.Systems;

public static class LegacyTileContainerRegistry
{
  private static readonly IReadOnlySet<ushort> ContainerTileTypes = new HashSet<ushort>
  {
    21, 88, 467, 470, 475
  }.ToFrozenSet();

  public static IReadOnlySet<ushort> RegisterDefaults()
  {
    return ContainerTileTypes;
  }

  public static bool IsContainer(ushort tileType)
  {
    return ContainerTileTypes.Contains(tileType);
  }
}
