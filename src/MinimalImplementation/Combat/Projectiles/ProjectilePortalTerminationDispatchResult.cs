using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Projectiles;

public sealed record ProjectilePortalTerminationDispatchResult(
  OwnerCommitResult Result,
  bool IsNoOp,
  ProjectileView? RetiredProjectile,
  ProjectilePacketBody? Broadcast,
  OwnerCommitResult? TrackerCleanup);
