using System;
using System.Collections.Generic;
using System.Numerics;

namespace Terraria.Projectile;

/// <summary>
/// Maps selected branches of Version4 Projectile.Colliding. Unsupported
/// means another earlier branch or an unmapped shape owns this decision.
/// </summary>
public static class ProjectileCollidingGeometryQuery
{
  /// <param name="projectileDirection">
  /// Snapshot of the inherited Version4 Entity.direction field.
  /// </param>
  /// <param name="ownerMountedCenter">
  /// Snapshot of Main.player[owner].MountedCenter for AI style 15.
  /// </param>
  /// <param name="canHitLineFromProjectileHitboxCenterToTargetCenter">
  /// Snapshot of Version4 Collision.CanHitLine(myRect.Center, targetRect.Center).
  /// </param>
  /// <param name="canHitFromProjectileCenterToTargetCenter">
  /// Snapshot of Version4 Collision.CanHit(projectile center, target center).
  /// </param>
  /// <param name="ai137Visibility">
  /// Results of the two Version4 AI_137_CanHit target-point checks.
  /// </param>
  /// <param name="ai19Extension">
  /// Snapshot of AI_019_Spears_GetExtensionHitbox for the projectile owner.
  /// </param>
  public static ProjectileCollidingGeometryResult Evaluate(
    in ProjectileCollisionGeometryInput projectile,
    in ProjectileDamageHitbox projectileHitbox,
    in ProjectileCollisionTargetRectangle targetHitbox,
    int projectileDirection,
    Vector2 ownerMountedCenter,
    bool canHitLineFromProjectileHitboxCenterToTargetCenter,
    bool canHitFromProjectileCenterToTargetCenter,
    ProjectileAi137VisibilitySnapshot ai137Visibility = default,
    ProjectileAi19ExtensionSnapshot ai19Extension = default)
  {
    ProjectileCollisionEnvironmentSnapshot collisionEnvironment = new(
      hasCanHitResult: true,
      canHit: canHitFromProjectileCenterToTargetCenter,
      hasCanHitLineResult: true,
      canHitLine: canHitLineFromProjectileHitboxCenterToTargetCenter,
      ai137Visibility: ai137Visibility);
    return Evaluate(
      in projectile,
      in projectileHitbox,
      in targetHitbox,
      projectileDirection,
      ownerMountedCenter,
      in collisionEnvironment,
      ai19Extension);
  }

