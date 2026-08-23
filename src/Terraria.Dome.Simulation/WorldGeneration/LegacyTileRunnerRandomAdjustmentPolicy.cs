using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyTileRunnerRandomAdjustment(
  double Strength,
  int Steps,
  bool AppliedWorldVariantAdjustment);

public static class LegacyTileRunnerRandomAdjustmentPolicy
{
  public static LegacyTileRunnerRandomAdjustment Apply(
    double strength,
    int steps,
    int tileType,
    bool mudWall,
    bool drunkWorld,
    bool remixWorld,
    bool getGoodWorld,
    int firstRandomOffset,
    int secondRandomOffset,
    int goodWorldStepOffset)
  {
    if (!double.IsFinite(strength) || strength <= 0 || steps <= 0 ||
        firstRandomOffset < -80 || firstRandomOffset > 80 ||
        secondRandomOffset < -80 || secondRandomOffset > 80 ||
        goodWorldStepOffset < 0 || goodWorldStepOffset > 2)
    {
      throw new ArgumentOutOfRangeException(nameof(strength));
    }

    if (mudWall)
    {
      return new LegacyTileRunnerRandomAdjustment(strength, steps, false);
    }

    if (drunkWorld)
    {
      double factor = 1.0 + firstRandomOffset * 0.01;
      double adjustedStrength = strength * factor;
      int adjustedSteps = (int)(steps * (1.0 + secondRandomOffset * 0.01));
      return new LegacyTileRunnerRandomAdjustment(adjustedStrength, adjustedSteps, true);
    }

    if (remixWorld)
    {
      return new LegacyTileRunnerRandomAdjustment(
        strength * (1.0 + firstRandomOffset * 0.01),
        steps,
        true);
    }

    if (getGoodWorld && tileType != 57)
    {
      return new LegacyTileRunnerRandomAdjustment(
        strength * (1.0 + firstRandomOffset * 0.015),
        steps + goodWorldStepOffset,
        true);
    }

    return new LegacyTileRunnerRandomAdjustment(strength, steps, false);
  }
}
