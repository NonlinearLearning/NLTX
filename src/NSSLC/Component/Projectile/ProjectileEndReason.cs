namespace Terraria.Projectile;

public enum ProjectileEndReason : byte
{
  None,
  LifetimeExpired,
  HitLimitReached,
  DestroyedByCollision,
  NetworkTermination,
  WorldBoundary,
}
