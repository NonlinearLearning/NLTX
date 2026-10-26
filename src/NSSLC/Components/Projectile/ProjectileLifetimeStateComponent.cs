using System;

namespace Terraria.Projectile;

/// <summary>Tracks finite projectile lifetime in simulation ticks.</summary>
public readonly record struct ProjectileLifetimeStateComponent
{
  public ProjectileLifetimeStateComponent(int remainingTicks)
  {
    if (remainingTicks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(remainingTicks));
    }

    RemainingTicks = remainingTicks;
  }

  public int RemainingTicks { get; }

  public bool IsExpired => RemainingTicks == 0;

  public ProjectileLifetimeStateComponent Advance()
  {
    return new ProjectileLifetimeStateComponent(Math.Max(0, RemainingTicks - 1));
  }
}
