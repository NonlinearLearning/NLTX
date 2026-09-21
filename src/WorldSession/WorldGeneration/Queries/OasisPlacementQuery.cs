using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Performs pure strict-distance checks over copied oasis centers.
/// </summary>
public static class OasisPlacementQuery
{
  public static bool HasCenterWithinDistance(
    OasisPlacementHistorySnapshot snapshot,
    TilePosition candidate,
    int distance)
  {
    if (distance <= 0 || snapshot.Count <= 0)
    {
      return false;
    }

    int inspectedCount = Math.Min(snapshot.Count, snapshot.Centers.Count);
    double distanceSquared = (double)distance * distance;
    for (int index = 0; index < inspectedCount; index++)
    {
      TilePosition existing = snapshot.Centers[index];
      long deltaX = (long)candidate.X - existing.X;
      long deltaY = (long)candidate.Y - existing.Y;
      double squaredDistance = (double)deltaX * deltaX + (double)deltaY * deltaY;
      if (squaredDistance < distanceSquared)
      {
        return true;
      }
    }

    return false;
  }
}
