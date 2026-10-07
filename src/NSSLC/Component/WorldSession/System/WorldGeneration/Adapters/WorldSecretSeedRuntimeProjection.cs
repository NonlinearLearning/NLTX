using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Adapters;

public readonly record struct WorldSecretSeedRuntimeProjection
{
  public WorldSecretSeedRuntimeProjection(
    long generationId,
    ulong runtimeVersion,
    IReadOnlySet<string> enabledVariants)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(generationId);
    if (runtimeVersion == 0)
    {
      throw new ArgumentOutOfRangeException(nameof(runtimeVersion));
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
    EnabledVariants = copy.ToFrozenSet(StringComparer.Ordinal);
  }

  public long GenerationId { get; }

  public ulong RuntimeVersion { get; }

  public IReadOnlySet<string> EnabledVariants { get; }

  public int ActiveSecretSeedCount => EnabledVariants.Count;

  public bool IsEnabled(string variant)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(variant);
    return EnabledVariants.Contains(variant);
  }

  public static WorldSecretSeedRuntimeProjection From(
    WorldSecretSeedRuntimeRegistrySnapshot snapshot)
  {
    return new WorldSecretSeedRuntimeProjection(
      snapshot.GenerationId,
      snapshot.RuntimeVersion,
      snapshot.EnabledVariants);
  }
}
