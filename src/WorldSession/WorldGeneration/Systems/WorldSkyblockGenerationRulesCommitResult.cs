namespace Terraria.WorldGeneration.Systems;

public readonly record struct WorldSkyblockGenerationRulesCommitResult(
  long GenerationId,
  ulong ScanVersion,
  bool LowTilesChanged,
  bool DungeonCoordinatesCleared);
