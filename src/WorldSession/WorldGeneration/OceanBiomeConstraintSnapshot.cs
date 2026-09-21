namespace Terraria.WorldGeneration.Components;

public readonly record struct OceanBiomeConstraintSnapshot(
  long GenerationId,
  int OceanWaterStartRandomMax,
  int OceanWaterForcedJungleLength,
  int EvilBiomeBeachAvoidance,
  int EvilBiomeAvoidanceMidFixer,
  int LakesBeachAvoidance,
  int SmallHolesBeachAvoidance,
  int SurfaceCavesBeachAvoidance,
  int SurfaceCavesBeachAvoidance2);
