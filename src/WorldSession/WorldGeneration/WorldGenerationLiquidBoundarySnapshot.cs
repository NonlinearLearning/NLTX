namespace Terraria.WorldGeneration.Components;

public readonly record struct WorldGenerationLiquidBoundarySnapshot(
  long GenerationId,
  int LavaLine,
  int WaterLine);