  public static ProjectileCollidingGeometryResult Evaluate(
    in ProjectileCollisionGeometryInput projectile,
    in ProjectileDamageHitbox projectileHitbox,
    in ProjectileCollisionTargetRectangle targetHitbox,
    int projectileDirection,
    Vector2 ownerMountedCenter,
    in ProjectileCollisionEnvironmentSnapshot collisionEnvironment,
    ProjectileAi19ExtensionSnapshot ai19Extension = default)
  {

    int projectileType = projectile.Definition.ProjectileType;
    int aiStyle = projectile.Definition.BehaviorKey;
    ProjectileBehaviorStateComponent behavior = projectile.Behavior;

    if (aiStyle == 15)
    {
      if (behavior.Ai0 == 0.0f)
      {
        Vector2 mountedOffset = ClosestPointInRectangle(
          in targetHitbox,
          ownerMountedCenter) - ownerMountedCenter;
        mountedOffset.Y /= 0.8f;
        return mountedOffset.Length() <= 55.0f
          ? ProjectileCollidingGeometryResult.Collision
          : ProjectileCollidingGeometryResult.NoCollision;
      }

      if (IsNestedGeometryType(projectileType))
      {
        return Intersects(in projectileHitbox, in targetHitbox)
          ? ProjectileCollidingGeometryResult.Collision
          : ProjectileCollidingGeometryResult.NoCollision;
      }
    }

    bool isBeforeAiStyleGeometry = projectileType is 85 or 973 or 985 or 1106;
    if (aiStyle == 190 && !isBeforeAiStyleGeometry)
    {
      return EvaluateAiStyle190(projectile, in targetHitbox);
    }

    if (aiStyle != 15)
    {
      if (ProjectileSpecializedDefinitionQuery.IsWhipType(projectileType) &&
        !IsBeforeWhipGeometry(projectileType, in targetHitbox))
      {
        return EvaluateWhipControlPoints(
          projectile,
          in projectileHitbox,
          in targetHitbox);
      }

      if (projectileType == 871)
      {
        return EvaluatePelletStorm(projectile, in targetHitbox);
      }

      if (aiStyle == 137 && IsAfterAiStyle19Or137Geometry(
        projectileType,
        behavior,
        in targetHitbox))
      {
        return EvaluateAiStyle137(
          projectile,
          in projectileHitbox,
          in targetHitbox,
          collisionEnvironment.Ai137Visibility);
      }

      bool hasAi19SpearExtension = IsAi19SpearExtensionType(projectileType);
      if (aiStyle == 19 && hasAi19SpearExtension)
      {
        if (targetHitbox.Intersects(in projectileHitbox))
        {
          return ProjectileCollidingGeometryResult.Collision;
        }

        if (!ai19Extension.HasResults)
        {
          return ProjectileCollidingGeometryResult.Unsupported;
        }

        if (ai19Extension.HasExtensionHitbox)
        {
          ProjectileCollisionTargetRectangle extensionHitbox =
            ai19Extension.ExtensionHitbox;
          if (IntersectsAi19Extension(
            projectile,
            in targetHitbox,
            in extensionHitbox))
          {
            return ProjectileCollidingGeometryResult.Collision;
          }
        }

        return ProjectileCollidingGeometryResult.NoCollision;
      }

      if (aiStyle == 203 && IsAfterAiStyle203Geometry(
        projectileType,
        behavior,
        in targetHitbox))
      {
        return ProjectileCollidingGeometryResult.Unsupported;
      }
    }

    if (projectileType == 872)
    {
      return EvaluateLongTrail(projectile, in targetHitbox);
    }

    if (projectileType is 933 or 1100)
    {
      return EvaluateRotatingTrail(projectile, in targetHitbox);
    }

    if (IsLateCollidingGeometryType(projectileType) &&
      Intersects(in projectileHitbox, in targetHitbox))
    {
      return ProjectileCollidingGeometryResult.Collision;
    }

    Vector2 position = projectile.Kinematics.Position;
    Vector2 center = position + new Vector2(
      projectile.Width / 2.0f,
      projectile.Height / 2.0f);
    Vector2 velocity = projectile.Kinematics.Velocity;
    ProjectileTrajectoryStateComponent trajectory = projectile.Trajectory;
    float scale = projectile.Scale;

    switch (projectileType)
    {
      case 85:
      case 1106:
      {
        if (!Intersects(in projectileHitbox, in targetHitbox))
        {
          return ProjectileCollidingGeometryResult.NoCollision;
        }

        if (!collisionEnvironment.HasCanHitResult)
        {
          return ProjectileCollidingGeometryResult.Unsupported;
        }

        return collisionEnvironment.CanHit
          ? ProjectileCollidingGeometryResult.Collision
          : ProjectileCollidingGeometryResult.NoCollision;
      }

      case 973:
      {
        Vector2 closestPointOffset =
          ClosestPointInRectangle(in targetHitbox, center) - center;
        if (Intersects(in projectileHitbox, in targetHitbox))
        {
          return ProjectileCollidingGeometryResult.Collision;
        }

        if (!(closestPointOffset.Length() < 100.0f * scale))
        {
          return ProjectileCollidingGeometryResult.NoCollision;
        }

        if (!collisionEnvironment.HasCanHitResult)
        {
          return ProjectileCollidingGeometryResult.Unsupported;
        }

        return collisionEnvironment.CanHit
          ? ProjectileCollidingGeometryResult.Collision
          : ProjectileCollidingGeometryResult.NoCollision;
      }

      case 985:
      {
        const float ConeLength = 90.0f;
        const float MaximumAngle = MathF.PI / 4.0f;
        bool targetIsInCone = IntersectsConeFastInaccurate(
          in targetHitbox,
          center,
          ConeLength * scale,
          trajectory.Rotation,
          MaximumAngle);
        if (Intersects(in projectileHitbox, in targetHitbox))
        {
          return ProjectileCollidingGeometryResult.Collision;
        }

        if (!targetIsInCone)
        {
          return ProjectileCollidingGeometryResult.NoCollision;
        }

        if (!collisionEnvironment.HasCanHitResult)
        {
          return ProjectileCollidingGeometryResult.Unsupported;
        }

        return collisionEnvironment.CanHit
          ? ProjectileCollidingGeometryResult.Collision
          : ProjectileCollidingGeometryResult.NoCollision;
      }

      case 461:
        return EvaluateLine(
          in projectileHitbox,
          in targetHitbox,
          center,
          center + velocity * behavior.LocalAi1,
          22.0f * scale);

      case 464:
      {
        if (Intersects(in projectileHitbox, in targetHitbox))
        {
          return ProjectileCollidingGeometryResult.Collision;
        }

        if (behavior.Ai1 == 1.0f)
        {
          return ProjectileCollidingGeometryResult.NoCollision;
        }

        float velocityRotation = (float)Math.Atan2(velocity.Y, velocity.X);
        float orbitProgress = behavior.Ai0 % 45.0f / 45.0f;
        Vector2 spinningPoint = RotateForward(
          new Vector2(0.0f, -720.0f),
          velocityRotation) * orbitProgress;
        for (int index = 0; index < 6; index++)
        {
          float orbitAngle =
            (float)index * (MathF.PI * 2.0f) / 6.0f;
          Vector2 orbitOffset = RotateForward(spinningPoint, orbitAngle);
          ProjectileDamageHitbox orbitHitbox = CenteredHitbox(
            center + orbitOffset,
            30,
            30);
          if (targetHitbox.Intersects(in orbitHitbox))
          {
            return ProjectileCollidingGeometryResult.Collision;
          }
        }

        return ProjectileCollidingGeometryResult.NoCollision;
      }

      case 697:
      case 707:
      {
        float lineLength = projectileType == 697 ? 65.0f : 110.0f;
        float lineAngle = trajectory.Rotation -
          MathF.PI / 4.0f * Math.Sign(velocity.X);
        Vector2 lineDirection = ToRotationVector2(lineAngle);
        return EvaluateLine(
          in projectileHitbox,
          in targetHitbox,
          center - lineDirection * lineLength,
          center + lineDirection * lineLength,
          23.0f * scale);
      }

      case 699:
      {
        float lineRotation = trajectory.Rotation -
          MathF.PI / 4.0f * Math.Sign(velocity.X);
        if (trajectory.SpriteDirection == -1)
        {
          lineRotation += MathF.PI;
        }

        Vector2 direction = ToRotationVector2(lineRotation);
        return EvaluateLine(
          in projectileHitbox,
          in targetHitbox,
          center,
          center + direction * -95.0f,
          23.0f * scale);
      }

      case 642:
        return EvaluateLine(
          in projectileHitbox,
          in targetHitbox,
          center,
          center + velocity * behavior.LocalAi1,
          30.0f * scale);

      case 802:
      case 842:
      case 938:
      case 939:
      case 940:
      case 941:
      case 942:
      case 943:
      case 944:
      case 945:
        return EvaluateLine(
          in projectileHitbox,
          in targetHitbox,
          center,
          center + velocity * 6.0f,
          10.0f * scale);

      case 877:
      case 878:
      case 879:
      {
        float lineAngle = trajectory.Rotation -
          MathF.PI / 4.0f -
          MathF.PI / 2.0f -
          (trajectory.SpriteDirection == 1 ? MathF.PI : MathF.PI / 2.0f);
        ProjectileCollisionTargetRectangle broadPhase = CreateLanceBroadPhase(position);
        return EvaluateBoundedLine(
          in targetHitbox,
          in broadPhase,
          center,
          center + ToRotationVector2(lineAngle) * 95.0f,
          23.0f * scale);
      }

      case 632:
      case 537:
        return EvaluateLine(
          in projectileHitbox,
          in targetHitbox,
          center,
          center + velocity * behavior.LocalAi1,
          22.0f * scale);

      case 455:
        return EvaluateLine(
          in projectileHitbox,
          in targetHitbox,
          center,
          center + velocity * behavior.LocalAi1,
          36.0f * scale);

      case 611:
      {
        Vector2 normalizedVelocity = SafeNormalize(velocity, Vector2.Zero);
        return EvaluateLine(
          in projectileHitbox,
          in targetHitbox,
          center,
          center + velocity + normalizedVelocity * 48.0f,
          16.0f * scale);
      }

      case 623:
      {
        ProjectileDamageHitbox specialHitbox = CenteredHitbox(
          center + new Vector2(projectileDirection * 40.0f, 0.0f),
          80,
          40);
        return (behavior.Ai0 == 2.0f &&
          targetHitbox.Intersects(in specialHitbox)) ||
          targetHitbox.Intersects(in projectileHitbox)
          ? ProjectileCollidingGeometryResult.Collision
          : ProjectileCollidingGeometryResult.NoCollision;
      }

      case 684:
      {
        Vector2 normalizedDirection = SafeNormalize(velocity, Vector2.UnitY);
        Vector2 perpendicular = RotateForward(
          normalizedDirection,
          -1.5707963705062866) * scale;
        return EvaluateLine(
          in projectileHitbox,
          in targetHitbox,
          center - perpendicular * 40.0f,
          center + perpendicular * 40.0f,
          16.0f * scale);
      }

      case 687:
      {
        float endProgress = MathF.Min(1.0f, behavior.Ai0 / 25.0f);
        float startProgress = MathF.Max(0.0f, (behavior.Ai0 - 38.0f) / 40.0f);
        Vector2 beamDirection = ToRotationVector2(trajectory.Rotation);
        return EvaluateLine(
          in projectileHitbox,
          in targetHitbox,
          center + beamDirection * (400.0f * startProgress),
          center + beamDirection * (400.0f * endProgress),
          40.0f * scale);
      }

      case 919:
      case 932:
      {
        ProjectileCollisionTargetRectangle broadPhase = CreateLanceBroadPhase(position);
        Vector2 lineDirection = ToRotationVector2(trajectory.Rotation);
        return EvaluateBoundedLine(
          in targetHitbox,
          in broadPhase,
          center - lineDirection * 40.0f,
          center + lineDirection * 40.0f,
          8.0f);
      }

      case 923:
      {
        Vector2 lineDirection = ToRotationVector2(trajectory.Rotation);
        float beamWidth = scale * 0.7f;
        if (CheckLine(
          in targetHitbox,
          center,
          center + lineDirection * (scale * 510.0f),
          beamWidth * 100.0f) ||
          CheckLine(
            in targetHitbox,
            center,
            center + lineDirection * (scale * 660.0f),
            beamWidth * 60.0f) ||
          CheckLine(
            in targetHitbox,
            center,
            center + lineDirection * (scale * 800.0f),
            beamWidth * 10.0f))
        {
          return ProjectileCollidingGeometryResult.Collision;
        }

        return ProjectileCollidingGeometryResult.NoCollision;
      }

      case 756:
      case 961:
      case 1041:
      {
        if (behavior.Ai0 < 0.0f)
        {
          return ProjectileCollidingGeometryResult.NoCollision;
        }

        Vector2 normalizedVelocity = SafeNormalize(velocity, -Vector2.UnitY);
        Vector2 lineEnd = center + normalizedVelocity * 200.0f * scale;
        return CheckLine(in targetHitbox, center, lineEnd, 22.0f * scale)
          ? ProjectileCollidingGeometryResult.Collision
          : ProjectileCollidingGeometryResult.NoCollision;
      }

      case 466:
      case 580:
      case 686:
      case 711:
        if (Intersects(in projectileHitbox, in targetHitbox))
        {
          return ProjectileCollidingGeometryResult.Collision;
        }

        return EvaluateStoredPositionTrail(
          projectile,
          in projectileHitbox,
          in targetHitbox,
          projectileType == 711);

      case 598:
      case 614:
      case 636:
      case 963:
      {
        ProjectileCollisionTargetRectangle adjustedTarget = targetHitbox;
        if (projectileType == 963 && behavior.Ai0 >= 2.0f)
        {
          adjustedTarget = Inflate(in targetHitbox, 30, 30);
        }
        else if ((projectileType is 598 or 614 or 636) &&
          targetHitbox.Width > 8 && targetHitbox.Height > 8)
        {
          adjustedTarget = Inflate(
            in targetHitbox,
            -targetHitbox.Width / 8,
            -targetHitbox.Height / 8);
        }

        return adjustedTarget.Intersects(in projectileHitbox)
          ? ProjectileCollidingGeometryResult.Collision
          : ProjectileCollidingGeometryResult.NoCollision;
      }

      case 758:
      case 1093:
      {
        bool hasSpecialHitbox = behavior.Ai0 == 2.0f;
        int offset = projectileType == 758 ? 30 : 8;
        int specialWidth = projectileType == 758 ? 50 : 20;
        Vector2 specialCenter = center + new Vector2(
          trajectory.SpriteDirection * offset,
          0.0f);
        ProjectileDamageHitbox specialHitbox = CenteredHitbox(
          specialCenter,
          specialWidth,
          20);
        return (hasSpecialHitbox && targetHitbox.Intersects(in specialHitbox)) ||
          targetHitbox.Intersects(in projectileHitbox)
          ? ProjectileCollidingGeometryResult.Collision
          : ProjectileCollidingGeometryResult.NoCollision;
      }

      case 607:
      {
        ProjectileDamageHitbox movedHitbox = projectileHitbox with
        {
          X = projectileHitbox.X + (int)velocity.X,
          Y = projectileHitbox.Y + (int)velocity.Y,
        };
        return targetHitbox.Intersects(in movedHitbox)
          ? ProjectileCollidingGeometryResult.Collision
          : ProjectileCollidingGeometryResult.NoCollision;
      }

      case 661:
      {
        Vector2 projectileHitboxCenter = new(
          projectileHitbox.X + projectileHitbox.Width / 2,
          projectileHitbox.Y + projectileHitbox.Height / 2);
        Vector2 targetCenter = new(
          targetHitbox.X + targetHitbox.Width / 2,
          targetHitbox.Y + targetHitbox.Height / 2);
        if (Vector2.Distance(projectileHitboxCenter, targetCenter) > 500.0f)
        {
          return ProjectileCollidingGeometryResult.NoCollision;
        }

        if (!collisionEnvironment.HasCanHitLineResult)
        {
          return ProjectileCollidingGeometryResult.Unsupported;
        }

        if (!collisionEnvironment.CanHitLine)
        {
          return ProjectileCollidingGeometryResult.NoCollision;
        }

        return Intersects(in projectileHitbox, in targetHitbox)
          ? ProjectileCollidingGeometryResult.Collision
          : ProjectileCollidingGeometryResult.NoCollision;
      }

      case 927:
      {
        Vector2 normalizedVelocity = SafeNormalize(velocity, Vector2.Zero);
        for (float amount = 0.0f; amount <= 1.0f; amount += 0.05f)
        {
          float progress = 1.0f + 4.0f * Math.Clamp(amount, 0.0f, 1.0f);
          Vector2 displacement =
            normalizedVelocity * projectile.Width * progress * scale;
          ProjectileDamageHitbox sweptHitbox = projectileHitbox with
          {
            X = projectileHitbox.X + (int)displacement.X,
            Y = projectileHitbox.Y + (int)displacement.Y,
          };
          if (targetHitbox.Intersects(in sweptHitbox))
          {
            return ProjectileCollidingGeometryResult.Collision;
          }
        }

        return targetHitbox.Intersects(in projectileHitbox)
          ? ProjectileCollidingGeometryResult.Collision
          : ProjectileCollidingGeometryResult.NoCollision;
      }

      case 974:
      {
        float broadPhaseExpansion = 46.0f * scale;
        int expansion = (int)broadPhaseExpansion;
        ProjectileCollisionTargetRectangle broadPhase = Inflate(
          new ProjectileCollisionTargetRectangle(
            (int)position.X,
            (int)position.Y,
            projectile.Width,
            projectile.Height),
          expansion,
          expansion);
        Vector2 lineDirection = ToRotationVector2(trajectory.Rotation);
        return EvaluateBoundedLine(
          in targetHitbox,
          in broadPhase,
          center - lineDirection * broadPhaseExpansion,
          center + lineDirection * broadPhaseExpansion,
          8.0f * scale);
      }

      default:
      {
        if (aiStyle == 137)
        {
          return EvaluateAiStyle137(
            projectile,
            in projectileHitbox,
            in targetHitbox,
            collisionEnvironment.Ai137Visibility);
        }

        if (aiStyle == 203)
        {
          return ProjectileCollidingGeometryResult.Unsupported;
        }

        return Intersects(in projectileHitbox, in targetHitbox)
          ? ProjectileCollidingGeometryResult.Collision
          : ProjectileCollidingGeometryResult.NoCollision;
      }
    }
  }

