namespace Terraria.WorldGeneration.Systems;

public readonly record struct ShimmerBiomeCommitResult(
  ShimmerBiomeCommitStatus Status,
  bool BiomeCommitSucceeded,
  bool Published);
