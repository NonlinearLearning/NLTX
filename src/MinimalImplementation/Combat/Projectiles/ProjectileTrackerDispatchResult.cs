using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Projectiles;

public sealed record ProjectileTrackerDispatchResult(
  OwnerCommitResult Result,
  ProjectileTrackerSnapshotView? Snapshot,
  ProjectilePacketBody? Broadcast);
