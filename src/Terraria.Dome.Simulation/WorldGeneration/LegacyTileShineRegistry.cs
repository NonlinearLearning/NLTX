using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyTileShineRegistry
{
  private static readonly IReadOnlyDictionary<ushort, short> Values =
    new Dictionary<ushort, short>
    {
      [6] = 1150, [7] = 1100, [8] = 1000, [9] = 1050, [12] = 300, [21] = 1200,
      [22] = 1150, [45] = 1900, [46] = 2000, [47] = 2100, [63] = 900, [64] = 900,
      [65] = 900, [66] = 900, [67] = 900, [68] = 900, [107] = 950, [108] = 900,
      [109] = 9000, [110] = 9000, [111] = 850, [116] = 9000, [117] = 9000,
      [118] = 8000, [121] = 1850, [122] = 1800, [125] = 600, [129] = 300,
      [166] = 1125, [167] = 1075, [168] = 1025, [169] = 975, [178] = 500,
      [204] = 1150, [211] = 500, [221] = 925, [222] = 875, [223] = 825,
      [239] = 1100, [346] = 2000, [347] = 1900, [348] = 1800, [370] = 1900,
      [407] = 1000, [441] = 1200, [467] = 1200, [468] = 1200, [566] = 900,
      [617] = 400, [639] = 300, [680] = 1900, [681] = 2000, [682] = 2100,
      [685] = 1850, [686] = 1800
    }.ToFrozenDictionary();

  public static IReadOnlyDictionary<ushort, short> RegisterDefaults()
  {
    return Values;
  }

  public static short GetValue(ushort tileType)
  {
    return Values.TryGetValue(tileType, out short value) ? value : (short)0;
  }
}
