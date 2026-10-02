namespace Terraria.WorldGeneration.Systems;

public enum WorldSecretSeedRuntimeRegistryCommitStatus : byte
{
  Applied,
  Idempotent,
  RejectedInvalidCommand,
  RejectedUnknownVariant,
  RejectedStaleVersion,
  RejectedLifecycle,
  RejectedIdempotencyConflict,
}
