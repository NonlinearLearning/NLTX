using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Provides the committed beach values required to initialize ocean-biome constraints.
/// </summary>
public readonly record struct OceanBiomeConstraintCalculationInput(
  BeachBoundarySnapshot BeachBoundary);
