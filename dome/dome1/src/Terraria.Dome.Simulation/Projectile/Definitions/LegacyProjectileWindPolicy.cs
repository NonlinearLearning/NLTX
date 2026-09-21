using System;

namespace Terraria.Dome.Simulation.Projectile.Definitions;

public static class LegacyProjectileWindPolicy
{
  public const float MinimumWindStrengthToFlyKite = 0.2f;

  public static bool CanFlyKite(float windSpeedCurrent)
  {
    return float.IsFinite(windSpeedCurrent) &&
      MathF.Abs(windSpeedCurrent) >= MinimumWindStrengthToFlyKite;
  }
}
