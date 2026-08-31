using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Projectile.Definitions;

public static class LegacyProjectileHostileDamageScalingRegistry
{
  private static readonly IReadOnlySet<int> _lightningScalingTypes = new HashSet<int>
  {
    1091
  }.ToFrozenSet();

  public static IReadOnlySet<int> LightningScalingTypes => _lightningScalingTypes;

  public static bool UsesLightningScaling(int projectileType)
  {
    return _lightningScalingTypes.Contains(projectileType);
  }
}
