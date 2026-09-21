using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Projectile.Definitions;

public static class LegacyProjectileHookRegistry
{
  public const int ProjectileTypeCount = 1111;

  private static readonly IReadOnlySet<int> _hookTypes = new HashSet<int>
  {
    13, 32, 72, 165, 229, 256, 315, 322, 331, 332,
    372, 396, 403, 446, 489, 645, 652, 753, 865, 935
  }.ToFrozenSet();

  public static IReadOnlySet<int> RegisterDefaults()
  {
    return _hookTypes;
  }

  public static bool IsHook(int projectileType)
  {
    return projectileType >= 0 && projectileType < ProjectileTypeCount &&
      _hookTypes.Contains(projectileType);
  }
}
