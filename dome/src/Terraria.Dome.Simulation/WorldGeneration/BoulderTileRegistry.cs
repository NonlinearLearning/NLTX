using System;
using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class BoulderTileRegistry
{
  private static readonly IReadOnlyList<ushort> OrderedTileTypes =
    Array.AsReadOnly(new ushort[] { 138, 484, 664, 665, 711, 712, 713, 714, 715, 716 });
  private static readonly IReadOnlySet<ushort> DefaultTileTypes = new HashSet<ushort>
  {
    138,
    484,
    664,
    665,
    711,
    712,
    713,
    714,
    715,
    716
  }.ToFrozenSet();

  public static IReadOnlySet<ushort> RegisterDefaults()
  {
    return DefaultTileTypes;
  }

  public static IReadOnlyList<ushort> RegisterOrderedDefaults()
  {
    return OrderedTileTypes;
  }
}
