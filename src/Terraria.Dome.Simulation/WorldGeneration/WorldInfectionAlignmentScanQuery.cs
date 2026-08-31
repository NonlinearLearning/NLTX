using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class WorldInfectionAlignmentScanQuery
{
  private const int OuterWorldBuffer = 40;
  private const int SurfaceWeight = 5;
  private const int CavernWeight = 1;

  public static WorldInfectionAlignmentSnapshot ScanColumn(
    WorldGridSnapshot snapshot,
    int columnX,
    int surfaceY,
    bool? remixWorld = null)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    if (columnX < 0 || columnX >= snapshot.Metadata.Width)
    {
      throw new ArgumentOutOfRangeException(nameof(columnX));
    }

    if (surfaceY < OuterWorldBuffer || surfaceY >= snapshot.Metadata.Height - OuterWorldBuffer)
    {
      throw new ArgumentOutOfRangeException(nameof(surfaceY));
    }

    bool selectedRemixWorld = remixWorld ?? snapshot.Metadata.IsRemixWorld ?? false;
    int[] weightedCounts = new int[TileDefinitionRegistry.Version4TileCount];
    ScanRange(snapshot, columnX, OuterWorldBuffer, surfaceY + 1, SurfaceWeight, weightedCounts);
    ScanRange(
      snapshot,
      columnX,
      surfaceY + 1,
      snapshot.Metadata.Height - OuterWorldBuffer,
      CavernWeight,
      weightedCounts);
    return WorldInfectionAlignmentQuery.Evaluate(
      weightedCounts,
      CalculateSolidDenominator(weightedCounts, selectedRemixWorld),
      selectedRemixWorld);
  }

  private static void ScanRange(
    WorldGridSnapshot snapshot,
    int columnX,
    int startY,
    int endExclusiveY,
    int weight,
    int[] weightedCounts)
  {
    for (int y = startY; y < endExclusiveY; y++)
    {
      WorldTile tile = snapshot.GetTile(columnX, y);
      if (tile.IsActive && tile.Type < weightedCounts.Length)
      {
        weightedCounts[tile.Type] = checked(weightedCounts[tile.Type] + weight);
      }
    }
  }

  private static int CalculateSolidDenominator(int[] weightedCounts, bool remixWorld)
  {
    int totalGood = TileTypeCategoryCountQuery.Evaluate(
      weightedCounts,
      TileScanGroupKind.Hallow,
      remixWorld);
    int totalEvil = TileTypeCategoryCountQuery.Evaluate(
      weightedCounts,
      TileScanGroupKind.Corruption,
      remixWorld);
    int totalBlood = TileTypeCategoryCountQuery.Evaluate(
      weightedCounts,
      TileScanGroupKind.Crimson,
      remixWorld);
    return checked(
      weightedCounts[2] +
      weightedCounts[477] +
      weightedCounts[1] +
      weightedCounts[60] +
      weightedCounts[53] +
      weightedCounts[161] +
      totalGood +
      totalEvil +
      totalBlood);
  }
}
