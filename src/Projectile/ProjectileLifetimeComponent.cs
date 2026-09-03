namespace Terraria.Projectile;

public struct ProjectileLifetimeComponent
{
  public ProjectileLifetimeComponent(int remainingTicks, ProjectileEndReason endReason)
  {
    RemainingTicks = remainingTicks;
    EndReason = endReason;
  }

  public int RemainingTicks;
  public ProjectileEndReason EndReason;
}
