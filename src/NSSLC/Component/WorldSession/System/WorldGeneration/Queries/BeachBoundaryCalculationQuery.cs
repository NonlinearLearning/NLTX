using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Calculates Version4 beach boundaries from explicit, already-consumed random results.
/// </summary>
public static class BeachBoundaryCalculationQuery
{
  private const short LeftDungeonSide = -1;
  private const short RightDungeonSide = 1;

  public static BeachBoundarySnapshot Calculate(
    in BeachBoundaryCalculationInput input)
  {
    Validate(input);

    bool useTenthAnniversaryFixedBoundary =
      input.IsTenthAnniversaryWorld && !input.IsRemixWorld;
    int randomBoundaryCenter = input.BeachSandRandomCenter;
    int randomBoundaryWidth = input.BeachSandRandomWidthRange;

    int leftBeachEnd = useTenthAnniversaryFixedBoundary
      ? unchecked(randomBoundaryCenter + randomBoundaryWidth)
      : input.LeftBeachRandomRoll;
    if (input.DungeonSide == RightDungeonSide)
    {
      leftBeachEnd = unchecked(
        leftBeachEnd + input.BeachSandDungeonExtraWidth);
    }
    else
    {
      leftBeachEnd = unchecked(
        leftBeachEnd + input.BeachSandJungleExtraWidth);
    }

    int rightBeachStart = useTenthAnniversaryFixedBoundary
      ? unchecked(input.MaxTilesX - (randomBoundaryCenter + randomBoundaryWidth))
      : unchecked(input.MaxTilesX - input.RightBeachRandomRoll);
    if (input.DungeonSide == LeftDungeonSide)
    {
      rightBeachStart = unchecked(
        rightBeachStart - input.BeachSandDungeonExtraWidth);
    }
    else
    {
      rightBeachStart = unchecked(
        rightBeachStart - input.BeachSandJungleExtraWidth);
    }

    return new BeachBoundarySnapshot(
      input.GenerationId,
      leftBeachEnd,
      rightBeachStart,
      input.BeachBordersWidth,
      input.BeachSandRandomCenter,
      input.BeachSandRandomWidthRange,
      input.BeachSandDungeonExtraWidth,
      input.BeachSandJungleExtraWidth,
      input.ShellStartXLeft,
      input.ShellStartYLeft,
      input.ShellStartXRight,
      input.ShellStartYRight,
      input.OceanWaterStartRandomMin);
  }

  private static void Validate(in BeachBoundaryCalculationInput input)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(input.GenerationId);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(input.MaxTilesX);
    ArgumentOutOfRangeException.ThrowIfNegative(input.BeachSandRandomWidthRange);

    bool useTenthAnniversaryFixedBoundary =
      input.IsTenthAnniversaryWorld && !input.IsRemixWorld;
    if (useTenthAnniversaryFixedBoundary)
    {
      return;
    }

    long minimumRandomValue =
      (long)input.BeachSandRandomCenter - input.BeachSandRandomWidthRange;
    long maximumRandomValue =
      (long)input.BeachSandRandomCenter + input.BeachSandRandomWidthRange;
    if (input.LeftBeachRandomRoll < minimumRandomValue ||
        input.LeftBeachRandomRoll >= maximumRandomValue)
    {
      throw new ArgumentOutOfRangeException(nameof(input.LeftBeachRandomRoll));
    }

    if (input.RightBeachRandomRoll < minimumRandomValue ||
        input.RightBeachRandomRoll >= maximumRandomValue)
    {
      throw new ArgumentOutOfRangeException(nameof(input.RightBeachRandomRoll));
    }
  }
}