  private static bool IsNestedGeometryType(int projectileType)
  {
    return projectileType is
      85 or 598 or 607 or 614 or 623 or 636 or 661 or 756 or 758 or 871 or 872 or 877 or 878 or
      879 or 919 or 923 or 927 or 932 or 933 or 961 or 963 or 973 or 974 or 985 or 1041 or 1093 or
      1100 or 1106;
  }

  private static bool IsAi19SpearExtensionType(int projectileType)
  {
    return projectileType is 46 or 105 or 153;
  }

  private static ProjectileCollidingGeometryResult EvaluatePelletStorm(
    in ProjectileCollisionGeometryInput projectile,
    in ProjectileCollisionTargetRectangle targetHitbox)
  {
    ProjectileBehaviorStateComponent behavior = projectile.Behavior;
    Vector2 center = projectile.Kinematics.Position + new Vector2(
      projectile.Width / 2.0f,
      projectile.Height / 2.0f);

    for (int stormIndex = 0;
      stormIndex < ProjectileStormDefinitionFactory.StormCount;
      stormIndex++)
    {
      ProjectileStormDefinition storm = ProjectileStormDefinitionFactory.Create(
        stormIndex,
        behavior.LocalAi0);
      for (int bulletIndex = 0;
        bulletIndex < storm.BulletsInStorm;
        bulletIndex++)
      {
        if (!storm.IsValid(bulletIndex))
        {
          continue;
        }

        var entityHitbox = storm.GetBulletHitbox(bulletIndex, center);
        var stormHitbox = new ProjectileDamageHitbox(
          entityHitbox.X,
          entityHitbox.Y,
          entityHitbox.Width,
          entityHitbox.Height);
        if (targetHitbox.Intersects(in stormHitbox))
        {
          return ProjectileCollidingGeometryResult.Collision;
        }
      }
    }

    return ProjectileCollidingGeometryResult.NoCollision;
  }

