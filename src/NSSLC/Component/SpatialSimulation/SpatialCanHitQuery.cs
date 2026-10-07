using System;
using System.Numerics;

namespace Terraria.SpatialSimulation;

/// <summary>
/// Evaluates Version4's tile-based Collision.CanHit rule against an explicit tile snapshot.
/// </summary>
public static class SpatialCanHitQuery
{
  /// <summary>
  /// Returns false when a tile or tile fact required by the query is missing.
  /// </summary>
  public static bool TryEvaluate(
    Vector2 firstPosition,
    int firstWidth,
    int firstHeight,
    Vector2 secondPosition,
    int secondWidth,
    int secondHeight,
    SpatialTileLookupSnapshot tiles,
    int maxTilesX,
    int maxTilesY,
    out bool canHit)
  {
    ArgumentNullException.ThrowIfNull(tiles);
    ArgumentOutOfRangeException.ThrowIfLessThan(maxTilesX, 2);
    ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maxTilesY, 41);

    int x = ((int)firstPosition.X + firstWidth / 2) / SpatialTileSnapshot.TileSize;
    int y = ((int)firstPosition.Y + firstHeight / 2) / SpatialTileSnapshot.TileSize;
    int endX = ((int)secondPosition.X + secondWidth / 2) / SpatialTileSnapshot.TileSize;
    int endY = ((int)secondPosition.Y + secondHeight / 2) / SpatialTileSnapshot.TileSize;

    if (x <= 1)
    {
      x = 1;
    }

    if (x >= maxTilesX)
    {
      x = maxTilesX - 1;
    }

    if (endX <= 1)
    {
      endX = 1;
    }

    if (endX >= maxTilesX)
    {
      endX = maxTilesX - 1;
    }

    if (y <= 1)
    {
      y = 1;
    }

    if (y >= maxTilesY - 40)
    {
      y = maxTilesY - 40;
    }

    if (endY <= 1)
    {
      endY = 1;
    }

    if (endY >= maxTilesY - 40)
    {
      endY = maxTilesY - 40;
    }

    while (true)
    {
      int xDistance = Math.Abs(x - endX);
      int yDistance = Math.Abs(y - endY);
      if (x == endX && y == endY)
      {
        canHit = true;
        return true;
      }

      if (xDistance > yDistance)
      {
        x += x >= endX ? -1 : 1;
        if (!TryGetTile(tiles, x, y - 1, out SpatialTileSnapshot upperTile) ||
          !TryGetTile(tiles, x, y + 1, out SpatialTileSnapshot lowerTile))
        {
          canHit = false;
          return false;
        }

        if (!upperTile.Exists || !lowerTile.Exists)
        {
          canHit = false;
          return true;
        }

        if (!TryAreCanHitBlockers(
          upperTile,
          lowerTile,
          out bool horizontalTilesBlock))
        {
          canHit = false;
          return false;
        }

        if (horizontalTilesBlock)
        {
          canHit = false;
          return true;
        }
      }
      else
      {
        y += y >= endY ? -1 : 1;
        if (!TryGetTile(tiles, x - 1, y, out SpatialTileSnapshot leftTile) ||
          !TryGetTile(tiles, x + 1, y, out SpatialTileSnapshot rightTile))
        {
          canHit = false;
          return false;
        }

        if (!leftTile.Exists || !rightTile.Exists)
        {
          canHit = false;
          return true;
        }

        if (!TryAreCanHitBlockers(
          leftTile,
          rightTile,
          out bool verticalTilesBlock))
        {
          canHit = false;
          return false;
        }

        if (verticalTilesBlock)
        {
          canHit = false;
          return true;
        }
      }

      if (!TryGetTile(tiles, x, y, out SpatialTileSnapshot currentTile))
      {
        canHit = false;
        return false;
      }

      if (!currentTile.Exists)
      {
        canHit = false;
        return true;
      }

      if (!TryShouldContinueThroughTile(currentTile, out bool shouldContinue))
      {
        canHit = false;
        return false;
      }

      if (!shouldContinue)
      {
        canHit = false;
        return true;
      }
    }
  }

  private static bool TryGetTile(
    SpatialTileLookupSnapshot tiles,
    int x,
    int y,
    out SpatialTileSnapshot tile)
  {
    return tiles.TryGetTile(x, y, out tile);
  }

  private static bool TryAreCanHitBlockers(
    SpatialTileSnapshot firstTile,
    SpatialTileSnapshot secondTile,
    out bool bothBlock)
  {
    if (!TryIsCanHitBlocker(firstTile, out bool firstBlocks))
    {
      bothBlock = false;
      return false;
    }

    if (!firstBlocks)
    {
      bothBlock = false;
      return true;
    }

    if (!TryIsCanHitBlocker(secondTile, out bool secondBlocks))
    {
      bothBlock = false;
      return false;
    }

    bothBlock = secondBlocks;
    return true;
  }

  private static bool TryIsCanHitBlocker(
    SpatialTileSnapshot tile,
    out bool blocks)
  {
    if (!tile.IsActive ||
      !tile.IsSolid ||
      tile.IsSolidTop ||
      tile.IsHalfBrick ||
      tile.Slope != 0)
    {
      blocks = false;
      return true;
    }

    if (!tile.IsInactive.HasValue)
    {
      blocks = false;
      return false;
    }

    blocks = !tile.IsInactive.Value;
    return true;
  }

  private static bool TryShouldContinueThroughTile(
    SpatialTileSnapshot tile,
    out bool shouldContinue)
  {
    if (!tile.IsActive || !tile.IsSolid || tile.IsSolidTop)
    {
      shouldContinue = true;
      return true;
    }

    if (!tile.IsInactive.HasValue)
    {
      shouldContinue = false;
      return false;
    }

    shouldContinue = tile.IsInactive.Value;
    return true;
  }
}
