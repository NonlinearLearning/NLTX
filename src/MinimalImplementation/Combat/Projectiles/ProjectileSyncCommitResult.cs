using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Projectiles;

public sealed record ProjectileSyncCommitResult(
  OwnerCommitResult Result,
  ProjectileView? Projectile,
  ProjectileView? EvictedProjectile);
