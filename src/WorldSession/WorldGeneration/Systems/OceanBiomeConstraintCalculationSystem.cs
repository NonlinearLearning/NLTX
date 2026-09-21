using System;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Queries;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Commits the calculated ocean-biome constraints to the generation-scoped component.
/// </summary>
public static class OceanBiomeConstraintCalculationSystem
{
  public static OceanBiomeConstraintSnapshot CalculateAndCommit(
    OceanBiomeConstraintComponent component,
    in OceanBiomeConstraintCalculationInput input)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (input.BeachBoundary.GenerationId != component.GenerationId)
    {
      throw new ArgumentException(
        "Ocean-biome constraints cannot be calculated for another generation.",
        nameof(input));
    }

    OceanBiomeConstraintSnapshot calculated =
      OceanBiomeConstraintCalculationQuery.Calculate(in input);
    OceanBiomeConstraintSystem.Commit(component, in calculated);
    return OceanBiomeConstraintQuery.Snapshot(component);
  }
}
