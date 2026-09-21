using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyMergeDirtTileRegistry
{
  private static readonly IReadOnlySet<ushort> MergeDirtTileTypes = new HashSet<ushort>
  {
    1, 6, 7, 8, 9, 22, 25, 30, 37, 38, 39, 40,
    41, 43, 44, 45, 46, 47, 53, 56, 107, 108, 111, 112,
    116, 117, 118, 119, 120, 121, 122, 123, 140, 145, 146, 148,
    150, 151, 152, 153, 154, 155, 156, 157, 158, 159, 160, 166,
    167, 168, 169, 175, 176, 177, 188, 190, 193, 195, 197, 198,
    202, 203, 204, 206, 208, 221, 222, 223, 229, 230, 234, 249,
    250, 251, 252, 253, 311, 315, 321, 322, 346, 347, 348, 350,
    367, 368, 369, 370, 371, 408, 472, 473, 474, 478, 479, 481,
    482, 483, 495, 496, 498, 500, 501, 502, 503, 562, 563, 635,
    641, 666, 667, 677, 678, 679, 680, 681, 682, 683, 684, 685,
    686, 722, 734, 740, 744, 750
  }.ToFrozenSet();

  public static IReadOnlySet<ushort> RegisterDefaults()
  {
    return MergeDirtTileTypes;
  }

  public static bool IsMergeDirtTile(ushort tileType)
  {
    return MergeDirtTileTypes.Contains(tileType);
  }
}
