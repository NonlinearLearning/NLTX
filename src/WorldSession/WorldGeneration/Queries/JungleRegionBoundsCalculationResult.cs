namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Contains the bounds produced by the pure jungle-boundary calculation.
/// </summary>
public readonly record struct JungleRegionBoundsCalculationResult(
  int JungleMinX,
  int JungleMaxX);
