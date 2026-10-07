namespace Terraria.WorldGeneration.Definitions;

public readonly record struct WorldSkyblockGenerationPolicySelection(
  bool DenyFloatingIslands,
  bool DenyAllGeneration,
  bool DenySomeGeneration,
  long GenerationId,
  ulong RuntimeVersion);
