using System;

namespace Terraria.Projectile;

/// <summary>
/// Maps the state and relationship filters before Version4's projectile/NPC
/// geometry check. It does not perform collision or commit a hit.
/// </summary>
public static class ProjectileNpcDamageCandidateQuery
{
  public static ProjectileNpcDamageCandidateStatus Evaluate(
    in ProjectileDamageCandidateInput projectile,
    in ProjectileNpcTargetSnapshot target,
    in ProjectileNpcDamageGateContext context,
    ProjectileStaticNpcImmunityRegistryComponent staticNpcImmunityRegistry,
    uint gameUpdateCount)
  {
    if (!TryPassPreImmunityChecks(
      in projectile,
      in target,
      in context,
      out ProjectileNpcDamageCandidateStatus rejectedStatus))
    {
      return rejectedStatus;
    }

    ProjectileHitImmunityPolicyComponent immunityPolicy = projectile.HitImmunityPolicy;
    int projectileType = projectile.Definition.ProjectileType;
    bool hasStaticNpcImmunity = context.HasStaticNpcImmunity;
    if (immunityPolicy.UsesStaticNpcImmunityRegistry)
    {
      ArgumentNullException.ThrowIfNull(staticNpcImmunityRegistry);
      hasStaticNpcImmunity = ProjectileStaticNpcImmunitySystem.IsNpcImmune(
        staticNpcImmunityRegistry,
        projectileType,
        target.NpcSlot,
        gameUpdateCount);
    }

    ProjectileNpcDamageGateContext resolvedContext = context with
    {
      HasStaticNpcImmunity = hasStaticNpcImmunity,
    };
    return EvaluateAfterImmunityChecks(
      in projectile,
      in target,
      in resolvedContext);
  }

  public static ProjectileNpcDamageCandidateStatus Evaluate(
    in ProjectileDamageCandidateInput projectile,
    in ProjectileNpcTargetSnapshot target,
    in ProjectileNpcDamageGateContext context)
  {
    if (!TryPassPreImmunityChecks(
      in projectile,
      in target,
      in context,
      out ProjectileNpcDamageCandidateStatus rejectedStatus))
    {
      return rejectedStatus;
    }

    return EvaluateAfterImmunityChecks(in projectile, in target, in context);
  }

  private static ProjectileNpcDamageCandidateStatus EvaluateAfterImmunityChecks(
    in ProjectileDamageCandidateInput projectile,
    in ProjectileNpcTargetSnapshot target,
    in ProjectileNpcDamageGateContext context)
  {
    ProjectileHitImmunityPolicyComponent immunityPolicy =
      projectile.HitImmunityPolicy;
    bool usesLocalNpcImmunity = immunityPolicy.UsesLocalNpcImmunity;
    bool usesStaticNpcImmunity = immunityPolicy.UsesStaticNpcImmunity;
    bool immunityAllowsTarget =
      (!usesLocalNpcImmunity && !usesStaticNpcImmunity) ||
      (usesLocalNpcImmunity && !context.HasLocalNpcImmunity) ||
      (usesStaticNpcImmunity && !context.HasStaticNpcImmunity);
    if (!immunityAllowsTarget)
    {
      return ProjectileNpcDamageCandidateStatus.ProjectileImmunity;
    }

    if (ProjectileDerivedPropertiesQuery.ShouldCareForAttackCooldown(
        immunityPolicy,
        projectile.Source,
        projectile.Trap,
        projectile.Identity) &&
      !context.OwnerMeleeHitCooldownAllowsTarget)
    {
      return ProjectileNpcDamageCandidateStatus.OwnerMeleeHitCooldown;
    }

    if (target.DontTakeDamage && !target.IsZappingJellyfish)
    {
      return ProjectileNpcDamageCandidateStatus.InvulnerableTarget;
    }

    if (target.AiStyle == 112 && target.Ai2 > 1.0f)
    {
      return ProjectileNpcDamageCandidateStatus.NpcAiImmunity;
    }

    int projectileType = projectile.Definition.ProjectileType;
    bool ownerIsPlayer = projectile.Identity.OwnerSlot < 255;
    bool canHitAsFriendlyProjectile = !target.Friendly ||
      projectileType == 318 ||
      (target.Type == 22 && ownerIsPlayer && context.OwnerCanDamageGuide) ||
      (target.Type == 54 && ownerIsPlayer && context.OwnerCanDamageClothier);
    if (ownerIsPlayer && !context.OwnerDamageRulesAllowTarget)
    {
      canHitAsFriendlyProjectile = false;
    }

    bool canHitAsHostileProjectile = projectile.Disposition.Hostile &&
      target.Friendly &&
      !target.DontTakeDamageFromHostiles;
    if ((!projectile.Disposition.Friendly ||
        (!canHitAsFriendlyProjectile && !target.IsZappingJellyfish)) &&
      !canHitAsHostileProjectile)
    {
      return ProjectileNpcDamageCandidateStatus.DamageRelationship;
    }

    bool singleHitImmunityException =
      projectile.Penetration.MaximumHits == 1 &&
      !usesLocalNpcImmunity &&
      !usesStaticNpcImmunity;
    if (target.OwnerImmune && !singleHitImmunityException)
    {
      return ProjectileNpcDamageCandidateStatus.OwnerImmunity;
    }

    if (HasProjectileSpecificTargetImmunity(
      in projectile,
      target))
    {
      return ProjectileNpcDamageCandidateStatus.ProjectileTargetImmunity;
    }

    if (!target.NoTileCollide &&
      projectile.Collision.OwnerHitCheck &&
      !context.OwnerHitCheckAllowsTarget)
    {
      return ProjectileNpcDamageCandidateStatus.OwnerHitCheck;
    }

    return ProjectileNpcDamageCandidateStatus.ReadyForCollisionTest;
  }

  private static bool TryPassPreImmunityChecks(
    in ProjectileDamageCandidateInput projectile,
    in ProjectileNpcTargetSnapshot target,
    in ProjectileNpcDamageGateContext context,
    out ProjectileNpcDamageCandidateStatus rejectedStatus)
  {
    if (!ProjectileDamageGateQuery.CanEnterDamagePath(
      projectile.Definition,
      projectile.Behavior,
      projectile.Penetration,
      projectile.Kinematics,
      projectile.Animation,
      context.IsProjectilePet))
    {
      rejectedStatus = ProjectileNpcDamageCandidateStatus.DamageGateRejected;
      return false;
    }

    if (!context.IsDamageOwnerLocalPlayer)
    {
      rejectedStatus = ProjectileNpcDamageCandidateStatus.NonLocalOwner;
      return false;
    }

    if (projectile.Damage.CurrentDamage <= 0)
    {
      rejectedStatus = ProjectileNpcDamageCandidateStatus.NonPositiveDamage;
      return false;
    }

    if (!target.Active)
    {
      rejectedStatus = ProjectileNpcDamageCandidateStatus.InactiveTarget;
      return false;
    }

    rejectedStatus = default;
    return true;
  }

  private static bool HasProjectileSpecificTargetImmunity(
    in ProjectileDamageCandidateInput projectile,
    in ProjectileNpcTargetSnapshot target)
  {
    int projectileType = projectile.Definition.ProjectileType;
    return (projectileType == 11 && (target.Type is 47 or 57)) ||
      (projectileType == 31 && target.Type == 69) ||
      (target.TrapImmune && projectile.Trap.IsTrap) ||
      (target.Immortal && projectile.Source.IsNpcProjectile);
  }
}
