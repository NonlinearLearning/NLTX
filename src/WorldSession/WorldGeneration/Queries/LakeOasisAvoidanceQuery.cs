using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Combines immutable lake, cave, and tunnel histories for lake eligibility checks.
/// </summary>
public static class LakeOasisAvoidanceQuery
{
  public static bool IsLakeCandidateBlocked(
    LakePlacementHistorySnapshot lakeSnapshot,
    MountainCaveHistorySnapshot caveSnapshot,
    SurfaceTunnelHistorySnapshot tunnelSnapshot,
    int candidateX,
    int lakeSpacing,
    int caveAndTunnelSpacing)
  {
    if (lakeSnapshot.GenerationId != caveSnapshot.GenerationId ||
        lakeSnapshot.GenerationId != tunnelSnapshot.GenerationId)
    {
      throw new ArgumentException(
        "Lake, cave, and tunnel histories must belong to the same generation.");
    }

    return HasValueWithinDistance(
      lakeSnapshot.LakeX,
      lakeSnapshot.Count,
      candidateX,
      lakeSpacing) ||
      HasValueWithinDistance(
        caveSnapshot.XOrigins,
        caveSnapshot.Count,
        candidateX,
        caveAndTunnelSpacing) ||
      HasValueWithinDistance(
        tunnelSnapshot.CenterX,
        tunnelSnapshot.Count,
        candidateX,
        caveAndTunnelSpacing);
  }

  private static bool HasValueWithinDistance(
    System.Collections.Generic.IReadOnlyList<int> values,
    int count,
    int candidate,
    int distance)
  {
    if (distance <= 0 || count <= 0)
    {
      return false;
    }

    int inspectedCount = Math.Min(count, values.Count);
    for (int index = 0; index < inspectedCount; index++)
    {
      long difference = (long)candidate - values[index];
      if (difference > -distance && difference < distance)
      {
        return true;
      }
    }

    return false;
  }
}
