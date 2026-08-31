using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TileReframePolicy
{
  public static TileReframeDecision Evaluate(int currentCount, int maximumDepth = 25)
  {
    if (currentCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(currentCount));
    }

    if (maximumDepth <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumDepth));
    }

    return new TileReframeDecision(
      currentCount,
      maximumDepth,
      currentCount + 1 < maximumDepth);
  }
}
