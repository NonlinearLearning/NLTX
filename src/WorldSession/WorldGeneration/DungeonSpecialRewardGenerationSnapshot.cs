namespace Terraria.WorldGeneration.Components;

public readonly record struct DungeonSpecialRewardGenerationSnapshot(
  long GenerationId,
  bool GeneratedShadowKey,
  bool GeneratedRamRune);
