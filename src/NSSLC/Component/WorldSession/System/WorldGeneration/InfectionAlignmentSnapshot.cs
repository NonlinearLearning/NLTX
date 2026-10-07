namespace Terraria.WorldGeneration.Components;

public readonly record struct InfectionAlignmentSnapshot(
  long GenerationId,
  bool CrimsonLeft,
  bool FlipInfections);
