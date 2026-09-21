using System;
using System.Collections.Generic;

namespace Terraria.WorldGeneration.Adapters;

public sealed class WorldSecretSeedRuntimeRegistrySerializedState
{
  public WorldSecretSeedRuntimeRegistrySerializedState(
    long generationId,
    ulong runtimeVersion,
    IReadOnlyList<string> enabledVariantKeys)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(generationId);
    if (runtimeVersion == 0)
    {
      throw new ArgumentOutOfRangeException(nameof(runtimeVersion));
    }

    ArgumentNullException.ThrowIfNull(enabledVariantKeys);
    List<string> copy = new(enabledVariantKeys.Count);
    foreach (string? key in enabledVariantKeys)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(key);
      copy.Add(key);
    }

    GenerationId = generationId;
    RuntimeVersion = runtimeVersion;
    EnabledVariantKeys = copy.AsReadOnly();
  }

  public long GenerationId { get; }

  public ulong RuntimeVersion { get; }

  public IReadOnlyList<string> EnabledVariantKeys { get; }
}