  private static ProjectileCollidingGeometryResult EvaluateAiStyle190(
    in ProjectileCollisionGeometryInput projectile,
    in ProjectileCollisionTargetRectangle targetHitbox)
  {
    ProjectileBehaviorStateComponent behavior = projectile.Behavior;
    ProjectileTrajectoryStateComponent trajectory = projectile.Trajectory;
    Vector2 center = projectile.Kinematics.Position + new Vector2(
      projectile.Width / 2.0f,
      projectile.Height / 2.0f);
    const float MaximumAngle = (float)Math.PI / 4.0f;
    float coneLength = 94.0f * projectile.Scale;
    float coneRotation = trajectory.Rotation +
      (float)Math.PI * 2.0f / 25.0f * behavior.Ai0;

    if (IntersectsConeSlowMoreAccurate(
      in targetHitbox,
      center,
      coneLength,
      coneRotation,
      MaximumAngle))
    {
      return ProjectileCollidingGeometryResult.Collision;
    }

    float secondConeProgress = RemapClamped(
      behavior.LocalAi0,
      behavior.Ai1 * 0.3f,
      behavior.Ai1 * 0.5f,
      1.0f,
      0.0f);
    if (secondConeProgress > 0.0f)
    {
      float secondConeRotation = coneRotation -
        (float)Math.PI / 4.0f * behavior.Ai0 * secondConeProgress;
      if (IntersectsConeSlowMoreAccurate(
        in targetHitbox,
        center,
        coneLength,
        secondConeRotation,
        MaximumAngle))
      {
        return ProjectileCollidingGeometryResult.Collision;
      }
    }

    return ProjectileCollidingGeometryResult.NoCollision;
  }

