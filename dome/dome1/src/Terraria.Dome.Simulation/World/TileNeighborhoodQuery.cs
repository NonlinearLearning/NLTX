using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldModel;

public static class TileNeighborhoodQuery
{
  private const int SparseTileType = 235;
  private const int SparseTileXStride = 3;

  public static bool AreAnyTilesInSetNearby(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    IReadOnlyList<bool> tileSet,
    int distance)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileSet);
    ArgumentOutOfRangeException.ThrowIfNegative(distance);

    return AnyMatchingTileNearby(
      snapshot,
      x,
      y,
      distance,
      xStride: 1,
      tile => tile.Type < tileSet.Count && tileSet[tile.Type]);
  }

  public static bool IsTileNearby(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    int tileType,
    int distance)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentOutOfRangeException.ThrowIfNegative(tileType);
    ArgumentOutOfRangeException.ThrowIfNegative(distance);

    int xStride = tileType == SparseTileType ? SparseTileXStride : 1;
    return AnyMatchingTileNearby(
      snapshot,
      x,
      y,
      distance,
      xStride,
      tile => tile.Type == tileType);
  }

  public static int CountNearbyTileTypes(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    int radius,
    IReadOnlyList<int> tileTypes,
    int cap = 0)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileTypes);
    ArgumentOutOfRangeException.ThrowIfNegative(radius);
    if (tileTypes.Count == 0)
    {
      return 0;
    }

    int minimumX = (int)Math.Max(0, (long)x - radius);
    int maximumX = (int)Math.Min(snapshot.Metadata.Width - 1L, (long)x + radius);
    int minimumY = (int)Math.Max(0, (long)y - radius);
    int maximumY = (int)Math.Min(snapshot.Metadata.Height - 1L, (long)y + radius);
    int count = 0;
    for (int candidateX = minimumX; candidateX <= maximumX; candidateX++)
    {
      for (int candidateY = minimumY; candidateY <= maximumY; candidateY++)
      {
        WorldTile tile = snapshot.GetTile(candidateX, candidateY);
        if (!tile.IsActive)
        {
          continue;
        }

        for (int index = 0; index < tileTypes.Count; index++)
        {
          if (tileTypes[index] != tile.Type)
          {
            continue;
          }

          count++;
          if (cap > 0 && count >= cap)
          {
            return count;
          }

          break;
        }
      }
    }

    return count;
  }

  private static bool AnyMatchingTileNearby(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    int distance,
    int xStride,
    Func<WorldTile, bool> isMatch)
  {
    long minimumX = (long)x - distance;
    long maximumX = (long)x + distance;
    long minimumY = (long)y - distance;
    long maximumY = (long)y + distance;
    for (long candidateX = minimumX; candidateX <= maximumX; candidateX += xStride)
    {
      for (long candidateY = minimumY; candidateY <= maximumY; candidateY++)
      {
        if (candidateX < 0 || candidateX >= snapshot.Metadata.Width ||
            candidateY < 0 || candidateY >= snapshot.Metadata.Height)
        {
          continue;
        }

        WorldTile tile = snapshot.GetTile((int)candidateX, (int)candidateY);
        if (tile.IsActive && isMatch(tile))
        {
          return true;
        }
      }
    }

    return false;
  }
}
