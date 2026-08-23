using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyTileRunnerDistancePolicy
{
  public static bool IsWithinManhattanEnvelope(
    int tileX,
    int tileY,
    double centerX,
    double centerY,
    double strength,
    int randomOffset)
  {
    if (!double.IsFinite(centerX) || !double.IsFinite(centerY) ||
        !double.IsFinite(strength) || strength <= 0 || randomOffset < -10 ||
        randomOffset > 10)
    {
      throw new ArgumentOutOfRangeException(nameof(centerX));
    }

    double distance = Math.Abs(tileX - centerX) + Math.Abs(tileY - centerY);
    double limit = strength * 0.5 * (1.0 + randomOffset * 0.015);
    return distance < limit;
  }
}
