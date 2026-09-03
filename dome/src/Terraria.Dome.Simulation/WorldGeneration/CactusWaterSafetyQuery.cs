using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class CactusWaterSafetyQuery
{
  private const int LiquidScale = byte.MaxValue;

  public static CactusWaterSafetyResult Evaluate(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    WorldGenerationDistanceDefaults distances)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    long liquidUnits = 0;
    int scannedCellCount = 0;
    for (int scanX = x - distances.CactusWaterWidth;
         scanX < x + distances.CactusWaterWidth;
         scanX++)
    {
      for (int scanY = y - distances.CactusWaterHeight;
           scanY < y + distances.CactusWaterHeight;
           scanY++)
      {
        if (!snapshot.Metadata.IsInside(scanX, scanY))
        {
          continue;
        }

        liquidUnits += snapshot.GetTile(scanX, scanY).LiquidAmount;
        scannedCellCount++;
      }
    }

    return new CactusWaterSafetyResult(
      liquidUnits,
      scannedCellCount,
      liquidUnits / LiquidScale > distances.CactusWaterLimit);
  }

  public static bool ShouldBlockGrowth(
    CactusWaterSafetyResult result,
    bool remixWorld,
    int candidateY,
    double worldSurfaceY)
  {
    if (!double.IsFinite(worldSurfaceY))
    {
      throw new ArgumentOutOfRangeException(nameof(worldSurfaceY));
    }

    return result.ExceedsLimit && (!remixWorld || candidateY > worldSurfaceY);
  }
}
