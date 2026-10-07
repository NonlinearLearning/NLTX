using System;
using System.Numerics;

using Terraria.SpatialSimulation;

namespace Terraria.Projectile;

/// <summary>
/// Computes the explicit tile-visibility inputs used by Version4 AI style 137.
/// </summary>
public static class ProjectileAi137VisibilityQuery
{
  private const float SourceOffsetY = 20.0f;
  private const double CurveAngle = 1.5707963705062866;

  public static ProjectileAi137VisibilitySnapshot Evaluate(
    in ProjectileCollisionGeometryInput projectile,
    in ProjectileCollisionTargetRectangle targetHitbox,
    SpatialCollisionSnapshot collisionSnapshot,
    int maxTilesX,
    int maxTilesY)
  {
    ArgumentNullException.ThrowIfNull(collisionSnapshot);
    ArgumentOutOfRangeException.ThrowIfLessThan(maxTilesX, 2);
    ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maxTilesY, 41);

    if (!SpatialTileLookupSnapshot.TryCreate(
      collisionSnapshot.Tiles,
      out SpatialTileLookupSnapshot tileLookup))
    {
      return default;
    }

    return EvaluateWithTileLookup(
      projectile,
      in targetHitbox,
      tileLookup,
      maxTilesX,
      maxTilesY);
  }

  internal static ProjectileAi137VisibilitySnapshot EvaluateWithTileLookup(
    in ProjectileCollisionGeometryInput projectile,
    in ProjectileCollisionTargetRectangle targetHitbox,
    SpatialTileLookupSnapshot tileLookup,
    int maxTilesX,
    int maxTilesY)
  {
    ArgumentNullException.ThrowIfNull(tileLookup);
    ArgumentOutOfRangeException.ThrowIfLessThan(maxTilesX, 2);
    ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maxTilesY, 41);

    Vector2 projectileTop = projectile.Kinematics.Position +
      Vector2.UnitY * SourceOffsetY;
    if (!float.IsFinite(projectileTop.X) || !float.IsFinite(projectileTop.Y))
    {
      return default;
    }

    Vector2 targetCenter = new(
      targetHitbox.X + targetHitbox.Width / 2,
      targetHitbox.Y + targetHitbox.Height / 2);
    if (!TryCanHit(
      projectileTop,
      targetCenter,
      tileLookup,
      maxTilesX,
      maxTilesY,
      out bool targetCenterCanHit))
    {
      return default;
    }

    if (targetCenterCanHit)
    {
      return new ProjectileAi137VisibilitySnapshot(
        hasTargetCenterResult: true,
        targetCenterCanHit: true,
        hasTargetTopCenterResult: false,
        targetTopCenterCanHit: false);
    }

    Vector2 targetTopCenter = new(
      targetHitbox.X + targetHitbox.Width / 2,
      targetHitbox.Y);
    if (!TryCanHit(
      projectileTop,
      targetTopCenter,
      tileLookup,
      maxTilesX,
      maxTilesY,
      out bool targetTopCenterCanHit))
    {
      return new ProjectileAi137VisibilitySnapshot(
        hasTargetCenterResult: true,
        targetCenterCanHit: false,
        hasTargetTopCenterResult: false,
        targetTopCenterCanHit: false);
    }

    return new ProjectileAi137VisibilitySnapshot(
      hasTargetCenterResult: true,
      targetCenterCanHit: false,
      hasTargetTopCenterResult: true,
      targetTopCenterCanHit: targetTopCenterCanHit);
  }

  private static bool TryCanHit(
    Vector2 source,
    Vector2 target,
    SpatialTileLookupSnapshot tileLookup,
    int maxTilesX,
    int maxTilesY,
    out bool canHit)
  {
    int targetTileX = (int)target.X / SpatialTileSnapshot.TileSize;
    int targetTileY = (int)target.Y / SpatialTileSnapshot.TileSize;
    bool targetTileIsInsideWorld = targetTileX >= 0 &&
      targetTileX < maxTilesX &&
      targetTileY >= 0 &&
      targetTileY < maxTilesY;
    SpatialTileSnapshot targetTile = default;
    if (targetTileIsInsideWorld && !tileLookup.TryGetTile(
      targetTileX,
      targetTileY,
      out targetTile))
    {
      canHit = false;
      return false;
    }

    bool targetIsSolid = false;
    if (targetTileIsInsideWorld && !TryIsSolidTile(targetTile, out targetIsSolid))
    {
      canHit = false;
      return false;
    }

    if (targetTileIsInsideWorld && targetIsSolid)
    {
      canHit = false;
      return true;
    }

    if (!SpatialCanHitLineQuery.TryEvaluate(
      source,
      target,
      tileLookup,
      maxTilesX,
      maxTilesY,
      out canHit))
    {
      return false;
    }

    if (canHit)
    {
      return true;
    }

    Vector2 offset = target - source;
    Vector2 direction = SafeNormalize(offset, Vector2.UnitY);
    Vector2 midpoint = Vector2.Lerp(source, target, 0.5f);
    Vector2 positiveCurvePoint = midpoint + Rotate(direction, CurveAngle) *
      offset.Length() * 0.2f;
    if (!SpatialCanHitLineQuery.TryEvaluate(
      source,
      positiveCurvePoint,
      tileLookup,
      maxTilesX,
      maxTilesY,
      out bool firstPositiveSegmentCanHit))
    {
      canHit = false;
      return false;
    }

    if (firstPositiveSegmentCanHit)
    {
      if (!SpatialCanHitLineQuery.TryEvaluate(
        positiveCurvePoint,
        target,
        tileLookup,
        maxTilesX,
        maxTilesY,
        out bool secondPositiveSegmentCanHit))
      {
        canHit = false;
        return false;
      }

      if (secondPositiveSegmentCanHit)
      {
        canHit = true;
        return true;
      }
    }

    Vector2 negativeCurvePoint = midpoint + Rotate(direction, -CurveAngle) *
      offset.Length() * 0.2f;
    if (!SpatialCanHitLineQuery.TryEvaluate(
      source,
      negativeCurvePoint,
      tileLookup,
      maxTilesX,
      maxTilesY,
      out bool firstNegativeSegmentCanHit))
    {
      canHit = false;
      return false;
    }

    if (!firstNegativeSegmentCanHit)
    {
      canHit = false;
      return true;
    }

    if (!SpatialCanHitLineQuery.TryEvaluate(
      negativeCurvePoint,
      target,
      tileLookup,
      maxTilesX,
      maxTilesY,
      out bool secondNegativeSegmentCanHit))
    {
      canHit = false;
      return false;
    }

    canHit = secondNegativeSegmentCanHit;
    return true;
  }

  private static bool TryIsSolidTile(
    SpatialTileSnapshot tile,
    out bool isSolid)
  {
    if (!tile.Exists)
    {
      isSolid = true;
      return true;
    }

    if (!tile.IsActive ||
      !tile.IsSolid ||
      tile.IsSolidTop ||
      tile.IsHalfBrick ||
      tile.Slope != 0)
    {
      isSolid = false;
      return true;
    }

    if (!tile.IsInactive.HasValue)
    {
      isSolid = false;
      return false;
    }

    isSolid = !tile.IsInactive.Value;
    return true;
  }

  private static Vector2 SafeNormalize(Vector2 value, Vector2 fallback)
  {
    if (value == Vector2.Zero ||
      float.IsNaN(value.X) ||
      float.IsNaN(value.Y))
    {
      return fallback;
    }

    return Vector2.Normalize(value);
  }

  private static Vector2 Rotate(Vector2 value, double radians)
  {
    double cosine = Math.Cos(radians);
    double sine = Math.Sin(radians);
    return new Vector2(
      (float)(value.X * cosine - value.Y * sine),
      (float)(value.X * sine + value.Y * cosine));
  }
}
