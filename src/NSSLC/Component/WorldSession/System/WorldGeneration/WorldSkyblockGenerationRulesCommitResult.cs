using Terraria.WorldGeneration.Definitions;

namespace Terraria.WorldGeneration.Systems;

public readonly record struct WorldSkyblockGenerationRulesCommitResult(
  long GenerationId,
  ulong ScanVersion,
  WorldSkyblockGenerationRulesSelection CommittedRules,
  bool LowTilesChanged,
  bool DungeonCoordinatesCleared);
