using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class MossTileTypeRegistry
{
  private static readonly IReadOnlySet<ushort> _tileTypes = new HashSet<ushort>
  {
    179,
    180,
    181,
    182,
    183,
    381,
    534,
    536,
    539,
    625,
    627
  };

  public static IReadOnlySet<ushort> TileTypes => _tileTypes;
}
