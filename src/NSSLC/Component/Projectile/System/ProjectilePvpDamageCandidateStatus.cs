namespace Terraria.Projectile;

/// <summary>
/// Outcome of the pure PVP candidate filters that precede Version4 Colliding.
/// </summary>
public enum ProjectilePvpDamageCandidateStatus : byte
{
  ReadyForCollisionTest,
  DamageGateRejected,
  NonLocalOwner,
  NonPositiveDamage,
  LocalDamageOwnerNotHostile,
  InvalidTargetSlot,
  ProjectileOwnerTarget,
  InactiveTarget,
  DeadTarget,
  ImmuneTarget,
  NonHostileTarget,
  ProjectileImmunity,
  SameTeam,
  OwnerHitCheck,
}
