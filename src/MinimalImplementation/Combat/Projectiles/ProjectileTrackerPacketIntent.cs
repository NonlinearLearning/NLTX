using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Projectiles;

/// <summary>Packet-142 snapshot body committed by the projectile tracker owner.</summary>
public sealed record ProjectileTrackerPacketIntent(
  byte Player,
  SessionEpoch OwnerEpoch,
  long TrackerRevision,
  CommitVersion CommitVersion,
  ProjectilePacketBody Packet);
