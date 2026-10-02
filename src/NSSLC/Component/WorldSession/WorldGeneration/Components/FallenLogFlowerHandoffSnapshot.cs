namespace Terraria.WorldGeneration.Components;

public readonly record struct FallenLogFlowerHandoffSnapshot(
  long GenerationId,
  int LogX,
  int LogY,
  bool HasPendingLog);
