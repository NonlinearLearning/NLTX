namespace Terraria.WorldGeneration.Components;

public readonly record struct FloatingIslandHouseSnapshot(
  long GenerationId,
  bool SkyLake,
  int X,
  int Y,
  int Style);
