using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class WorldInfectionAlignmentQuery
{
  public static WorldInfectionAlignmentSnapshot Evaluate(
    IReadOnlyList<int> tileTypeCounts,
    int totalSolid,
    bool remixWorld = false)
  {
    ArgumentNullException.ThrowIfNull(tileTypeCounts);
    int totalEvil = TileTypeCategoryCountQuery.Evaluate(
      tileTypeCounts,
      TileScanGroupKind.Corruption,
      remixWorld);
    int totalBlood = TileTypeCategoryCountQuery.Evaluate(
      tileTypeCounts,
      TileScanGroupKind.Crimson,
      remixWorld);
    int totalGood = TileTypeCategoryCountQuery.Evaluate(
      tileTypeCounts,
      TileScanGroupKind.Hallow,
      remixWorld);
    return new WorldInfectionAlignmentSnapshot(totalEvil, totalBlood, totalGood, totalSolid);
  }
}
