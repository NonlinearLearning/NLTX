namespace Terraria.Projectile;

public readonly record struct ProjectileTickResult(
  int ActiveProjectileCount,
  int SkippedInactiveCount,
  int UpdateStepCount)
{
  public int LifetimeExpiredCount { get; init; }
}
