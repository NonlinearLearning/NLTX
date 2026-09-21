namespace Terraria.WorldGeneration.Components;

public readonly record struct ShimmerBiomeAnchorSnapshot(
  long GenerationId,
  bool HasAnchor,
  ShimmerBiomeAnchorPoint Position);
