using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class CommonSaplingTileRegistry
{
  private static readonly IReadOnlySet<ushort> DefaultTileTypes = new HashSet<ushort>
  {
    20,
    590,
    595,
    615
  }.ToFrozenSet();

  public static IReadOnlySet<ushort> RegisterDefaults()
  {
    return DefaultTileTypes;
  }
}
