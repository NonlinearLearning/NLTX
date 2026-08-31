using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyBrickTileRegistry
{
  private static readonly IReadOnlySet<ushort> _brickTileTypes = new HashSet<ushort>
  {
    1, 2, 23, 25, 30, 37, 38, 39, 41, 43, 44, 45, 46, 47,
    53, 54, 57, 59, 60, 70, 75, 76, 109, 112, 116, 117, 118, 119,
    120, 121, 122, 140, 147, 148, 150, 151, 152, 153, 154, 155,
    156, 157, 158, 159, 160, 161, 162, 163, 164, 175, 176, 177,
    179, 180, 181, 182, 183, 188, 190, 191, 193, 194, 195, 198,
    199, 200, 202, 203, 206, 208, 225, 226, 229, 234, 248, 249,
    250, 252, 253, 255, 256, 257, 258, 259, 260, 261, 262, 263,
    264, 265, 266, 267, 268, 273, 274, 311, 315, 321, 322, 326,
    327, 328, 329, 345, 346, 347, 348, 350, 357, 369, 370, 381,
    383, 385, 408, 409, 415, 416, 417, 418, 458, 459, 472, 473,
    474, 477, 478, 479, 481, 482, 483, 492, 495, 496, 498, 500, 501,
    502, 503, 507, 508, 512, 513, 514, 515, 516, 517, 534, 535,
    536, 537, 539, 540, 562, 563, 625, 626, 627, 628, 633, 635,
    641, 659, 661, 662, 666, 667, 669, 670, 671, 672, 673, 674,
    675, 676, 677, 678, 679, 680, 681, 682, 683, 684, 685, 686,
    687, 688, 689, 690, 691, 692, 708, 722, 734, 735, 736, 737,
    738, 740, 741, 742, 743, 744, 745, 746, 747, 748, 749, 750
  }.ToFrozenSet();

  public static IReadOnlySet<ushort> RegisterDefaults()
  {
    return _brickTileTypes;
  }

  public static bool IsBrickTile(ushort tileType)
  {
    return _brickTileTypes.Contains(tileType);
  }
}
