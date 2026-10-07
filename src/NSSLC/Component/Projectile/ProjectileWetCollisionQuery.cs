using System;
using System.Collections.Generic;
using System.Numerics;

using EntityEcs.Components;

using Terraria.SpatialSimulation;

namespace Terraria.Projectile;

/// <summary>
/// Resolves the liquid contact used by projectile movement from an explicit tile snapshot.
/// </summary>
public static class ProjectileWetCollisionQuery
{
  public const int WorldSurfaceBufferTiles = 40;

  public static bool TryEvaluate(
    Vector2 position,
    int width,
    int height,
    IReadOnlyList<SpatialTileSnapshot> tiles,
    int maxTilesX,
    int maxTilesY,
    out bool isWet,
    out bool isLavaWet,
    out bool isHoneyWet,
    out bool isShimmerWet)
  {
    ArgumentNullException.ThrowIfNull(tiles);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
    ArgumentOutOfRangeException.ThrowIfLessThan(maxTilesX, 1);
    ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maxTilesY, WorldSurfaceBufferTiles);

    isWet = false;
    isLavaWet = false;
    isHoneyWet = false;
    isShimmerWet = false;
    if (!float.IsFinite(position.X) ||
        !float.IsFinite(position.Y) ||
        !float.IsFinite(position.X + width) ||
        !float.IsFinite(position.Y + height))
    {
      return false;
    }

    int firstTileX = Math.Clamp(
      (int)(position.X / SpatialTileSnapshot.TileSize) - 1,
      0,
      maxTilesX - 1);
    int endTileX = Math.Clamp(
      (int)((position.X + width) / SpatialTileSnapshot.TileSize) + 2,
      0,
      maxTilesX - 1);
    int firstTileY = Math.Clamp(
      (int)(position.Y / SpatialTileSnapshot.TileSize) - 1,
      0,
      maxTilesY - WorldSurfaceBufferTiles);
    int endTileY = Math.Clamp(
      (int)((position.Y + height) / SpatialTileSnapshot.TileSize) + 2,
      0,
      maxTilesY - WorldSurfaceBufferTiles);

    if (firstTileX >= endTileX || firstTileY >= endTileY)
    {
      return true;
    }

    if (!SpatialTileLookupSnapshot.TryCreate(tiles, out SpatialTileLookupSnapshot tileLookup))
    {
      return false;
    }

    int wetWidth = Math.Min(10, width);
    int wetHeight = Math.Min(height / 2, height);
    Vector2 wetCenter = position + new Vector2(width / 2, height / 2);
    Vector2 wetPosition = wetCenter - new Vector2(wetWidth / 2, wetHeight / 2);
    SpatialGeometrySnapshot wetProbe = new(
      0,
      wetPosition,
      new Vector2(wetWidth, wetHeight));
    SpatialGeometrySnapshot projectileHitbox = new(
      0,
      position,
      new Vector2(width, height));

    for (int x = firstTileX; x < endTileX; x++)
    {
      for (int y = firstTileY; y < endTileY; y++)
      {
        if (!tileLookup.TryGetTile(x, y, out SpatialTileSnapshot tile))
        {
          return false;
        }

        if (!tile.Exists)
        {
          continue;
        }

        if (tile.HasLiquid)
        {
          SpatialGeometrySnapshot liquid = tile.LiquidGeometry(0);
          if (!isLavaWet &&
              tile.LiquidKind == LiquidKind.Lava &&
              SpatialCollisionQuery.CheckAabb(in projectileHitbox, in liquid))
          {
            isLavaWet = true;
          }

          if (!isWet && SpatialCollisionQuery.CheckAabb(in wetProbe, in liquid))
          {
            isWet = true;
            isHoneyWet = tile.LiquidKind == LiquidKind.Honey;
            isShimmerWet = tile.LiquidKind == LiquidKind.Shimmer;
          }

          if (isWet && isLavaWet)
          {
            return true;
          }

          continue;
        }

        if (!tile.IsActive || tile.Slope == 0 || y <= 0)
        {
          continue;
        }

        if (!tileLookup.TryGetTile(x, y - 1, out SpatialTileSnapshot liquidAbove))
        {
          return false;
        }

        if (!liquidAbove.Exists || !liquidAbove.HasLiquid)
        {
          continue;
        }

        SpatialGeometrySnapshot fullTile = new(
          0,
          new Vector2(x * SpatialTileSnapshot.TileSize, y * SpatialTileSnapshot.TileSize),
          new Vector2(SpatialTileSnapshot.TileSize, SpatialTileSnapshot.TileSize));
        if (!isWet && SpatialCollisionQuery.CheckAabb(in wetProbe, in fullTile))
        {
          isWet = true;
          isHoneyWet = liquidAbove.LiquidKind == LiquidKind.Honey;
          isShimmerWet = liquidAbove.LiquidKind == LiquidKind.Shimmer;
        }

        if (isWet && isLavaWet)
        {
          return true;
        }
      }
    }

    return true;
  }
}
