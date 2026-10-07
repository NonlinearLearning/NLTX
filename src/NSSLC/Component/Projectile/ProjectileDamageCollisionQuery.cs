using System;
using System.Numerics;

using Terraria.SpatialSimulation;

namespace Terraria.Projectile;

/// <summary>
/// Composes projectile damage admission with mapped collision geometry.
/// Target and projectile state arrive as detached capability values.
/// </summary>
public static class ProjectileDamageCollisionQuery
{
  public static ProjectileNpcDamageCollisionResult EvaluateNpc(
    in ProjectileDamageCandidateInput projectile,
    in ProjectileCollisionGeometryInput collisionInput,
    in ProjectileNpcTargetSnapshot target,
    in ProjectileNpcDamageGateContext context,
    ProjectileStaticNpcImmunityRegistryComponent staticNpcImmunityRegistry,
    uint gameUpdateCount,
    in ProjectileDamageHitbox projectileHitbox,
    in ProjectileCollisionTargetRectangle targetHitbox,
    int projectileDirection,
    Vector2 ownerMountedCenter,
    bool canHitLineFromProjectileHitboxCenterToTargetCenter,
    bool canHitFromProjectileCenterToTargetCenter,
    ProjectileAi137VisibilitySnapshot ai137Visibility = default,
    ProjectileAi19ExtensionSnapshot ai19Extension = default)
  {
    ProjectileNpcDamageCandidateStatus candidateStatus =
      ProjectileNpcDamageCandidateQuery.Evaluate(
        in projectile,
        in target,
        in context,
        staticNpcImmunityRegistry,
        gameUpdateCount);
    if (candidateStatus != ProjectileNpcDamageCandidateStatus.ReadyForCollisionTest)
    {
      return CreateNpcNonCollisionResult(candidateStatus);
    }

    ProjectileCollisionEnvironmentSnapshot collisionEnvironment = new(
      hasCanHitResult: true,
      canHit: canHitFromProjectileCenterToTargetCenter,
      hasCanHitLineResult: true,
      canHitLine: canHitLineFromProjectileHitboxCenterToTargetCenter,
      ai137Visibility: ai137Visibility);
    return EvaluateNpcGeometry(
      candidateStatus,
      in collisionInput,
      target.Type,
      in projectileHitbox,
      in targetHitbox,
      projectileDirection,
      ownerMountedCenter,
      in collisionEnvironment,
      ai19Extension);
  }

  public static ProjectileNpcDamageCollisionResult EvaluateNpcWithSpatialSnapshot(
    in ProjectileDamageCandidateInput projectile,
    in ProjectileCollisionGeometryInput collisionInput,
    in ProjectileNpcTargetSnapshot target,
    in ProjectileNpcDamageGateContext context,
    ProjectileStaticNpcImmunityRegistryComponent staticNpcImmunityRegistry,
    uint gameUpdateCount,
    in ProjectileDamageHitbox projectileHitbox,
    in ProjectileCollisionTargetRectangle targetHitbox,
    int projectileDirection,
    Vector2 ownerMountedCenter,
    SpatialCollisionSnapshot collisionSnapshot,
    int maxTilesX,
    int maxTilesY,
    ProjectileAi19ExtensionSnapshot ai19Extension = default)
  {
    ProjectileNpcDamageCandidateStatus candidateStatus =
      ProjectileNpcDamageCandidateQuery.Evaluate(
        in projectile,
        in target,
        in context,
        staticNpcImmunityRegistry,
        gameUpdateCount);
    if (candidateStatus != ProjectileNpcDamageCandidateStatus.ReadyForCollisionTest)
    {
      return CreateNpcNonCollisionResult(candidateStatus);
    }

    ProjectileCollisionTargetRectangle effectiveTargetHitbox = target.Type == 414
      ? ProjectileCollidingGeometryQuery.Inflate(in targetHitbox, 8, 8)
      : targetHitbox;
    ProjectileCollisionEnvironmentSnapshot collisionEnvironment =
      ProjectileCollisionEnvironmentQuery.Evaluate(
        in collisionInput,
        in projectileHitbox,
        in effectiveTargetHitbox,
        collisionSnapshot,
        maxTilesX,
        maxTilesY);
    return EvaluateNpcGeometry(
      candidateStatus,
      in collisionInput,
      target.Type,
      in projectileHitbox,
      in targetHitbox,
      projectileDirection,
      ownerMountedCenter,
      in collisionEnvironment,
      ai19Extension);
  }

