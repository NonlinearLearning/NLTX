namespace Terraria.Projectile;

/// <summary>
/// Detached projectile capability values used by NPC and PVP damage filters.
/// Target-specific immunity is supplied separately by the corresponding gate
/// context so capturing this value does not copy the projectile's immunity arrays.
/// </summary>
public readonly struct ProjectileDamageCandidateInput
{
  public ProjectileDamageCandidateInput(
    ProjectileDefinitionComponent definition,
    ProjectileBehaviorStateComponent behavior,
    ProjectileKinematicsStateComponent kinematics,
    ProjectilePenetrationStateComponent penetration,
    ProjectileAnimationStateComponent animation,
    ProjectileDamagePayloadComponent damage,
    ProjectileHitImmunityPolicyComponent hitImmunityPolicy,
    ProjectileSourceMetadataComponent source,
    ProjectileIdentityComponent identity,
    ProjectileDispositionStateComponent disposition,
    ProjectileCollisionPolicyComponent collision,
    ProjectileTrapCapabilityComponent trap)
  {
    Definition = definition;
    Behavior = behavior;
    Kinematics = kinematics;
    Penetration = penetration;
    Animation = animation;
    Damage = damage;
    HitImmunityPolicy = hitImmunityPolicy;
    Source = source;
    Identity = identity;
    Disposition = disposition;
    Collision = collision;
    Trap = trap;
  }

  public ProjectileDefinitionComponent Definition { get; }

  public ProjectileBehaviorStateComponent Behavior { get; }

  public ProjectileKinematicsStateComponent Kinematics { get; }

  public ProjectilePenetrationStateComponent Penetration { get; }

  public ProjectileAnimationStateComponent Animation { get; }

  public ProjectileDamagePayloadComponent Damage { get; }

  public ProjectileHitImmunityPolicyComponent HitImmunityPolicy { get; }

  public ProjectileSourceMetadataComponent Source { get; }

  public ProjectileIdentityComponent Identity { get; }

  public ProjectileDispositionStateComponent Disposition { get; }

  public ProjectileCollisionPolicyComponent Collision { get; }

  public ProjectileTrapCapabilityComponent Trap { get; }

}
