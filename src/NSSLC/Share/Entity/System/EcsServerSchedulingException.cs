using System;

namespace Terraria.Entity;

/// <summary>Identifies a rejected ECS registration graph.</summary>
public enum EcsServerSchedulingFailureKind : byte
{
  DuplicateKey = 0,
  MissingDependency = 1,
  CrossPhaseDependency = 2,
  Cycle = 3,

  /// <summary>A coordination rule text could not be parsed into a rule.</summary>
  CoordinationRuleInvalid = 4,

  /// <summary>The schedule violates at least one loaded coordination rule.</summary>
  CoordinationRuleViolation = 5,
}

/// <summary>
/// Reports a registration graph that cannot produce a valid deterministic schedule.
/// </summary>
public sealed class EcsServerSchedulingException : InvalidOperationException
{
  public EcsServerSchedulingException(
      EcsServerSchedulingFailureKind kind,
      string message)
      : base(message)
  {
    Kind = kind;
  }

  public EcsServerSchedulingFailureKind Kind { get; }
}
