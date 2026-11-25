using System;
using System.Collections.Generic;
using Arch.System;

namespace Terraria.Share.Entities;

/// <summary>
/// Associates one Arch system with an explicit NSSLC server phase.
/// </summary>
public sealed class EcsServerSystemRegistration
{
  /// <summary>
  /// Creates a legacy registration without dependency metadata.
  /// </summary>
  /// <remarks>
  /// Legacy registrations remain usable for callers that only need the fixed Simulation phase.
  /// They receive a synthetic schedule key inside <see cref="EcsServer"/> and cannot be dependency
  /// targets. New composition roots should use the keyed constructor.
  /// </remarks>
  public EcsServerSystemRegistration(
      EcsServerPhase phase,
      ISystem<EcsServerTickContext> system)
      : this(
          phase,
          (string?)null,
          system,
          (IEnumerable<string>?)null,
          (IEnumerable<string>?)null,
          (string?)null,
          requireKey: false)
  {
  }

  /// <summary>
  /// Creates a registration with explicit phase-local dependency metadata.
  /// </summary>
  public EcsServerSystemRegistration(
      EcsServerPhase phase,
      string key,
      ISystem<EcsServerTickContext> system,
      IEnumerable<string>? before = null,
      IEnumerable<string>? after = null,
      string? reason = null)
      : this(phase, key, system, before, after, reason, requireKey: true)
  {
  }

  private EcsServerSystemRegistration(
      EcsServerPhase phase,
      string? key,
      ISystem<EcsServerTickContext> system,
      IEnumerable<string>? before,
      IEnumerable<string>? after,
      string? reason,
      bool requireKey)
  {
    if (!Enum.IsDefined(phase))
    {
      throw new ArgumentOutOfRangeException(nameof(phase));
    }

    if (requireKey)
    {
      ValidateKey(key, nameof(key));
    }

    ArgumentNullException.ThrowIfNull(system);
    if (reason is not null && string.IsNullOrWhiteSpace(reason))
    {
      throw new ArgumentException(
          "The registration reason must be null or contain non-whitespace text.",
          nameof(reason));
    }

    Phase = phase;
    Key = key;
    System = system;
    Before = CopyKeys(before, nameof(before));
    After = CopyKeys(after, nameof(after));
    Reason = reason;
  }

  public EcsServerPhase Phase { get; }

  /// <summary>
  /// Gets the stable composition-root key, or null for a legacy unkeyed registration.
  /// </summary>
  public string? Key { get; }

  public ISystem<EcsServerTickContext> System { get; }

  /// <summary>Gets same-phase keys that must run after this registration.</summary>
  public IReadOnlyList<string> Before { get; }

  /// <summary>Gets same-phase keys that must run before this registration.</summary>
  public IReadOnlyList<string> After { get; }

  /// <summary>Gets the human-readable reason for the dependency metadata.</summary>
  public string? Reason { get; }

  private static void ValidateKey(string? key, string parameterName)
  {
    if (string.IsNullOrWhiteSpace(key) || key != key.Trim())
    {
      throw new ArgumentException(
          "Registration keys must contain non-whitespace text without surrounding whitespace.",
          parameterName);
    }
  }

  private static IReadOnlyList<string> CopyKeys(
      IEnumerable<string>? keys,
      string parameterName)
  {
    if (keys is null)
    {
      return Array.Empty<string>();
    }

    List<string> copied = [];
    HashSet<string> seen = new(StringComparer.Ordinal);
    foreach (string? key in keys)
    {
      ValidateKey(key, parameterName);
      if (!seen.Add(key!))
      {
        throw new ArgumentException(
            $"Dependency key '{key}' is repeated in one registration.", parameterName);
      }

      copied.Add(key);
    }

    return copied.AsReadOnly();
  }
}
