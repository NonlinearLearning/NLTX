using System;

namespace Terraria.Projectile;

public struct ProjectileLifetimeStateComponent
{
  public const int DefaultTimeLeft = 3600;

  public ProjectileLifetimeStateComponent(
    bool active = true,
    int timeLeft = DefaultTimeLeft,
    ProjectileEndReason endReason = ProjectileEndReason.None)
  {
    if (timeLeft < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(timeLeft));
    }

    if (endReason != ProjectileEndReason.None && (active || timeLeft != 0))
    {
      throw new ArgumentException(
        "A terminated projectile must be inactive with no remaining time.",
        nameof(endReason));
    }

    Active = active;
    TimeLeft = timeLeft;
    EndReason = endReason;
  }

  public bool Active;

  public int TimeLeft;

  public ProjectileEndReason EndReason;

  public bool IsActive => Active;

  public bool IsExpired => TimeLeft == 0;
}
