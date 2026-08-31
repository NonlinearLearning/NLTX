using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Items;

public static class LegacyAnglerQuestItemRegistry
{
  public const byte QuestCount = 41;
  public const ushort ItemTypeCount = 6147;

  private static readonly IReadOnlyDictionary<byte, ushort> _itemTypes =
    new Dictionary<byte, ushort>
    {
      [0] = 2450,
      [1] = 2451,
      [2] = 2452,
      [3] = 2453,
      [4] = 2454,
      [5] = 2455,
      [6] = 2456,
      [7] = 2457,
      [8] = 2458,
      [9] = 2459,
      [10] = 2460,
      [11] = 2461,
      [12] = 2462,
      [13] = 2463,
      [14] = 2464,
      [15] = 2465,
      [16] = 2466,
      [17] = 2467,
      [18] = 2468,
      [19] = 2469,
      [20] = 2470,
      [21] = 2471,
      [22] = 2472,
      [23] = 2473,
      [24] = 2474,
      [25] = 2475,
      [26] = 2476,
      [27] = 2477,
      [28] = 2478,
      [29] = 2479,
      [30] = 2480,
      [31] = 2481,
      [32] = 2482,
      [33] = 2483,
      [34] = 2484,
      [35] = 2485,
      [36] = 2486,
      [37] = 2487,
      [38] = 2488,
      [39] = 4393,
      [40] = 4394
    }.ToFrozenDictionary();

  public static IReadOnlyDictionary<byte, ushort> RegisterDefaults()
  {
    return _itemTypes;
  }

  public static bool TryGetItemType(byte questIndex, out ushort itemType)
  {
    return _itemTypes.TryGetValue(questIndex, out itemType);
  }
}
