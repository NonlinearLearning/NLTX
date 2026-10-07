using System;
using System.Collections.Generic;
using System.Numerics;

using EntityEcs.Components;

namespace Terraria.Projectile;

/// <summary>
/// Detached component values consumed by projectile collision geometry.
/// Trail arrays and whip points are supplied as detached values by the caller.
/// The live projectile root remains the owner of location, velocity and collider
/// components; this input is one immutable algorithm snapshot.
/// </summary>
public readonly record struct ProjectileCollisionGeometryInput
{
  public ProjectileCollisionGeometryInput(
    ProjectileDefinitionComponent definition,
    ProjectileBehaviorStateComponent behavior,
    ProjectileKinematicsStateComponent kinematics,
    ColliderComponent collider,
    float scale,
    ProjectilePenetrationStateComponent penetration,
    ProjectileTrajectoryStateComponent trajectory,
    ProjectileTrailCacheComponent trail,
    DirectionComponent direction)
  {
    if (!float.IsFinite(scale) || scale < 0.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(scale));
    }

    if (!float.IsFinite(collider.Width) || !float.IsFinite(collider.Height) ||
      collider.Width < 0.0f || collider.Height < 0.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(collider));
    }

    Definition = definition;
    Behavior = behavior;
    Kinematics = kinematics;
    Collider = collider;
    Scale = scale;
    Penetration = penetration;
    Trajectory = trajectory;
    Trail = CloneTrail(trail);
    Direction = direction;
  }

  public ProjectileCollisionGeometryInput(
    ProjectileDefinitionComponent definition,
    ProjectileBehaviorStateComponent behavior,
    LocationComponent location,
    VelocityComponent velocity,
    ColliderComponent collider,
    float scale,
    ProjectilePenetrationStateComponent penetration,
    ProjectileTrajectoryStateComponent trajectory,
    ProjectileTrailCacheComponent trail,
    DirectionComponent direction)
    : this(
        definition,
        behavior,
        new ProjectileKinematicsStateComponent(
          new Vector2(location.X, location.Y),
          new Vector2(velocity.X, velocity.Y)),
        collider,
        scale,
        penetration,
        trajectory,
        trail,
        direction)
  {
  }

  public ProjectileDefinitionComponent Definition { get; init; }

  public ProjectileBehaviorStateComponent Behavior { get; init; }

  public ProjectileKinematicsStateComponent Kinematics { get; init; }

  public ColliderComponent Collider { get; init; }

  public float Scale { get; init; }

  public int Width => (int)Collider.Width;

  public int Height => (int)Collider.Height;

  public ProjectilePenetrationStateComponent Penetration { get; init; }

  public ProjectileTrajectoryStateComponent Trajectory { get; init; }

  public ProjectileTrailCacheComponent Trail { get; init; }

  public DirectionComponent Direction { get; init; }

  private static ProjectileTrailCacheComponent CloneTrail(
    ProjectileTrailCacheComponent source)
  {
    return new ProjectileTrailCacheComponent
    {
      OldPositions = source.OldPositions is null
        ? Array.Empty<Vector2>()
        : (Vector2[])source.OldPositions.Clone(),
      OldRotations = source.OldRotations is null
        ? Array.Empty<float>()
        : (float[])source.OldRotations.Clone(),
      OldSpriteDirections = source.OldSpriteDirections is null
        ? Array.Empty<int>()
        : (int[])source.OldSpriteDirections.Clone(),
      WhipPoints = source.WhipPoints is null
        ? new List<Vector2>()
        : new List<Vector2>(source.WhipPoints),
    };
  }

}
