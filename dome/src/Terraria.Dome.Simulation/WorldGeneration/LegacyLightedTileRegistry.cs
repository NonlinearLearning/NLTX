using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyLightedTileRegistry
{
  private static readonly IReadOnlySet<ushort> LightedTileTypes = new HashSet<ushort>
  {
    4, 17, 19, 20, 22, 26, 27, 31, 33, 34, 35, 37,
    42, 49, 58, 61, 70, 71, 72, 76, 77, 83, 84, 92,
    93, 95, 96, 98, 100, 109, 125, 126, 129, 133, 140, 149,
    160, 171, 173, 174, 184, 190, 204, 209, 215, 237, 238, 262,
    263, 264, 265, 266, 267, 268, 270, 271, 286, 302, 316, 317,
    318, 327, 336, 340, 341, 342, 343, 344, 346, 347, 348, 349,
    350, 354, 356, 370, 372, 381, 390, 391, 405, 415, 416, 417,
    418, 429, 463, 491, 500, 501, 502, 503, 517, 519, 528, 534,
    535, 536, 537, 539, 540, 548, 564, 568, 569, 570, 572, 578,
    580, 581, 582, 592, 593, 594, 597, 598, 613, 614, 619, 620,
    625, 626, 627, 628, 633, 634, 637, 638, 646, 656, 658, 659,
    660, 663, 667, 684, 687, 688, 689, 690, 691, 692, 695, 696,
    699, 701, 703, 708, 711, 717, 718, 719, 739
  }.ToFrozenSet();

  public static IReadOnlySet<ushort> RegisterDefaults()
  {
    return LightedTileTypes;
  }

  public static bool IsLighted(ushort tileType)
  {
    return LightedTileTypes.Contains(tileType);
  }
}
