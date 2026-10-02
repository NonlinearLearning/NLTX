namespace Terraria.WorldGeneration.Adapters;

public readonly record struct WorldGenerationActionCommitResult(
  bool Accepted,
  string? RejectionReason = null);
