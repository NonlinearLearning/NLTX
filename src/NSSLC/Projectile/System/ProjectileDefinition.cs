using System;
using Terraria.Ai;
using Terraria.Attributes;
using Terraria.Combat;
using Terraria.Physics;

namespace Terraria.Projectile;

/// <summary>
/// Immutable projectile content definition. It contains defaults, not a runtime entity instance.
/// </summary>
public sealed class ProjectileDefinition
{
  public ProjectileDefinition(
      ProjectileDefinitionId id,
      AiBehaviorSelectionComponent behavior,
      ProjectileKinematicsStateComponent kinematics,
      ProjectileLifetimeStateComponent lifetime,
      AttributeSetComponent attributes,
      ProjectileCollisionPolicyComponent collisionPolicy,
      EntityCollisionCapabilityComponent collision,
      CombatAttackDefinitionId attackDefinitionId)
  {
    if (!id.IsValid)
    {
      throw new ArgumentException("A projectile definition id is required.", nameof(id));
    }

    Id = id;
    Behavior = behavior;
    Kinematics = kinematics;
    Lifetime = lifetime;
    Attributes = attributes ?? throw new ArgumentNullException(nameof(attributes));
    CollisionPolicy = collisionPolicy;
    Collision = collision;
    if (!attackDefinitionId.IsValid)
    {
      throw new ArgumentException(
          "A valid combat attack definition id is required.", nameof(attackDefinitionId));
    }

    AttackDefinitionId = attackDefinitionId;
  }

  public ProjectileDefinitionId Id { get; }

  public AiBehaviorSelectionComponent Behavior { get; }

  public ProjectileKinematicsStateComponent Kinematics { get; }

  public ProjectileLifetimeStateComponent Lifetime { get; }

  public AttributeSetComponent Attributes { get; }

  public ProjectileCollisionPolicyComponent CollisionPolicy { get; }

  public EntityCollisionCapabilityComponent Collision { get; }

  public CombatAttackDefinitionId AttackDefinitionId { get; }
}
