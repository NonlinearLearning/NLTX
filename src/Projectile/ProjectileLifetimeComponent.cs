namespace Terraria.Projectile;

public struct ProjectileLifetimeComponent
{
  public ProjectileLifetimeComponent()
  {
    RemainingTicks = 3600;
    EndReason = ProjectileEndReason.None;
  }

  public ProjectileLifetimeComponent(
    int remainingTicks,
    ProjectileEndReason endReason = ProjectileEndReason.None)
  {
    RemainingTicks = remainingTicks;
    EndReason = endReason;
  }

  public int RemainingTicks;
  public ProjectileEndReason EndReason;

  public bool IsActive => RemainingTicks > 0;

  public bool IsExpired => RemainingTicks == 0;
}
