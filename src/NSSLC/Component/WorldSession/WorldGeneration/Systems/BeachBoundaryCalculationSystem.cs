using System;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Queries;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Commits the pure beach-boundary calculation to its generation-scoped component.
/// </summary>
public static class BeachBoundaryCalculationSystem
{
  public static BeachBoundarySnapshot CalculateAndCommit(
    BeachBoundaryComponent component,
    in BeachBoundaryCalculationInput input)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (input.GenerationId != component.GenerationId)
    {
      throw new ArgumentException(
        "Beach boundaries cannot be calculated for another generation.",
        nameof(input));
    }

    BeachBoundarySnapshot calculated =
      BeachBoundaryCalculationQuery.Calculate(in input);
    BeachBoundarySystem.Commit(component, in calculated);
    return BeachBoundaryQuery.Snapshot(component);
  }
}