  private static ProjectileCollidingGeometryResult EvaluateAiStyle137(
    in ProjectileCollisionGeometryInput projectile,
    in ProjectileDamageHitbox projectileHitbox,
    in ProjectileCollisionTargetRectangle targetHitbox,
    ProjectileAi137VisibilitySnapshot visibility)
  {
    if (!targetHitbox.Intersects(in projectileHitbox))
    {
      return ProjectileCollidingGeometryResult.NoCollision;
    }

    Vector2 center = projectile.Kinematics.Position + new Vector2(
      projectile.Width / 2.0f,
      projectile.Height / 2.0f);
    if (DistanceFromRectangleToPoint(in targetHitbox, center) >=
      projectile.Height / 2 - 20)
    {
      return ProjectileCollidingGeometryResult.NoCollision;
    }

    if (!visibility.HasTargetCenterResult)
    {
      return ProjectileCollidingGeometryResult.Unsupported;
    }

    if (visibility.TargetCenterCanHit)
    {
      return ProjectileCollidingGeometryResult.Collision;
    }

    if (!visibility.HasTargetTopCenterResult)
    {
      return ProjectileCollidingGeometryResult.Unsupported;
    }

    return visibility.TargetTopCenterCanHit
      ? ProjectileCollidingGeometryResult.Collision
      : ProjectileCollidingGeometryResult.NoCollision;
  }

  private static bool IntersectsAi19Extension(
    in ProjectileCollisionGeometryInput projectile,
    in ProjectileCollisionTargetRectangle targetHitbox,
    in ProjectileCollisionTargetRectangle extensionHitbox)
  {
    Vector2 projectileCenter = projectile.Kinematics.Position + new Vector2(
      projectile.Width / 2.0f,
      projectile.Height / 2.0f);
    Vector2 extensionCenter = new(
      extensionHitbox.X + extensionHitbox.Width / 2,
      extensionHitbox.Y + extensionHitbox.Height / 2);
    float extensionLength = Vector2.Distance(projectileCenter, extensionCenter);
    float stepLength = MathF.Max(extensionHitbox.Width, extensionHitbox.Height);
    if (stepLength < 12.0f)
    {
      stepLength = 12.0f;
    }

    Vector2 extensionSize = new(
      extensionHitbox.Width,
      extensionHitbox.Height);
    for (float distance = stepLength; distance < extensionLength; distance += stepLength)
    {
      Vector2 center = Vector2.Lerp(
        projectileCenter,
        extensionCenter,
        distance / extensionLength);
      ProjectileDamageHitbox segmentHitbox = CenteredHitbox(
        center,
        (int)extensionSize.X,
        (int)extensionSize.Y);
      if (targetHitbox.Intersects(in segmentHitbox))
      {
        return true;
      }
    }

    return targetHitbox.Intersects(in extensionHitbox);
  }

  private static float DistanceFromRectangleToPoint(
    in ProjectileCollisionTargetRectangle rectangle,
    Vector2 point)
  {
    int right = rectangle.X + rectangle.Width;
    int bottom = rectangle.Y + rectangle.Height;
    if (point.X >= rectangle.X && point.X <= right &&
      point.Y >= rectangle.Y && point.Y <= bottom)
    {
      return 0.0f;
    }

    if (point.X >= rectangle.X && point.X <= right)
    {
      return point.Y < rectangle.Y
        ? rectangle.Y - point.Y
        : point.Y - bottom;
    }

    if (point.Y >= rectangle.Y && point.Y <= bottom)
    {
      return point.X < rectangle.X
        ? rectangle.X - point.X
        : point.X - right;
    }

    if (point.X < rectangle.X)
    {
      Vector2 corner = point.Y < rectangle.Y
        ? new Vector2(rectangle.X, rectangle.Y)
        : new Vector2(rectangle.X, bottom);
      return Vector2.Distance(point, corner);
    }

    Vector2 rightCorner = point.Y < rectangle.Y
      ? new Vector2(right, rectangle.Y)
      : new Vector2(right, bottom);
    return Vector2.Distance(point, rightCorner);
  }

  private static bool IntersectsConeSlowMoreAccurate(
    in ProjectileCollisionTargetRectangle targetHitbox,
    Vector2 coneCenter,
    float coneLength,
    float coneRotation,
    float maximumAngle)
  {
    Vector2 coneEnd = coneCenter + ToRotationVector2(coneRotation) * coneLength;
    Vector2 closestPoint = ClosestPointInRectangle(in targetHitbox, coneEnd);
    if (DoesFitInCone(
      closestPoint,
      coneCenter,
      coneLength,
      coneRotation,
      maximumAngle))
    {
      return true;
    }

    Vector2 topLeft = new(targetHitbox.X, targetHitbox.Y);
    Vector2 topRight = new(targetHitbox.X + targetHitbox.Width, targetHitbox.Y);
    Vector2 bottomLeft = new(targetHitbox.X, targetHitbox.Y + targetHitbox.Height);
    Vector2 bottomRight = new(
      targetHitbox.X + targetHitbox.Width,
      targetHitbox.Y + targetHitbox.Height);
    return DoesFitInCone(topLeft, coneCenter, coneLength, coneRotation, maximumAngle) ||
      DoesFitInCone(topRight, coneCenter, coneLength, coneRotation, maximumAngle) ||
      DoesFitInCone(bottomLeft, coneCenter, coneLength, coneRotation, maximumAngle) ||
      DoesFitInCone(bottomRight, coneCenter, coneLength, coneRotation, maximumAngle);
  }

  private static bool DoesFitInCone(
    Vector2 point,
    Vector2 coneCenter,
    float coneLength,
    float coneRotation,
    float maximumAngle)
  {
    Vector2 offset = point - coneCenter;
    Vector2 rotatedOffset = RotateForward(offset, -coneRotation);
    float angle = (float)Math.Atan2(rotatedOffset.Y, rotatedOffset.X);
    if (angle < -maximumAngle || angle > maximumAngle)
    {
      return false;
    }

    return offset.Length() < coneLength;
  }

