using System;

namespace Terraria.Dome.Simulation.Projectile.Definitions;

public static class LegacyProjectileLifetimePolicy
{
  public const int ArrowLifetimeTicks = 1200;

  public const int SentryLifetimeTicks = 36000;

  public static int Resolve(int lifetimeTicks, bool isSentry)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(lifetimeTicks);
    return isSentry ? SentryLifetimeTicks : lifetimeTicks;
  }
}
