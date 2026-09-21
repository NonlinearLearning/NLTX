using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using Terraria.WorldGeneration.Definitions;

namespace Terraria.WorldGeneration.Components;

public sealed class WorldSecretSeedRuntimeRegistryComponent
{
  private readonly WorldSecretSeedRegistryDefinitionsProjection _definitions;
  private FrozenSet<string> _enabledVariants;
  private readonly Dictionary<string, string> _processedCommandFingerprints =
    new(StringComparer.Ordinal);

  public WorldSecretSeedRuntimeRegistryComponent(
    long generationId,
    ulong runtimeVersion,
    WorldSecretSeedRegistryDefinitionsProjection? definitions = null,
    IReadOnlySet<string>? enabledVariants = null)
  {
    ValidateIdentity(generationId, runtimeVersion);
    _definitions = definitions ?? WorldSecretSeedRegistryDefinitionsProjection.Version4;
    GenerationId = generationId;
    RuntimeVersion = runtimeVersion;
    Lifecycle = WorldSecretSeedRuntimeRegistryLifecycle.Active;
    _enabledVariants = CreateValidatedSet(enabledVariants ?? FrozenSet<string>.Empty);
  }

  public long GenerationId { get; private set; }

  public ulong RuntimeVersion { get; private set; }

  public WorldSecretSeedRuntimeRegistryLifecycle Lifecycle { get; private set; }

  public IReadOnlySet<string> EnabledVariants => _enabledVariants;

  public int ActiveSecretSeedCount => _enabledVariants.Count;

  public WorldSecretSeedRuntimeRegistrySnapshot CreateSnapshot()
  {
    return new WorldSecretSeedRuntimeRegistrySnapshot(
      GenerationId,
      RuntimeVersion,
      Lifecycle,
      _enabledVariants);
  }

  internal bool Contains(string variant)
  {
    return _enabledVariants.Contains(variant);
  }

  internal bool IsKnownVariant(string variant)
  {
    return _definitions.TryGet(variant, out _);
  }

  internal void ReplaceEnabledVariants(IReadOnlySet<string> enabledVariants)
  {
    ArgumentNullException.ThrowIfNull(enabledVariants);
    _enabledVariants = CreateValidatedSet(enabledVariants);
  }

  internal bool TryGetProcessedCommand(
    string idempotencyKey,
    string fingerprint,
    out bool conflicts)
  {
    if (!_processedCommandFingerprints.TryGetValue(
      idempotencyKey,
      out string? previousFingerprint))
    {
      conflicts = false;
      return false;
    }

    conflicts = !StringComparer.Ordinal.Equals(previousFingerprint, fingerprint);
    return true;
  }

  internal void RecordProcessedCommand(
    string idempotencyKey,
    string fingerprint)
  {
    _processedCommandFingerprints[idempotencyKey] = fingerprint;
  }

  internal void ResetForGeneration(long generationId, ulong runtimeVersion)
  {
    ValidateIdentity(generationId, runtimeVersion);
    GenerationId = generationId;
    RuntimeVersion = runtimeVersion;
    Lifecycle = WorldSecretSeedRuntimeRegistryLifecycle.Active;
    _enabledVariants = FrozenSet<string>.Empty;
    _processedCommandFingerprints.Clear();
  }

  internal void Close()
  {
    Lifecycle = WorldSecretSeedRuntimeRegistryLifecycle.Closed;
    _enabledVariants = FrozenSet<string>.Empty;
    _processedCommandFingerprints.Clear();
  }

  private FrozenSet<string> CreateValidatedSet(IReadOnlySet<string> enabledVariants)
  {
    HashSet<string> copy = new(StringComparer.Ordinal);
    foreach (string? variant in enabledVariants)
    {
      if (string.IsNullOrWhiteSpace(variant) || !IsKnownVariant(variant))
      {
        throw new ArgumentException(
          "Every enabled secret-seed variant must be a known stable registry key.",
          nameof(enabledVariants));
      }

      copy.Add(variant);
    }

    return copy.ToFrozenSet(StringComparer.Ordinal);
  }

  private static void ValidateIdentity(long generationId, ulong runtimeVersion)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(generationId);
    if (runtimeVersion == 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(runtimeVersion),
        "A runtime registry version must be greater than zero.");
    }
  }
}
