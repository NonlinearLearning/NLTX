using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Calculates the Version4 ocean-biome constraints from explicit beach-boundary facts.
/// </summary>
public static class OceanBiomeConstraintCalculationQuery
{
  public static OceanBiomeConstraintSnapshot Calculate(
    in OceanBiomeConstraintCalculationInput input)
  {
    BeachBoundarySnapshot beachBoundary = input.BeachBoundary;
    ArgumentOutOfRangeException.ThrowIfNegative(beachBoundary.GenerationId);

    int beachSandRandomCenter = beachBoundary.BeachSandRandomCenter;
    return new OceanBiomeConstraintSnapshot(
      beachBoundary.GenerationId,
      beachBoundary.OceanWaterStartRandomMin + 40,
      275,
      beachSandRandomCenter + 60,
      50,
      beachSandRandomCenter + 20,
      beachSandRandomCenter + 20,
      beachSandRandomCenter + 20,
      beachSandRandomCenter + 20);
  }
}
