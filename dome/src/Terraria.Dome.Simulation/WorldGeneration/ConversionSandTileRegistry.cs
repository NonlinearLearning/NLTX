using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class ConversionSandTileRegistry
{
  private static readonly IReadOnlySet<ushort> DefaultTileTypes = new HashSet<ushort>
  {
    53,
    112,
    116,
    234
  }.ToFrozenSet();

  public static IReadOnlySet<ushort> RegisterDefaults()
  {
    return DefaultTileTypes;
  }
}
