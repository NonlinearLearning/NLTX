using System;
using System.Numerics;

using EntityEcs.Components;

namespace Terraria.Projectile;

/// <summary>Applies mapped projectile motion and presentation rules.</summary>
public static class ProjectileMotionAndAiSystem
{
  private const float OrdinaryArrowGravityStartAiTick = 15.0f;
  private const float OrdinaryArrowGravityAcceleration = 0.1f;
  private const float MaximumDownwardVelocity = 16.0f;
  private const float OrdinaryArrowRotationOffset = 1.57f;

  public static void AdvanceOrdinaryArrowAi(
    in ProjectileDefinitionComponent definition,
    ref ProjectileBehaviorStateComponent behavior,
    ref ProjectileKinematicsStateComponent kinematics,
    ref ProjectileTrajectoryStateComponent trajectory)
  {
    if (definition.ProjectileType != 1 || definition.BehaviorKey != 1)
    {
      throw new NotSupportedException(
        "Only the ordinary arrow projectile profile is mapped by this AI step.");
    }

    behavior.Ai0 += 1.0f;

    Vector2 velocity = kinematics.Velocity;
    if (behavior.Ai0 >= OrdinaryArrowGravityStartAiTick)
    {
      behavior.Ai0 = OrdinaryArrowGravityStartAiTick;
      velocity.Y += OrdinaryArrowGravityAcceleration;
    }

    trajectory.Rotation = (float)Math.Atan2(velocity.Y, velocity.X) +
      OrdinaryArrowRotationOffset;
    if (velocity.Y > MaximumDownwardVelocity)
    {
      velocity.Y = MaximumDownwardVelocity;
    }

    kinematics.Velocity = velocity;
  }

  public static void AdvanceOrdinaryArrowAi(
    in ProjectileDefinitionComponent definition,
    ref ProjectileBehaviorStateComponent behavior,
    ref LocationComponent location,
    ref VelocityComponent velocity,
    ref ProjectileTrajectoryStateComponent trajectory)
  {
    var kinematics = new ProjectileKinematicsStateComponent(
      new Vector2(location.X, location.Y),
      new Vector2(velocity.X, velocity.Y));
    AdvanceOrdinaryArrowAi(
      in definition,
      ref behavior,
      ref kinematics,
      ref trajectory);
    location = new LocationComponent(kinematics.Position.X, kinematics.Position.Y);
    velocity = new VelocityComponent(kinematics.Velocity.X, kinematics.Velocity.Y);
  }

  public static void CommitOrdinaryArrowStep(
    in ProjectileDefinitionComponent definition,
    ref ProjectileKinematicsStateComponent kinematics,
    ref DirectionComponent direction,
    Vector2 position,
    Vector2 velocity)
  {
    if (definition.ProjectileType != 1 || definition.BehaviorKey != 1)
    {
      throw new NotSupportedException(
        "Only the ordinary arrow projectile profile is mapped by this step.");
    }

    kinematics.Position = position;
    kinematics.Velocity = velocity;
    direction.Horizontal = velocity.X < 0.0f ? -1 : 1;
  }

  public static void CommitOrdinaryArrowStep(
    in ProjectileDefinitionComponent definition,
    ref LocationComponent location,
    ref VelocityComponent currentVelocity,
    ref DirectionComponent direction,
    Vector2 position,
    Vector2 velocity)
  {
    var kinematics = new ProjectileKinematicsStateComponent(
      new Vector2(location.X, location.Y),
      new Vector2(currentVelocity.X, currentVelocity.Y));
    CommitOrdinaryArrowStep(
      in definition,
      ref kinematics,
      ref direction,
      position,
      velocity);
    location = new LocationComponent(kinematics.Position.X, kinematics.Position.Y);
    currentVelocity = new VelocityComponent(
      kinematics.Velocity.X,
      kinematics.Velocity.Y);
  }

  public static bool ApplyMagicQuiverExtraUpdate(
    in ProjectileSourceMetadataComponent source,
    in ProjectileDispositionStateComponent disposition,
    in ProjectileDamagePayloadComponent damage,
    ref ProjectileUpdateCadenceComponent cadence,
    bool ownerHasMagicQuiver)
  {
    if (!ownerHasMagicQuiver ||
        source.IsNpcProjectile ||
        !disposition.Friendly ||
        !damage.IsArrow ||
        cadence.ExtraUpdates >= 1)
    {
      return false;
    }

    cadence.ExtraUpdates = 1;
    return true;
  }

  /// <summary>Moves vertical graphics offset toward zero and clamps it to ±16 pixels.</summary>
  public static void AdvanceGfxOffY(
    ref ProjectileTrajectoryStateComponent trajectory,
    float horizontalVelocity)
  {
    float adjustment = (1.0f + MathF.Abs(horizontalVelocity) / 3.0f) *
      trajectory.StepSpeed;
    float gfxOffY = trajectory.GfxOffY;
    if (gfxOffY > 0.0f)
    {
      gfxOffY -= adjustment;
      if (gfxOffY < 0.0f)
      {
        gfxOffY = 0.0f;
      }
    }
    else if (gfxOffY < 0.0f)
    {
      gfxOffY += adjustment;
      if (gfxOffY > 0.0f)
      {
        gfxOffY = 0.0f;
      }
    }

    trajectory.GfxOffY = Math.Clamp(gfxOffY, -16.0f, 16.0f);
  }
}
