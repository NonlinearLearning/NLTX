using System;

namespace Terraria.WorldGeneration.Housing;

public sealed class HousingWaterRuleDefinition
{
  public HousingWaterRuleDefinition(
    int cactusWaterWidth,
    int cactusWaterHeight,
    int cactusWaterLimit)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(cactusWaterWidth);
    ArgumentOutOfRangeException.ThrowIfNegative(cactusWaterHeight);
    ArgumentOutOfRangeException.ThrowIfNegative(cactusWaterLimit);

    CactusWaterWidth = cactusWaterWidth;
    CactusWaterHeight = cactusWaterHeight;
    CactusWaterLimit = cactusWaterLimit;
  }

  public static HousingWaterRuleDefinition Version4 { get; } = new(
    cactusWaterWidth: 50,
    cactusWaterHeight: 25,
    cactusWaterLimit: 25);

  public int CactusWaterWidth { get; }

  public int CactusWaterHeight { get; }

  public int CactusWaterLimit { get; }
}
