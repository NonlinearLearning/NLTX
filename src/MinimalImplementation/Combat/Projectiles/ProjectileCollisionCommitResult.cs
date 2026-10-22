using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Projectiles;

public sealed record ProjectileCollisionCommitResult(
  OwnerCommitResult Result,
  ProjectileView? UpdatedProjectile,
  ProjectileView? RetiredProjectile,
  OwnerCommitResult? TrackerCleanup);
