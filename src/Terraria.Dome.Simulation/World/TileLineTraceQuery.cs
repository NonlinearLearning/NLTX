using System;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldModel;

public static class TileLineTraceQuery
{
  private const double TileSizePixels = 16.0;

  public static bool IsSafeFromRainPath(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int startX,
    int startY,
    float windSpeedCurrent,
    int distanceTiles = 85,
    double widthPixels = 4.0)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    ArgumentOutOfRangeException.ThrowIfNegative(distanceTiles);
    ArgumentOutOfRangeException.ThrowIfNegative(widthPixels);
    if (!float.IsFinite(windSpeedCurrent))
    {
      throw new ArgumentOutOfRangeException(nameof(windSpeedCurrent));
    }

    double velocityX = windSpeedCurrent * 18.0;
    const double velocityY = 14.0;
    double length = Math.Sqrt(velocityX * velocityX + velocityY * velocityY);
    double directionX = length == 0.0 ? 0.0 : -velocityX / length;
    double directionY = length == 0.0 ? -1.0 : -velocityY / length;
    double startPixelX = startX * TileSizePixels + TileSizePixels / 2.0;
    double startPixelY = startY * TileSizePixels + TileSizePixels / 2.0;
    double endPixelX = startPixelX + directionX * TileSizePixels * distanceTiles;
    double endPixelY = startPixelY + directionY * TileSizePixels * distanceTiles;
    double halfWidth = widthPixels / 2.0;
    int startTileX = ToTile(startPixelX);
    int startTileY = ToTile(startPixelY);
    int endTileX = ToTile(endPixelX);
    int endTileY = ToTile(endPixelY);
    int minimumOffsetX = ToTile(startPixelX - directionY * halfWidth) - startTileX;
    int minimumOffsetY = ToTile(startPixelY + directionX * halfWidth) - startTileY;
    int maximumOffsetX = ToTile(startPixelX + directionY * halfWidth) - startTileX;
    int maximumOffsetY = ToTile(startPixelY - directionX * halfWidth) - startTileY;
    return PlotLine(
      startTileX,
      startTileY,
      endTileX,
      endTileY,
      (x, y) => PlotLine(
        x + minimumOffsetX,
        y + minimumOffsetY,
        x + maximumOffsetX,
        y + maximumOffsetY,
        (candidateX, candidateY) => !IsSolid(snapshot, tileDefinitions, candidateX, candidateY)));
  }

  private static bool IsSolid(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y)
  {
    if (!snapshot.Metadata.IsInside(x, y))
    {
      return false;
    }

    WorldTile tile = snapshot.GetTile(x, y);
    return tile.IsActive && !tile.IsInactive &&
      tileDefinitions.TryGet(tile.Type, out TileDefinition definition) &&
      definition.BlocksLiquid && !definition.IsPlatform;
  }

  private static bool PlotLine(int x0, int y0, int x1, int y1, Func<int, int, bool> plot)
  {
    if (x0 == x1 && y0 == y1)
    {
      return plot(x0, y0);
    }

    bool isSteep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
    if (isSteep)
    {
      (x0, y0) = (y0, x0);
      (x1, y1) = (y1, x1);
    }

    int deltaX = Math.Abs(x1 - x0);
    int deltaY = Math.Abs(y1 - y0);
    int error = deltaX / 2;
    int y = y0;
    int stepX = x0 < x1 ? 1 : -1;
    int stepY = y0 < y1 ? 1 : -1;
    for (int x = x0; x != x1; x += stepX)
    {
      if (!plot(isSteep ? y : x, isSteep ? x : y))
      {
        return false;
      }

      error -= deltaY;
      if (error < 0)
      {
        y += stepY;
        error += deltaX;
      }
    }

    return true;
  }

  private static int ToTile(double coordinate)
  {
    return (int)Math.Floor(coordinate / TileSizePixels);
  }
}