  public static ProjectilePvpDamageCollisionResult EvaluatePlayer(
    in ProjectileDamageCandidateInput projectile,
    in ProjectileCollisionGeometryInput collisionInput,
    in ProjectilePvpTargetSnapshot target,
    in ProjectilePvpDamageGateContext context,
    in ProjectileDamageHitbox projectileHitbox,
    in ProjectileCollisionTargetRectangle targetHitbox,
    int projectileDirection,
    Vector2 ownerMountedCenter,
    bool canHitLineFromProjectileHitboxCenterToTargetCenter,
    bool canHitFromProjectileCenterToTargetCenter,
    ProjectileAi137VisibilitySnapshot ai137Visibility = default,
    ProjectileAi19ExtensionSnapshot ai19Extension = default)
  {
    ProjectilePvpDamageCandidateStatus candidateStatus =
      ProjectilePvpDamageCandidateQuery.Evaluate(
        in projectile,
        in target,
        in context);
    if (candidateStatus != ProjectilePvpDamageCandidateStatus.ReadyForCollisionTest)
    {
      return CreatePlayerNonCollisionResult(candidateStatus);
    }

    ProjectileCollisionEnvironmentSnapshot collisionEnvironment = new(
      hasCanHitResult: true,
      canHit: canHitFromProjectileCenterToTargetCenter,
      hasCanHitLineResult: true,
      canHitLine: canHitLineFromProjectileHitboxCenterToTargetCenter,
      ai137Visibility: ai137Visibility);
    ProjectileCollidingGeometryResult geometryResult =
      ProjectileCollidingGeometryQuery.Evaluate(
        in collisionInput,
        in projectileHitbox,
        in targetHitbox,
        projectileDirection,
        ownerMountedCenter,
        in collisionEnvironment,
        ai19Extension);
    return new ProjectilePvpDamageCollisionResult(candidateStatus, geometryResult);
  }

  public static ProjectilePvpDamageCollisionResult EvaluatePlayerWithSpatialSnapshot(
    in ProjectileDamageCandidateInput projectile,
    in ProjectileCollisionGeometryInput collisionInput,
    in ProjectilePvpTargetSnapshot target,
    in ProjectilePvpDamageGateContext context,
    in ProjectileDamageHitbox projectileHitbox,
    in ProjectileCollisionTargetRectangle targetHitbox,
    int projectileDirection,
    Vector2 ownerMountedCenter,
    SpatialCollisionSnapshot collisionSnapshot,
    int maxTilesX,
    int maxTilesY,
    ProjectileAi19ExtensionSnapshot ai19Extension = default)
  {
    ProjectilePvpDamageCandidateStatus candidateStatus =
      ProjectilePvpDamageCandidateQuery.Evaluate(
        in projectile,
        in target,
        in context);
    if (candidateStatus != ProjectilePvpDamageCandidateStatus.ReadyForCollisionTest)
    {
      return CreatePlayerNonCollisionResult(candidateStatus);
    }

    ProjectileCollisionEnvironmentSnapshot collisionEnvironment =
      ProjectileCollisionEnvironmentQuery.Evaluate(
        in collisionInput,
        in projectileHitbox,
        in targetHitbox,
        collisionSnapshot,
        maxTilesX,
        maxTilesY);
    ProjectileCollidingGeometryResult geometryResult =
      ProjectileCollidingGeometryQuery.Evaluate(
        in collisionInput,
        in projectileHitbox,
        in targetHitbox,
        projectileDirection,
        ownerMountedCenter,
        in collisionEnvironment,
        ai19Extension);
    return new ProjectilePvpDamageCollisionResult(candidateStatus, geometryResult);
  }

  private static ProjectileNpcDamageCollisionResult EvaluateNpcGeometry(
    ProjectileNpcDamageCandidateStatus candidateStatus,
    in ProjectileCollisionGeometryInput collisionInput,
    int npcType,
    in ProjectileDamageHitbox projectileHitbox,
    in ProjectileCollisionTargetRectangle targetHitbox,
    int projectileDirection,
    Vector2 ownerMountedCenter,
    in ProjectileCollisionEnvironmentSnapshot collisionEnvironment,
    ProjectileAi19ExtensionSnapshot ai19Extension)
  {
    ProjectileCollisionTargetRectangle effectiveTargetHitbox = npcType == 414
      ? ProjectileCollidingGeometryQuery.Inflate(in targetHitbox, 8, 8)
      : targetHitbox;
    ProjectileCollidingGeometryResult geometryResult =
      ProjectileCollidingGeometryQuery.Evaluate(
        in collisionInput,
        in projectileHitbox,
        in effectiveTargetHitbox,
        projectileDirection,
        ownerMountedCenter,
        in collisionEnvironment,
        ai19Extension);
    return new ProjectileNpcDamageCollisionResult(candidateStatus, geometryResult);
  }

  private static ProjectileNpcDamageCollisionResult CreateNpcNonCollisionResult(
    ProjectileNpcDamageCandidateStatus candidateStatus)
  {
    return new ProjectileNpcDamageCollisionResult(
      candidateStatus,
      ProjectileCollidingGeometryResult.NotEvaluated);
  }

  private static ProjectilePvpDamageCollisionResult CreatePlayerNonCollisionResult(
    ProjectilePvpDamageCandidateStatus candidateStatus)
  {
    return new ProjectilePvpDamageCollisionResult(
      candidateStatus,
      ProjectileCollidingGeometryResult.NotEvaluated);
  }
}
