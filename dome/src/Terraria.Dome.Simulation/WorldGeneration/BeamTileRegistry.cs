using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class BeamTileRegistry
{
  private static readonly IReadOnlySet<ushort> DefaultTileTypes = new HashSet<ushort>
  {
    124,
    561,
    574,
    575,
    576,
    577,
    578
  }.ToFrozenSet();

  public static IReadOnlySet<ushort> RegisterDefaults()
  {
    return DefaultTileTypes;
  }
}