  private static float RemapClamped(
    float value,
    float fromMinimum,
    float fromMaximum,
    float toMinimum,
    float toMaximum)
  {
    float amount;
    if (fromMinimum < fromMaximum)
    {
      if (value < fromMinimum)
      {
        amount = 0.0f;
      }
      else if (value > fromMaximum)
      {
        amount = 1.0f;
      }
      else
      {
        amount = (value - fromMinimum) / (fromMaximum - fromMinimum);
      }
    }
    else if (value < fromMaximum)
    {
      amount = 1.0f;
    }
    else if (value > fromMinimum)
    {
      amount = 0.0f;
    }
    else
    {
      amount = (value - fromMinimum) / (fromMaximum - fromMinimum);
    }

    return toMinimum + (toMaximum - toMinimum) * amount;
  }

  private static bool IsBeforeWhipGeometry(
    int projectileType,
    in ProjectileCollisionTargetRectangle targetHitbox)
  {
    if (projectileType == 598)
    {
      return targetHitbox.Width > 8 && targetHitbox.Height > 8;
    }

    return projectileType is
      85 or 623 or 871 or 872 or 877 or 878 or 879 or 919 or 923 or 927 or 932 or 933 or 973 or
      974 or 985 or 1100 or 1106;
  }

  private static bool IsAfterAiStyle19Or137Geometry(
    int projectileType,
    ProjectileBehaviorStateComponent behavior,
    in ProjectileCollisionTargetRectangle targetHitbox)
  {
    if (projectileType is 598 or 614 or 636)
    {
      return targetHitbox.Width <= 8 || targetHitbox.Height <= 8;
    }

    if (projectileType == 963)
    {
      return behavior.Ai0 < 2.0f;
    }

    return (projectileType is 756 or 961 or 1041) ||
      IsLateCollidingGeometryType(projectileType);
  }

  private static bool IsAfterAiStyle203Geometry(
    int projectileType,
    ProjectileBehaviorStateComponent behavior,
    in ProjectileCollisionTargetRectangle targetHitbox)
  {
    if (projectileType is 598 or 614 or 636)
    {
      return targetHitbox.Width <= 8 || targetHitbox.Height <= 8;
    }

    if (projectileType == 963)
    {
      return behavior.Ai0 < 2.0f;
    }

    return IsLateCollidingGeometryType(projectileType);
  }

  private static bool IsLateCollidingGeometryType(int projectileType)
  {
    return projectileType is
      461 or 464 or 455 or 537 or 611 or 632 or 642 or 684 or 687 or 697 or 699 or 707 or 711 or
      802 or 842 or 938 or 939 or 940 or 941 or 942 or 943 or 944 or 945 or 466 or 580 or 686;
  }

  private static bool Intersects(
    in ProjectileDamageHitbox projectileHitbox,
    in ProjectileCollisionTargetRectangle targetHitbox)
  {
    return targetHitbox.Intersects(in projectileHitbox);
  }

  private static ProjectileCollidingGeometryResult EvaluateLongTrail(
    in ProjectileCollisionGeometryInput projectile,
    in ProjectileCollisionTargetRectangle targetHitbox)
  {
    Vector2[]? oldPositions = projectile.Trail.OldPositions;
    if (oldPositions is null || oldPositions.Length < 80)
    {
      return ProjectileCollidingGeometryResult.Unsupported;
    }

    Vector2 position = projectile.Kinematics.Position;
    ProjectileDamageHitbox baseHitbox = new(
      (int)position.X,
      (int)position.Y,
      projectile.Width,
      projectile.Height);

    for (int index = 0; index < 80; index += 2)
    {
      Vector2 oldPosition = oldPositions[index];
      if (oldPosition == Vector2.Zero)
      {
        continue;
      }

      ProjectileDamageHitbox oldHitbox = baseHitbox with
      {
        X = (int)oldPosition.X,
        Y = (int)oldPosition.Y,
      };
      if (Intersects(in oldHitbox, in targetHitbox))
      {
        return ProjectileCollidingGeometryResult.Collision;
      }
    }

    return ProjectileCollidingGeometryResult.NoCollision;
  }

  private static ProjectileCollidingGeometryResult EvaluateRotatingTrail(
    in ProjectileCollisionGeometryInput projectile,
    in ProjectileCollisionTargetRectangle targetHitbox)
  {
    ProjectileTrailCacheComponent trail = projectile.Trail;
    Vector2[]? oldPositions = trail.OldPositions;
    float[]? oldRotations = trail.OldRotations;
    if (oldPositions is null || oldRotations is null)
    {
      return ProjectileCollidingGeometryResult.Unsupported;
    }

    const float HalfTrailLength = 40.0f;
    const int LanceHitboxSize = 300;
    Vector2 size = new(
      projectile.Width,
      projectile.Height);
    float localAi0 = projectile.Behavior.LocalAi0;

    for (int index = 14; index < oldPositions.Length; index += 15)
    {
      if (index >= oldRotations.Length)
      {
        return ProjectileCollidingGeometryResult.Unsupported;
      }

      float trailAge = localAi0 - index;
      if (trailAge < 0.0f || trailAge > 60.0f)
      {
        continue;
      }

      Vector2 trailCenter = oldPositions[index] + size / 2.0f;
      Vector2 trailDirection = ToRotationVector2(
        oldRotations[index] + MathF.PI / 2.0f);
      ProjectileCollisionTargetRectangle broadPhase = new(
        (int)trailCenter.X - LanceHitboxSize / 2,
        (int)trailCenter.Y - LanceHitboxSize / 2,
        LanceHitboxSize,
        LanceHitboxSize);
      if (broadPhase.Intersects(in targetHitbox) &&
        CheckLine(
          in targetHitbox,
          trailCenter - trailDirection * HalfTrailLength,
          trailCenter + trailDirection * HalfTrailLength,
          20.0f))
      {
        return ProjectileCollidingGeometryResult.Collision;
      }
    }

    Vector2 position = projectile.Kinematics.Position;
    Vector2 center = position + size / 2.0f;
    Vector2 currentDirection = ToRotationVector2(
      projectile.Trajectory.Rotation + MathF.PI / 2.0f);
    ProjectileCollisionTargetRectangle currentBroadPhase = new(
      (int)position.X - LanceHitboxSize / 2,
      (int)position.Y - LanceHitboxSize / 2,
      LanceHitboxSize,
      LanceHitboxSize);
    return currentBroadPhase.Intersects(in targetHitbox) &&
      CheckLine(
        in targetHitbox,
        center - currentDirection * HalfTrailLength,
        center + currentDirection * HalfTrailLength,
        20.0f)
      ? ProjectileCollidingGeometryResult.Collision
      : ProjectileCollidingGeometryResult.NoCollision;
  }

