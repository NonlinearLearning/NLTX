using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Projectiles;

public sealed record ProjectileSyncDispatchResult(
  OwnerCommitResult Result,
  ProjectileView? Projectile,
  ProjectileView? EvictedProjectile,
  ProjectilePacketBody? Broadcast,
  OwnerCommitResult? TrackerCleanup = null);
