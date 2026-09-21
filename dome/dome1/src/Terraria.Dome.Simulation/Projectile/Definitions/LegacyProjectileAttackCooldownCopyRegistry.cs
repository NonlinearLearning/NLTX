using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Projectile.Definitions;

public static class LegacyProjectileAttackCooldownCopyRegistry
{
  private static readonly IReadOnlySet<int> _copyTypes = new HashSet<int>
  {
    1059, 1060, 1061, 1062, 1063, 1064, 1065, 1066, 1067, 1068,
    1069, 1070, 1071, 1072, 1074, 1075, 1076, 1101, 1102
  }.ToFrozenSet();

  public static IReadOnlySet<int> CopyTypes => _copyTypes;

  public static bool IsEnabled(int projectileType)
  {
    return _copyTypes.Contains(projectileType);
  }
}
