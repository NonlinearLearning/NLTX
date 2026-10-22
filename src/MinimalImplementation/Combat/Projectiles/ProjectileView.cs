using System.Numerics;
using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Projectiles;

/// <summary>Detached per-commit projection; it is not mutable projectile state.</summary>
public sealed record ProjectileView(
  CombatEntityIdentity Entity,
  int RuntimeSlot,
  short ProtocolIdentity,
  byte Owner,
  SessionEpoch? OwnerEpoch,
  short Type,
  Vector2 Position,
  Vector2 Velocity,
  float Ai0,
  float Ai1,
  float Ai2,
  ushort BannerId,
  int Damage,
  float Knockback,
  int OriginalDamage,
  short? ProtocolUuid,
  int TimeLeft,
  bool Friendly,
  bool Hostile,
  bool NetImportant,
  bool NeedsProtocolUuid,
  long Revision,
  CommitVersion CommitVersion,
  int AiStyle,
  int Width,
  int Height,
  bool TileCollide,
  short Penetrate,
  short RemainingPenetration,
  bool IsDirty) {
  /// <summary>Committed owner state; the default describes a fresh packet-27 instance.</summary>
  public ProjectileLiquidState LiquidState { get; init; } = ProjectileLiquidState.Dry;

  /// <summary>Persistent owner state; packet 27 cannot set the update budget.</summary>
  public int ExtraUpdates { get; init; }
}
