using System;

namespace Terraria.WorldGeneration.Housing;

public static class CactusWaterEligibilityQuery
{
  private const int LiquidScale = byte.MaxValue;

  public static CactusWaterEligibilityResult Evaluate(
    CactusWaterGridSnapshot snapshot,
    int x,
    int y,
    HousingWaterRuleDefinition rules)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(rules);
    if (!snapshot.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    long liquidUnits = 0;
    int scannedCellCount = 0;
    long startX = (long)x - rules.CactusWaterWidth;
    long endX = (long)x + rules.CactusWaterWidth;
    long startY = (long)y - rules.CactusWaterHeight;
    long endY = (long)y + rules.CactusWaterHeight;
    for (long scanX = startX; scanX < endX; scanX++)
    {
      for (long scanY = startY; scanY < endY; scanY++)
      {
        if (scanX < 0 || scanX >= snapshot.Width ||
            scanY < 0 || scanY >= snapshot.Height)
        {
          continue;
        }

        liquidUnits += snapshot.GetLiquidAmount((int)scanX, (int)scanY);
        scannedCellCount++;
      }
    }

    return new CactusWaterEligibilityResult(
      liquidUnits,
      scannedCellCount,
      liquidUnits / LiquidScale > rules.CactusWaterLimit);
  }

  public static bool ShouldBlockGrowth(
    CactusWaterEligibilityResult result,
    bool remixWorld,
    int candidateY,
    double worldSurfaceY)
  {
    if (!double.IsFinite(worldSurfaceY))
    {
      throw new ArgumentOutOfRangeException(nameof(worldSurfaceY));
    }

    return result.ExceedsLimit && (!remixWorld || candidateY <= worldSurfaceY);
  }
}
