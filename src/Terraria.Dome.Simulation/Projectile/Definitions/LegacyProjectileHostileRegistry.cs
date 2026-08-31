using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Projectile.Definitions;

public static class LegacyProjectileHostileRegistry
{
  public const int ProjectileTypeCount = 1111;

  private static readonly IReadOnlySet<int> _hostileTypes = new HashSet<int>
  {
    12, 31, 38, 39, 40, 43, 44, 55, 56, 67, 71, 75, 81, 82, 83, 84, 96, 98, 100, 101,
    102, 108, 109, 110, 115, 128, 129, 164, 174, 176, 177, 179, 180, 184, 185, 186, 188,
    201, 202, 203, 204, 205, 240, 241, 257, 258, 259, 264, 270, 275, 276, 277, 281, 288,
    290, 291, 292, 293, 294, 299, 300, 302, 303, 325, 327, 329, 345, 346, 347, 348, 349,
    350, 351, 352, 384, 385, 386, 435, 436, 437, 438, 447, 448, 449, 450, 452, 454, 455,
    456, 462, 464, 465, 466, 467, 468, 471, 472, 490, 498, 501, 508, 526, 537, 538, 539,
    540, 572, 573, 574, 575, 576, 577, 580, 581, 592, 593, 596, 605, 629, 654, 655, 657,
    658, 661, 670, 671, 672, 675, 676, 681, 682, 683, 686, 687, 713, 719, 763, 802, 812,
    836, 871, 872, 873, 874, 909, 919, 920, 921, 922, 923, 926, 949, 961, 962, 965, 980,
    1001, 1002, 1005, 1007, 1021, 1048, 1049, 1073, 1078, 1091, 1092
  }.ToFrozenSet();

  public static IReadOnlySet<int> RegisterDefaults()
  {
    return _hostileTypes;
  }

  public static bool IsHostile(int projectileType)
  {
    return projectileType >= 0 && projectileType < ProjectileTypeCount &&
      _hostileTypes.Contains(projectileType);
  }
}
