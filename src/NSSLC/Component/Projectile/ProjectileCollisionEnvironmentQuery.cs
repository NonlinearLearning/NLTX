using System;
using System.Numerics;

using Terraria.SpatialSimulation;

namespace Terraria.Projectile;

/// <summary>
/// Computes the tile-dependent collision inputs from one explicit spatial snapshot.
/// </summary>
public static class ProjectileCollisionEnvironmentQuery
{
  public static ProjectileCollisionEnvironmentSnapshot Evaluate(
    in ProjectileCollisionGeometryInput projectile,
    in ProjectileDamageHitbox projectileHitbox,
    in ProjectileCollisionTargetRectangle targetHitbox,
    SpatialCollisionSnapshot collisionSnapshot,
    int maxTilesX,
    int maxTilesY)
  {
    ArgumentNullException.ThrowIfNull(collisionSnapshot);

    int projectileType = projectile.Definition.ProjectileType;
    bool needsCanHit = projectileType is 85 or 973 or 985 or 1106;
    bool needsCanHitLine = projectileType == 661;
    bool needsAi137Visibility = projectile.Definition.BehaviorKey == 137;
    if (!needsCanHit && !needsCanHitLine && !needsAi137Visibility)
    {
      return default;
    }

    ArgumentOutOfRangeException.ThrowIfLessThan(maxTilesX, 2);
    ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maxTilesY, 41);
    if (!SpatialTileLookupSnapshot.TryCreate(
      collisionSnapshot.Tiles,
      out SpatialTileLookupSnapshot tileLookup))
    {
      return default;
    }

    bool hasCanHitResult = false;
    bool canHit = false;
    if (needsCanHit)
    {
      Vector2 projectileCenter = projectile.Kinematics.Position + new Vector2(
        projectile.Width / 2.0f,
        projectile.Height / 2.0f);
      Vector2 targetCenter = GetTargetCenter(in targetHitbox);
      hasCanHitResult = SpatialCanHitQuery.TryEvaluate(
        projectileCenter,
        0,
        0,
        targetCenter,
        0,
        0,
        tileLookup,
        maxTilesX,
        maxTilesY,
        out canHit);
    }

    bool hasCanHitLineResult = false;
    bool canHitLine = false;
    if (needsCanHitLine)
    {
      Vector2 projectileHitboxCenter = new(
        projectileHitbox.X + projectileHitbox.Width / 2,
        projectileHitbox.Y + projectileHitbox.Height / 2);
      Vector2 targetCenter = GetTargetCenter(in targetHitbox);
      hasCanHitLineResult = SpatialCanHitLineQuery.TryEvaluate(
        projectileHitboxCenter,
        targetCenter,
        tileLookup,
        maxTilesX,
        maxTilesY,
        out canHitLine);
    }

    ProjectileAi137VisibilitySnapshot ai137Visibility = needsAi137Visibility
      ? ProjectileAi137VisibilityQuery.EvaluateWithTileLookup(
        projectile,
        in targetHitbox,
        tileLookup,
        maxTilesX,
        maxTilesY)
      : default;
    return new ProjectileCollisionEnvironmentSnapshot(
      hasCanHitResult,
      canHit,
      hasCanHitLineResult,
      canHitLine,
      ai137Visibility);
  }

  private static Vector2 GetTargetCenter(
    in ProjectileCollisionTargetRectangle targetHitbox)
  {
    return new Vector2(
      targetHitbox.X + targetHitbox.Width / 2,
      targetHitbox.Y + targetHitbox.Height / 2);
  }
}
