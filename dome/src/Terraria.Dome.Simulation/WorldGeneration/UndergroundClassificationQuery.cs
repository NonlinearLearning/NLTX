using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class UndergroundClassificationQuery
{
  private const int WindowWidth = 120;
  private const int VerticalOffset = 80;
  private const int ScanHeight = 3;
  private const int SolidThresholdNumerator = 4;
  private const int SolidThresholdDenominator = 5;

  public static UndergroundClassificationResult Evaluate(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y,
    int worldSurface,
    bool currentTileHasWall)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    if (y > worldSurface + VerticalOffset)
    {
      return new UndergroundClassificationResult(true, 0, 0, 0, 0, true, false, false);
    }

    if (y < worldSurface / 2)
    {
      return new UndergroundClassificationResult(false, 0, 0, 0, 0, false, true, false);
    }

    int windowStartY = y - VerticalOffset;
    int windowStartX = Math.Max(x - WindowWidth / 2, 0);
    int maximumStartX = Math.Max(snapshot.Metadata.Width - WindowWidth - 1, 0);
    windowStartX = Math.Min(windowStartX, maximumStartX);
    int solidTileCount = 0;
    int scannedTileCount = 0;
    for (int offsetX = 0; offsetX < WindowWidth; offsetX++)
    {
      for (int offsetY = 0; offsetY < ScanHeight; offsetY++)
      {
        int tileX = windowStartX + offsetX;
        int tileY = windowStartY + offsetY;
        if (!snapshot.Metadata.IsInside(tileX, tileY))
        {
          continue;
        }

        scannedTileCount++;
        if (TileStateQuery.IsSolid(snapshot, tileDefinitions, tileX, tileY))
        {
          solidTileCount++;
        }
      }
    }

    bool denseSolid = scannedTileCount > 0 &&
      solidTileCount * SolidThresholdDenominator >=
      scannedTileCount * SolidThresholdNumerator;
    bool underground = denseSolid || currentTileHasWall;
    return new UndergroundClassificationResult(
      underground,
      solidTileCount,
      scannedTileCount,
      windowStartX,
      windowStartY,
      false,
      false,
      currentTileHasWall && !denseSolid);
  }
}