  private static ProjectileCollidingGeometryResult EvaluateStoredPositionTrail(
    in ProjectileCollisionGeometryInput projectile,
    in ProjectileDamageHitbox projectileHitbox,
    in ProjectileCollisionTargetRectangle targetHitbox,
    bool isPenetratingTrail)
  {
    if (isPenetratingTrail && projectile.Penetration.RemainingHits == -1)
    {
      return ProjectileCollidingGeometryResult.NoCollision;
    }

    Vector2[]? oldPositions = projectile.Trail.OldPositions;
    if (oldPositions is null)
    {
      return ProjectileCollidingGeometryResult.Unsupported;
    }

    for (int index = 0; index < oldPositions.Length; index++)
    {
      Vector2 oldPosition = oldPositions[index];
      if (oldPosition == Vector2.Zero)
      {
        break;
      }

      ProjectileDamageHitbox oldHitbox = projectileHitbox with
      {
        X = (int)oldPosition.X,
        Y = (int)oldPosition.Y,
      };
      if (Intersects(in oldHitbox, in targetHitbox))
      {
        return ProjectileCollidingGeometryResult.Collision;
      }
    }

    return ProjectileCollidingGeometryResult.NoCollision;
  }

  private static ProjectileCollidingGeometryResult EvaluateWhipControlPoints(
    in ProjectileCollisionGeometryInput projectile,
    in ProjectileDamageHitbox projectileHitbox,
    in ProjectileCollisionTargetRectangle targetHitbox)
  {
    List<Vector2>? controlPoints = projectile.Trail.WhipPoints;
    if (controlPoints is null || controlPoints.Count == 0)
    {
      return ProjectileCollidingGeometryResult.Unsupported;
    }

    for (int index = 0; index < controlPoints.Count; index++)
    {
      Vector2 controlPoint = controlPoints[index];
      ProjectileDamageHitbox pointHitbox = projectileHitbox with
      {
        X = (int)controlPoint.X - projectileHitbox.Width / 2,
        Y = (int)controlPoint.Y - projectileHitbox.Height / 2,
      };
      if (targetHitbox.Intersects(in pointHitbox))
      {
        return ProjectileCollidingGeometryResult.Collision;
      }
    }

    return ProjectileCollidingGeometryResult.NoCollision;
  }

  private static ProjectileDamageHitbox CenteredHitbox(
    Vector2 center,
    int width,
    int height)
  {
    return new ProjectileDamageHitbox(
      (int)(center.X - width / 2.0f),
      (int)(center.Y - height / 2.0f),
      width,
      height);
  }

  internal static ProjectileCollisionTargetRectangle Inflate(
    in ProjectileCollisionTargetRectangle rectangle,
    int horizontal,
    int vertical)
  {
    return new ProjectileCollisionTargetRectangle(
      rectangle.X - horizontal,
      rectangle.Y - vertical,
      rectangle.Width + horizontal * 2,
      rectangle.Height + vertical * 2);
  }

  private static Vector2 ClosestPointInRectangle(
    in ProjectileCollisionTargetRectangle rectangle,
    Vector2 point)
  {
    Vector2 closestPoint = point;
    if (closestPoint.X < rectangle.X)
    {
      closestPoint.X = rectangle.X;
    }

    int right = rectangle.X + rectangle.Width;
    if (closestPoint.X > right)
    {
      closestPoint.X = right;
    }

    if (closestPoint.Y < rectangle.Y)
    {
      closestPoint.Y = rectangle.Y;
    }

    int bottom = rectangle.Y + rectangle.Height;
    if (closestPoint.Y > bottom)
    {
      closestPoint.Y = bottom;
    }

    return closestPoint;
  }

  private static bool IntersectsConeFastInaccurate(
    in ProjectileCollisionTargetRectangle targetHitbox,
    Vector2 coneCenter,
    float coneLength,
    float coneRotation,
    float maximumAngle)
  {
    Vector2 coneEnd = coneCenter + ToRotationVector2(coneRotation) * coneLength;
    Vector2 offset = ClosestPointInRectangle(in targetHitbox, coneEnd) - coneCenter;
    Vector2 rotatedOffset = RotateForward(offset, -coneRotation);
    float angle = (float)Math.Atan2(rotatedOffset.Y, rotatedOffset.X);
    if (angle < -maximumAngle || angle > maximumAngle)
    {
      return false;
    }

    return offset.Length() < coneLength;
  }

  private static ProjectileCollidingGeometryResult EvaluateLine(
    in ProjectileDamageHitbox projectileHitbox,
    in ProjectileCollisionTargetRectangle targetHitbox,
    Vector2 lineStart,
    Vector2 lineEnd,
    float lineWidth)
  {
    if (targetHitbox.Intersects(in projectileHitbox) ||
      CheckLine(in targetHitbox, lineStart, lineEnd, lineWidth))
    {
      return ProjectileCollidingGeometryResult.Collision;
    }

    return ProjectileCollidingGeometryResult.NoCollision;
  }

  private static ProjectileCollidingGeometryResult EvaluateBoundedLine(
    in ProjectileCollisionTargetRectangle targetHitbox,
    in ProjectileCollisionTargetRectangle broadPhase,
    Vector2 lineStart,
    Vector2 lineEnd,
    float lineWidth)
  {
    return broadPhase.Intersects(in targetHitbox) &&
      CheckLine(in targetHitbox, lineStart, lineEnd, lineWidth)
      ? ProjectileCollidingGeometryResult.Collision
      : ProjectileCollidingGeometryResult.NoCollision;
  }

