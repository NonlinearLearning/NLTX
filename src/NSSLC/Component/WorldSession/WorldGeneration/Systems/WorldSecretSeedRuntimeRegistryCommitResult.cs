using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

public readonly record struct WorldSecretSeedRuntimeRegistryCommitResult(
  WorldSecretSeedRuntimeRegistryCommitStatus Status,
  WorldSecretSeedRuntimeRegistrySnapshot Snapshot)
{
  public bool Accepted =>
    Status is
      WorldSecretSeedRuntimeRegistryCommitStatus.Applied or
      WorldSecretSeedRuntimeRegistryCommitStatus.Idempotent;

  public bool Changed =>
    Status == WorldSecretSeedRuntimeRegistryCommitStatus.Applied;
}
