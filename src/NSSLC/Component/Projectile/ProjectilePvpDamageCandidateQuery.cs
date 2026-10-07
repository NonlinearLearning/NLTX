namespace Terraria.Projectile;

/// <summary>
/// Maps the player-state filters before Version4's projectile/player geometry
/// check. It does not perform collision or commit PVP damage.
/// </summary>
public static class ProjectilePvpDamageCandidateQuery
{
  public static ProjectilePvpDamageCandidateStatus Evaluate(
    in ProjectileDamageCandidateInput projectile,
    in ProjectilePvpTargetSnapshot target,
    in ProjectilePvpDamageGateContext context)
  {
    ProjectilePvpDamageCandidateStatus status = EvaluateBeforeProjectileImmunity(
      in projectile,
      in target,
      in context);
    if (status != ProjectilePvpDamageCandidateStatus.ReadyForCollisionTest)
    {
      return status;
    }

    return EvaluateAfterProjectileImmunity(in projectile, in target, in context);
  }

  private static ProjectilePvpDamageCandidateStatus EvaluateBeforeProjectileImmunity(
    in ProjectileDamageCandidateInput projectile,
    in ProjectilePvpTargetSnapshot target,
    in ProjectilePvpDamageGateContext context)
  {
    if (!ProjectileDamageGateQuery.CanEnterDamagePath(
      projectile.Definition,
      projectile.Behavior,
      projectile.Penetration,
      projectile.Kinematics,
      projectile.Animation,
      context.IsProjectilePet))
    {
      return ProjectilePvpDamageCandidateStatus.DamageGateRejected;
    }

    if (!context.IsDamageOwnerLocalPlayer)
    {
      return ProjectilePvpDamageCandidateStatus.NonLocalOwner;
    }

    if (projectile.Damage.CurrentDamage <= 0)
    {
      return ProjectilePvpDamageCandidateStatus.NonPositiveDamage;
    }

    if (!context.LocalDamageOwnerIsHostile)
    {
      return ProjectilePvpDamageCandidateStatus.LocalDamageOwnerNotHostile;
    }

    int targetPlayerSlot = target.Slot.Value;
    if ((uint)targetPlayerSlot >= 255u)
    {
      return ProjectilePvpDamageCandidateStatus.InvalidTargetSlot;
    }

    if (targetPlayerSlot == projectile.Identity.OwnerSlot)
    {
      return ProjectilePvpDamageCandidateStatus.ProjectileOwnerTarget;
    }

    if (!target.Active)
    {
      return ProjectilePvpDamageCandidateStatus.InactiveTarget;
    }

    if (target.Dead)
    {
      return ProjectilePvpDamageCandidateStatus.DeadTarget;
    }

    if (target.Immune)
    {
      return ProjectilePvpDamageCandidateStatus.ImmuneTarget;
    }

    if (!target.Hostile)
    {
      return ProjectilePvpDamageCandidateStatus.NonHostileTarget;
    }

    return ProjectilePvpDamageCandidateStatus.ReadyForCollisionTest;
  }

  private static ProjectilePvpDamageCandidateStatus EvaluateAfterProjectileImmunity(
    in ProjectileDamageCandidateInput projectile,
    in ProjectilePvpTargetSnapshot target,
    in ProjectilePvpDamageGateContext context)
  {
    if (context.ProjectileIsImmune)
    {
      return ProjectilePvpDamageCandidateStatus.ProjectileImmunity;
    }

    if (context.LocalDamageOwnerTeam != 0 &&
      context.LocalDamageOwnerTeam == target.Team)
    {
      return ProjectilePvpDamageCandidateStatus.SameTeam;
    }

    if (projectile.Collision.OwnerHitCheck &&
      !context.OwnerHitCheckAllowsTarget)
    {
      return ProjectilePvpDamageCandidateStatus.OwnerHitCheck;
    }

    return ProjectilePvpDamageCandidateStatus.ReadyForCollisionTest;
  }
}
