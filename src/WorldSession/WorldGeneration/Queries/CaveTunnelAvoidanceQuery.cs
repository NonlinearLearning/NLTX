using System.Collections.Generic;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Performs pure spacing checks over copied C09 history snapshots.
/// </summary>
public static class CaveTunnelAvoidanceQuery
{
  public static bool HasMountainCaveWithinDistance(
    MountainCaveHistorySnapshot snapshot,
    int candidateX,
    int distance)
  {
    return HasValueWithinDistance(
      snapshot.XOrigins,
      snapshot.Count,
      candidateX,
      distance);
  }

  public static bool HasSurfaceTunnelWithinDistance(
    SurfaceTunnelHistorySnapshot snapshot,
    int candidateX,
    int distance)
  {
    return HasValueWithinDistance(
      snapshot.CenterX,
      snapshot.Count,
      candidateX,
      distance);
  }

  public static bool HasSurfaceOrePatchWithinDistance(
    SurfaceOrePatchHistorySnapshot snapshot,
    int candidateX,
    int distance)
  {
    return HasValueWithinDistance(
      snapshot.PatchX,
      snapshot.Count,
      candidateX,
      distance);
  }

  private static bool HasValueWithinDistance(
    IReadOnlyList<int> positions,
    int count,
    int candidateX,
    int distance)
  {
    if (distance <= 0 || count <= 0)
    {
      return false;
    }

    int inspectedCount = count < positions.Count ? count : positions.Count;
    for (int index = 0; index < inspectedCount; index++)
    {
      long difference = (long)candidateX - positions[index];
      if (difference > -distance && difference < distance)
      {
        return true;
      }
    }

    return false;
  }
}
