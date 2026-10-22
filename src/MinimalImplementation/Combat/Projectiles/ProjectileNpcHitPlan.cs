using MinimalImplementation.Combat.Npcs;

namespace MinimalImplementation.Combat.Projectiles;

/// <summary>
/// One finite-penetration Arrow hit. Keep this instance through retries and emit its
/// projections only after Result is applied. It is used on the common owner thread.
/// </summary>
public sealed class ProjectileNpcHitPlan {
  internal ProjectileNpcHitPlan(
      ProjectileNpcCombatService owner,
      ProjectileCollisionResolutionPreparation collision, NpcDamageRequest damage) {
    Owner = owner;
    Collision = collision;
    Damage = damage;
  }

  internal ProjectileNpcCombatService Owner { get; }
  internal ProjectileCollisionResolutionPreparation Collision { get; }
  internal NpcDamageRequest Damage { get; }
  internal ProjectileCollisionResolutionResult? ConsumedProjectile { get; set; }
  internal ProjectileNpcHitResult? Completed { get; set; }
}
