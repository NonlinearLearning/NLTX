using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyErrorWorldPassCountPolicy
{
  private const int DensePassMultiplier = 10;
  private const int SwapDivisor = 2;

  public static LegacyErrorWorldPassCounts Create(
    int worldWidth,
    int errorWorldAdjustment,
    bool isSkyblockWorld)
  {
    if (worldWidth <= 0 || errorWorldAdjustment <= 0 ||
        worldWidth > int.MaxValue / DensePassMultiplier)
    {
      throw new ArgumentOutOfRangeException(nameof(worldWidth));
    }

    int densePassCount = worldWidth * DensePassMultiplier / errorWorldAdjustment;
    int randomBlockRewrites = isSkyblockWorld ? densePassCount / SwapDivisor : densePassCount;
    int swapCount = worldWidth / SwapDivisor / errorWorldAdjustment;
    return new LegacyErrorWorldPassCounts(
      randomBlockRewrites,
      swapCount,
      swapCount,
      densePassCount);
  }
}