  private static ProjectileCollisionTargetRectangle CreateLanceBroadPhase(
    Vector2 position)
  {
    const int HitboxSize = 300;
    return new ProjectileCollisionTargetRectangle(
      (int)position.X - HitboxSize / 2,
      (int)position.Y - HitboxSize / 2,
      HitboxSize,
      HitboxSize);
  }

  private static bool CheckLine(
    in ProjectileCollisionTargetRectangle target,
    Vector2 lineStart,
    Vector2 lineEnd,
    float lineWidth)
  {
    float halfWidth = lineWidth * 0.5f;
    Vector2 lineDimensions = lineEnd - lineStart;
    Vector2 boundsPosition = lineStart;
    Vector2 boundsDimensions = lineDimensions;

    if (boundsDimensions.X > 0.0f)
    {
      boundsDimensions.X += lineWidth;
      boundsPosition.X -= halfWidth;
    }
    else
    {
      boundsPosition.X += boundsDimensions.X - halfWidth;
      boundsDimensions.X = -boundsDimensions.X + lineWidth;
    }

    if (boundsDimensions.Y > 0.0f)
    {
      boundsDimensions.Y += lineWidth;
      boundsPosition.Y -= halfWidth;
    }
    else
    {
      boundsPosition.Y += boundsDimensions.Y - halfWidth;
      boundsDimensions.Y = -boundsDimensions.Y + lineWidth;
    }

    Vector2 targetPosition = new(target.X, target.Y);
    Vector2 targetDimensions = new(target.Width, target.Height);
    if (!CheckAabb(targetPosition, targetDimensions, boundsPosition, boundsDimensions))
    {
      return false;
    }

    float length = lineDimensions.Length();
    float angle = (float)Math.Atan2(lineDimensions.Y, lineDimensions.X);
    Vector2 topLeft = Rotate(targetPosition - lineStart, angle);
    Vector2 topRight = Rotate(
      new Vector2(target.X + target.Width, target.Y) - lineStart,
      angle);
    Vector2 bottomRight = Rotate(
      targetPosition + targetDimensions - lineStart,
      angle);
    Vector2 bottomLeft = Rotate(
      new Vector2(target.X, target.Y + target.Height) - lineStart,
      angle);

    if (IsBeforeLineEnd(topLeft, halfWidth, length) ||
      IsBeforeLineEnd(topRight, halfWidth, length) ||
      IsBeforeLineEnd(bottomRight, halfWidth, length) ||
      IsBeforeLineEnd(bottomLeft, halfWidth, length))
    {
      return true;
    }

    return EdgesIntersectLineBoundary(
        topLeft,
        topRight,
        bottomRight,
        bottomLeft,
        length,
        halfWidth) ||
      EdgesIntersectLineBoundary(
        topLeft,
        topRight,
        bottomRight,
        bottomLeft,
        length,
        -halfWidth);
  }

  private static bool CheckAabb(
    Vector2 firstPosition,
    Vector2 firstDimensions,
    Vector2 secondPosition,
    Vector2 secondDimensions)
  {
    return firstPosition.X < secondPosition.X + secondDimensions.X &&
      firstPosition.Y < secondPosition.Y + secondDimensions.Y &&
      firstPosition.X + firstDimensions.X > secondPosition.X &&
      firstPosition.Y + firstDimensions.Y > secondPosition.Y;
  }

  private static bool IsBeforeLineEnd(Vector2 point, float halfWidth, float lineLength)
  {
    return MathF.Abs(point.Y) < halfWidth &&
      point.X < lineLength &&
      point.X >= 0.0f;
  }

  private static bool EdgesIntersectLineBoundary(
    Vector2 topLeft,
    Vector2 topRight,
    Vector2 bottomRight,
    Vector2 bottomLeft,
    float lineLength,
    float boundaryY)
  {
    Vector2 boundaryStart = new(0.0f, boundaryY);
    Vector2 boundaryEnd = new(lineLength, boundaryY);
    return SegmentsIntersect(boundaryStart, boundaryEnd, topLeft, topRight) ||
      SegmentsIntersect(boundaryStart, boundaryEnd, topRight, bottomRight) ||
      SegmentsIntersect(boundaryStart, boundaryEnd, bottomRight, bottomLeft) ||
      SegmentsIntersect(boundaryStart, boundaryEnd, bottomLeft, topLeft);
  }

  private static bool SegmentsIntersect(
    Vector2 firstStart,
    Vector2 firstEnd,
    Vector2 secondStart,
    Vector2 secondEnd)
  {
    Vector2 firstDirection = firstEnd - firstStart;
    Vector2 secondDirection = secondEnd - secondStart;
    float denominator = Cross(firstDirection, secondDirection);
    if (denominator == 0.0f)
    {
      return false;
    }

    Vector2 relativeStart = secondStart - firstStart;
    float firstAmount = Cross(relativeStart, secondDirection) / denominator;
    float secondAmount = Cross(relativeStart, firstDirection) / denominator;
    return firstAmount >= 0.0f && firstAmount <= 1.0f &&
      secondAmount >= 0.0f && secondAmount <= 1.0f;
  }

  private static float Cross(Vector2 first, Vector2 second)
  {
    return first.X * second.Y - first.Y * second.X;
  }

  private static Vector2 Rotate(Vector2 vector, float angle)
  {
    float cosine = (float)Math.Cos(angle);
    float sine = (float)Math.Sin(angle);
    return new Vector2(
      vector.X * cosine + vector.Y * sine,
      -vector.X * sine + vector.Y * cosine);
  }

  private static Vector2 RotateForward(Vector2 vector, double angle)
  {
    float cosine = (float)Math.Cos(angle);
    float sine = (float)Math.Sin(angle);
    return new Vector2(
      vector.X * cosine - vector.Y * sine,
      vector.X * sine + vector.Y * cosine);
  }

  private static Vector2 ToRotationVector2(float rotation)
  {
    return new Vector2(
      (float)Math.Cos(rotation),
      (float)Math.Sin(rotation));
  }

  private static Vector2 SafeNormalize(Vector2 vector, Vector2 defaultValue)
  {
    return vector == Vector2.Zero || float.IsNaN(vector.X) || float.IsNaN(vector.Y)
      ? defaultValue
      : Vector2.Normalize(vector);
  }
}
