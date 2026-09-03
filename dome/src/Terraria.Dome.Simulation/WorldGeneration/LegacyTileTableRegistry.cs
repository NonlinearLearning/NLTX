using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyTileTableRegistry
{
  private static readonly IReadOnlySet<ushort> TableTileTypes = new HashSet<ushort>
  {
    14, 18, 19, 87, 88, 101, 114, 275, 276, 277, 278, 279, 280, 281, 285, 286,
    296, 297, 298, 299, 309, 310, 339, 358, 359, 361, 362, 363, 364, 376, 380,
    391, 392, 393, 394, 405, 413, 414, 427, 469, 532, 533, 538, 542, 544, 550,
    551, 553, 554, 555, 556, 558, 559, 582, 599, 600, 601, 602, 603, 604, 605,
    606, 607, 608, 609, 610, 611, 612, 619, 629, 632, 640, 643, 644, 645, 710
  }.ToFrozenSet();

  public static IReadOnlySet<ushort> RegisterDefaults()
  {
    return TableTileTypes;
  }

  public static bool IsTable(ushort tileType)
  {
    return TableTileTypes.Contains(tileType);
  }
}
