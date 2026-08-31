using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyIceBiomeSurfaceDefinition(
  int SnowWallType,
  int SnowTileType,
  int IceTileType,
  int MinimumDrift,
  int MaximumDriftExclusive,
  int DriftAdjustmentChance,
  int DriftAdjustmentMinimum,
  int DriftAdjustmentMaximumExclusive)
{
  public void Validate()
  {
    if (SnowWallType < 0 || SnowTileType < 0 || IceTileType < 0 ||
        MaximumDriftExclusive <= MinimumDrift || DriftAdjustmentChance <= 0 ||
        DriftAdjustmentMaximumExclusive <= DriftAdjustmentMinimum)
    {
      throw new InvalidOperationException("IceBiome surface definition is invalid.");
    }
  }

  public static LegacyIceBiomeSurfaceDefinition CreateDefault()
  {
    return new LegacyIceBiomeSurfaceDefinition(40, 147, 161, -3, 4, 3, -4, 5);
  }
}
