using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct LegacyPyramidDualDungeonDecision(
  bool IsRejected,
  int PlacementY,
  int PyramidMaxDepth);

public static class LegacyPyramidDualDungeonPolicy
{
  private const int PotentialBoundsFluff = 5;
  private const int PlacementYOffset = 50;
  private const int AdjustedPyramidMaxDepth = 100;

  public static LegacyPyramidDualDungeonDecision Evaluate(
    int x,
    int placementY,
    int pyramidMaxDepth,
    bool dualDungeonsEnabled,
    IReadOnlyList<DungeonBoundsSnapshot>? potentialOuterBounds)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pyramidMaxDepth);
    if (!dualDungeonsEnabled)
    {
      return new LegacyPyramidDualDungeonDecision(false, placementY, pyramidMaxDepth);
    }

    if (potentialOuterBounds is null || potentialOuterBounds.Count == 0)
    {
      return new LegacyPyramidDualDungeonDecision(true, placementY, pyramidMaxDepth);
    }

    int adjustedPlacementY = placementY;
    int adjustedPyramidMaxDepth = pyramidMaxDepth;
    if (ContainsAny(
          x,
          adjustedPlacementY + adjustedPyramidMaxDepth,
          potentialOuterBounds))
    {
      adjustedPlacementY -= PlacementYOffset;
      adjustedPyramidMaxDepth = AdjustedPyramidMaxDepth;
      if (ContainsAny(
            x,
            adjustedPlacementY + adjustedPyramidMaxDepth,
            potentialOuterBounds))
      {
        return new LegacyPyramidDualDungeonDecision(
          true,
          adjustedPlacementY,
          adjustedPyramidMaxDepth);
      }
    }

    return new LegacyPyramidDualDungeonDecision(
      false,
      adjustedPlacementY,
      adjustedPyramidMaxDepth);
  }

  private static bool ContainsAny(
    int x,
    int y,
    IReadOnlyList<DungeonBoundsSnapshot> potentialOuterBounds)
  {
    for (int index = 0; index < potentialOuterBounds.Count; index++)
    {
      if (potentialOuterBounds[index].ContainsWithFluff(x, y, PotentialBoundsFluff))
      {
        return true;
      }
    }

    return false;
  }
}
