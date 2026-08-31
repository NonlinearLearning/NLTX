using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Projectile.Definitions;

public static class LegacyPetProjectileRegistry
{
  public const int ProjectileTypeCount = 1111;

  private static readonly FrozenSet<int> _petProjectileTypes = new HashSet<int>
  {
    111, 112, 127, 175, 191, 192, 193, 194, 197, 198, 199, 200,
    208, 209, 210, 211, 236, 266, 268, 269, 313, 314, 317, 319,
    324, 334, 353, 373, 375, 380, 387, 388, 390, 391, 392, 393,
    394, 395, 398, 407, 423, 492, 499, 533, 613, 623, 625, 626,
    627, 628, 653, 701, 702, 703, 755, 758, 759, 764, 765, 774,
    815, 816, 817, 821, 825, 831, 833, 834, 835, 854, 858, 859,
    860, 864, 875, 881, 882, 883, 884, 885, 886, 887, 888, 889,
    890, 891, 892, 893, 894, 895, 896, 897, 898, 899, 900, 901,
    934, 946, 951, 956, 957, 958, 959, 960, 963, 970, 994, 998,
    1003, 1004, 1018, 1022, 1027, 1046, 1050, 1056, 1090, 1093,
    1094, 1095, 1096
  }.ToFrozenSet();

  public static IReadOnlySet<int> RegisterDefaults()
  {
    return _petProjectileTypes;
  }

  public static bool IsPet(int projectileType)
  {
    return _petProjectileTypes.Contains(projectileType);
  }
}
