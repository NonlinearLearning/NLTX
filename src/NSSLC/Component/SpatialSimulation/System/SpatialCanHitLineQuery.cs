using System;
using System.Numerics;

namespace Terraria.SpatialSimulation;

/// <summary>
/// Evaluates a tile-based line-of-sight query against an explicit tile snapshot.
/// </summary>
public static class SpatialCanHitLineQuery
{
  /// <summary>
  /// Returns false when a tile or tile fact required by the query is missing.
  /// </summary>
  public static bool TryEvaluate(
    Vector2 first,
    Vector2 second,
    ISpatialTileLookup tiles,
    int maxTilesX,
    int maxTilesY,
    out bool canHit)
  {
    ArgumentNullException.ThrowIfNull(tiles);
    ArgumentOutOfRangeException.ThrowIfLessThan(maxTilesX, 2);
    ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maxTilesY, 41);

    int x = (int)(first.X / SpatialTileSnapshot.TileSize);
    int y = (int)(first.Y / SpatialTileSnapshot.TileSize);
    int endX = (int)(second.X / SpatialTileSnapshot.TileSize);
    int endY = (int)(second.Y / SpatialTileSnapshot.TileSize);
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

    float xDistance = Math.Abs(x - endX);
    float yDistance = Math.Abs(y - endY);
    if (xDistance == 0.0f && yDistance == 0.0f)
    {
      canHit = true;
      return true;
    }

    float xProgress = 1.0f;
    float yProgress = 1.0f;
    if (xDistance == 0.0f || yDistance == 0.0f)
    {
      if (xDistance == 0.0f)
      {
        xProgress = 0.0f;
      }

      if (yDistance == 0.0f)
      {
        yProgress = 0.0f;
      }
    }
    else if (xDistance > yDistance)
    {
      xProgress = xDistance / yDistance;
    }
    else
    {
      yProgress = yDistance / xDistance;
    }

    float xRemainder = 0.0f;
    float yRemainder = 0.0f;
    int direction = y < endY ? 2 : 1;
    int remainingX = (int)xDistance;
    int remainingY = (int)yDistance;
    int directionX = Math.Sign(endX - x);
    int directionY = Math.Sign(endY - y);
    bool finished = false;
    bool finishAfterCurrentTile = false;

    do
    {
      switch (direction)
      {
        case 2:
        {
          xRemainder += xProgress;
          int stepCount = (int)xRemainder;
          xRemainder -= stepCount;
          for (int step = 0; step < stepCount; step++)
          {
            if (!TryIsBlockedLineStep(
              tiles,
              x,
              y - 1,
              x,
              y + 1,
              x,
              y,
              out bool isBlocked))
            {
              canHit = false;
              return false;
            }

            if (isBlocked)
            {
              canHit = false;
              return true;
            }

            if (remainingX == 0 && remainingY == 0)
            {
              finished = true;
              break;
            }

            x += directionX;
            remainingX--;
            if (remainingX == 0 && remainingY == 0 && stepCount == 1)
            {
              finishAfterCurrentTile = true;
            }
          }

          if (remainingY != 0)
          {
            direction = 1;
          }

          break;
        }
        case 1:
        {
          yRemainder += yProgress;
          int stepCount = (int)yRemainder;
          yRemainder -= stepCount;
          for (int step = 0; step < stepCount; step++)
          {
            if (!TryIsBlockedLineStep(
              tiles,
              x - 1,
              y,
              x + 1,
              y,
              x,
              y,
              out bool isBlocked))
            {
              canHit = false;
              return false;
            }

            if (isBlocked)
            {
              canHit = false;
              return true;
            }

            if (remainingX == 0 && remainingY == 0)
            {
              finished = true;
              break;
            }

            y += directionY;
            remainingY--;
            if (remainingX == 0 && remainingY == 0 && stepCount == 1)
            {
              finishAfterCurrentTile = true;
            }
          }

          if (remainingX != 0)
          {
            direction = 2;
          }

          break;
        }
      }

      if (!TryGetLineTile(tiles, x, y, out SpatialTileSnapshot currentTile))
      {
        canHit = false;
        return false;
      }

      if (!TryIsLineBlockingTile(currentTile, out bool currentTileIsBlocking))
      {
        canHit = false;
        return false;
      }

      if (currentTileIsBlocking)
      {
        canHit = false;
        return true;
      }
    }
    while (!(finished || finishAfterCurrentTile));

    canHit = true;
    return true;
  }

  private static bool TryIsBlockedLineStep(
    ISpatialTileLookup tiles,
    int firstX,
    int firstY,
    int secondX,
    int secondY,
    int thirdX,
    int thirdY,
    out bool isBlocked)
  {
    if (!TryGetLineTile(tiles, firstX, firstY, out SpatialTileSnapshot firstTile) ||
      !TryGetLineTile(tiles, secondX, secondY, out SpatialTileSnapshot secondTile) ||
      !TryGetLineTile(tiles, thirdX, thirdY, out SpatialTileSnapshot thirdTile))
    {
      isBlocked = false;
      return false;
    }

    if (!firstTile.Exists || !secondTile.Exists || !thirdTile.Exists)
    {
      isBlocked = true;
      return true;
    }

    bool firstTileSupported = TryIsLineBlockingTile(firstTile, out bool firstTileIsBlocking);
    bool secondTileSupported = TryIsLineBlockingTile(secondTile, out bool secondTileIsBlocking);
    bool thirdTileSupported = TryIsLineBlockingTile(thirdTile, out bool thirdTileIsBlocking);
    isBlocked = firstTileIsBlocking || secondTileIsBlocking || thirdTileIsBlocking;
    if (isBlocked)
    {
      return true;
    }

    if (!firstTileSupported || !secondTileSupported || !thirdTileSupported)
    {
      return false;
    }

    return true;
  }

  private static bool TryGetLineTile(
    ISpatialTileLookup tiles,
    int x,
    int y,
    out SpatialTileSnapshot tile)
  {
    return tiles.TryGetTile(x, y, out tile);
  }

  private static bool TryIsLineBlockingTile(
    SpatialTileSnapshot tile,
    out bool isBlocking)
  {
    if (!tile.Exists)
    {
      isBlocking = true;
      return true;
    }

    if (!tile.IsActive || !tile.IsSolid || tile.IsSolidTop)
    {
      isBlocking = false;
      return true;
    }

    if (!tile.IsInactive.HasValue)
    {
      isBlocking = false;
      return false;
    }

    isBlocking = !tile.IsInactive.Value;
    return true;
  }
}
