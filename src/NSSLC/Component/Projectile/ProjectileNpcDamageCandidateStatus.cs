namespace Terraria.Projectile;

/// <summary>
/// Outcome of the pure NPC candidate filters that precede Version4 Colliding.
/// </summary>
public enum ProjectileNpcDamageCandidateStatus : byte
{
  ReadyForCollisionTest,
  DamageGateRejected,
  NonLocalOwner,
  NonPositiveDamage,
  InactiveTarget,
  ProjectileImmunity,
  OwnerMeleeHitCooldown,
  InvulnerableTarget,
  NpcAiImmunity,
  DamageRelationship,
  OwnerImmunity,
  ProjectileTargetImmunity,
  OwnerHitCheck,
}
