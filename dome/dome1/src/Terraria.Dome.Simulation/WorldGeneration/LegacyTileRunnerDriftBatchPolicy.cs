using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyTileRunnerDriftBatch(
  double CenterX,
  double CenterY,
  double DirectionX,
  double DirectionY,
  double RemainingSteps,
  int AppliedDriftCount);

public static class LegacyTileRunnerDriftBatchPolicy
{
  private static readonly int[] StrengthThresholds =
  {
    50,
    100,
    150,
    200,
    250,
    300,
    400,
    500,
    600,
    700,
    800,
    900
  };

  private static readonly IReadOnlyList<int> ReadOnlyStrengthThresholds =
    Array.AsReadOnly(StrengthThresholds);

  public static IReadOnlyList<int> RegisterDefaults()
  {
    return ReadOnlyStrengthThresholds;
  }

  public static LegacyTileRunnerDriftBatch Apply(
    double centerX,
    double centerY,
    double directionX,
    double directionY,
    double strength,
    double remainingSteps,
    bool drunkWorld,
    int drunkGateRoll,
    IReadOnlyList<int> directionXRolls,
    IReadOnlyList<int> directionYRolls)
  {
    ArgumentNullException.ThrowIfNull(directionXRolls);
    ArgumentNullException.ThrowIfNull(directionYRolls);
    if (directionXRolls.Count < StrengthThresholds.Length ||
        directionYRolls.Count < StrengthThresholds.Length)
    {
      throw new ArgumentException("All strength-band random draws are required.");
    }

    bool apply = !drunkWorld || drunkGateRoll != 0;
    int appliedCount = 0;
    for (int index = 0; index < StrengthThresholds.Length; index++)
    {
      if (!apply || strength <= StrengthThresholds[index])
      {
        break;
      }

      centerX += directionX;
      centerY += directionY;
      remainingSteps -= 1.0;
      directionX += directionXRolls[index] * 0.05;
      directionY += directionYRolls[index] * 0.05;
      appliedCount++;
    }

    return new LegacyTileRunnerDriftBatch(
      centerX,
      centerY,
      directionX,
      directionY,
      remainingSteps,
      appliedCount);
  }
}
