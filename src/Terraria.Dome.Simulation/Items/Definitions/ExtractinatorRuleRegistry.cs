using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Terraria.Dome.Simulation.Items.Definitions;

public sealed class ExtractinatorRuleRegistry
{
  private readonly IReadOnlyDictionary<ushort, int> _modes;
  private readonly IReadOnlyDictionary<ushort, ushort> _chlorophyteTrades;

  private ExtractinatorRuleRegistry(
    IReadOnlyDictionary<ushort, int> modes,
    IReadOnlyDictionary<ushort, ushort> chlorophyteTrades)
  {
    _modes = modes;
    _chlorophyteTrades = chlorophyteTrades;
  }

  public static ExtractinatorRuleRegistry CreateVersion4()
  {
    Dictionary<ushort, int> modes = new()
    {
      [424] = 0,
      [1103] = 0,
      [3347] = 1,
      [2339] = 2,
      [2338] = 2,
      [2337] = 2,
      [4354] = 3,
      [4389] = 3,
      [4377] = 3,
      [4378] = 3,
      [5127] = 3,
      [5128] = 3,
      [5395] = 4,
      [1124] = 5,
      [4090] = 6,
      [173] = 6
    };
    Dictionary<ushort, ushort> trades = new()
    {
      [12] = 699,
      [699] = 12,
      [11] = 700,
      [700] = 11,
      [14] = 701,
      [701] = 14,
      [13] = 702,
      [702] = 13,
      [56] = 880,
      [880] = 56,
      [364] = 1104,
      [1104] = 364,
      [365] = 1105,
      [1105] = 365,
      [366] = 1106,
      [1106] = 366,
      [134] = 137,
      [137] = 139,
      [139] = 134,
      [20] = 703,
      [703] = 20,
      [22] = 704,
      [704] = 22,
      [21] = 705,
      [705] = 21,
      [19] = 706,
      [706] = 19,
      [57] = 1257,
      [1257] = 57,
      [381] = 1184,
      [1184] = 381,
      [382] = 1191,
      [1191] = 382,
      [391] = 1198,
      [1198] = 391,
      [86] = 1329,
      [1329] = 86,
      [61] = 3,
      [836] = 3,
      [409] = 3,
      [370] = 169,
      [1246] = 169,
      [408] = 169,
      [833] = 664,
      [835] = 664,
      [834] = 664,
      [3276] = 3271,
      [3277] = 3271,
      [3339] = 3271,
      [3274] = 3272,
      [3275] = 3272,
      [3338] = 3272
    };
    return new ExtractinatorRuleRegistry(
      new ReadOnlyDictionary<ushort, int>(modes),
      new ReadOnlyDictionary<ushort, ushort>(trades));
  }

  public bool TryGetChlorophyteTrade(ushort itemType, out ushort resultItemType)
  {
    return _chlorophyteTrades.TryGetValue(itemType, out resultItemType);
  }

  public bool TryGetMode(ushort itemType, out int extractionMode)
  {
    return _modes.TryGetValue(itemType, out extractionMode);
  }

  public static bool IsSupportedMode(int extractionMode)
  {
    return extractionMode >= -1 && extractionMode <= 6;
  }
}
