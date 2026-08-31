using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyPyramidCandidatePolicy
{
  private const int WorldEdgePadding = 300;
  private const double DungeonSideExclusionFraction = 0.15;
  private const int PyramidSpacing = 220;

  public static bool IsEligible(
    int x,
    int y,
    int worldWidth,
    LegacyDungeonSide dungeonSide,
    int generatingDungeonPositionX,
    IReadOnlyList<int> existingPyramidXs,
    bool drunkWorldGen,
    bool tenthAnniversaryWorldGen,
    bool dualDungeonsEnabled,
    DungeonBoundsSnapshot? undergroundDesertBounds)
  {
    ArgumentNullException.ThrowIfNull(existingPyramidXs);
    if (worldWidth <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldWidth));
    }

    if (x < 0 || x >= worldWidth)
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    if (y < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(y));
    }

    if (!Enum.IsDefined(dungeonSide))
    {
      throw new ArgumentOutOfRangeException(nameof(dungeonSide));
    }

    if (x <= WorldEdgePadding || x >= worldWidth - WorldEdgePadding)
    {
      return false;
    }

    double sideExclusion = worldWidth * DungeonSideExclusionFraction;
    if (dungeonSide == LegacyDungeonSide.Left &&
        x < generatingDungeonPositionX + sideExclusion)
    {
      return false;
    }

    if (dungeonSide == LegacyDungeonSide.Right &&
        x > generatingDungeonPositionX - sideExclusion)
    {
      return false;
    }

    if (tenthAnniversaryWorldGen && !dualDungeonsEnabled)
    {
      if (!undergroundDesertBounds.HasValue || undergroundDesertBounds.Value.Contains(x, y))
      {
        return false;
      }
    }

    int minimumSpacing = drunkWorldGen ? PyramidSpacing / 2 : PyramidSpacing;
    for (int index = 0; index < existingPyramidXs.Count; index++)
    {
      long horizontalDistance = Math.Abs((long)x - existingPyramidXs[index]);
      if (horizontalDistance < minimumSpacing)
      {
        return false;
      }
    }

    return true;
  }
}
