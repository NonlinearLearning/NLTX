using System;
using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.WorldGeneration.Components;

public readonly record struct WorldSecretSeedRuntimeRegistrySnapshot
{
  public WorldSecretSeedRuntimeRegistrySnapshot(
    long generationId,
    ulong runtimeVersion,
    WorldSecretSeedRuntimeRegistryLifecycle lifecycle,
    IReadOnlySet<string> enabledVariants)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(generationId);
    if (runtimeVersion == 0)
    {
      throw new ArgumentOutOfRangeException(nameof(runtimeVersion));
    }

    if (!Enum.IsDefined(lifecycle))
    {
      throw new ArgumentOutOfRangeException(nameof(lifecycle));
    }

    ArgumentNullException.ThrowIfNull(enabledVariants);
    HashSet<string> copy = new(StringComparer.Ordinal);
    foreach (string? variant in enabledVariants)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(variant);
      copy.Add(variant);
    }

    GenerationId = generationId;
    RuntimeVersion = runtimeVersion;
    Lifecycle = lifecycle;
    EnabledVariants = copy.ToFrozenSet(StringComparer.Ordinal);
  }

  public long GenerationId { get; }

  public ulong RuntimeVersion { get; }

  public WorldSecretSeedRuntimeRegistryLifecycle Lifecycle { get; }

  public IReadOnlySet<string> EnabledVariants { get; }

  public int ActiveSecretSeedCount => EnabledVariants.Count;

  public bool IsActive =>
    Lifecycle == WorldSecretSeedRuntimeRegistryLifecycle.Active;

  public bool IsEnabled(string variant)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(variant);
    return EnabledVariants.Contains(variant);
  }
}
