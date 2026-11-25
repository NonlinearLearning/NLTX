using System;

namespace Terraria.Share.Entities;

/// <summary>Identifies what one coordination rule constrains in an ECS schedule.</summary>
public enum EcsCoordinationRuleKind : byte
{
  /// <summary>One key must execute before another key in the same phase.</summary>
  Before = 0,

  /// <summary>One key must be registered in the given phase.</summary>
  RequiresPhase = 1,

  /// <summary>One key must declare at least one before/after dependency edge.</summary>
  RequiresDependency = 2,

  /// <summary>One key must carry a non-empty registration reason.</summary>
  RequiresReason = 3
}

/// <summary>
/// One declarative coordination rule evaluated against a schedule that has already been built.
/// </summary>
/// <remarks>
/// A rule states a property the schedule must hold; it never states an order by registration
/// index. Rules are data so the same scheduler can be checked against different collaboration
/// contracts without changing registration code, and so a violated contract fails at
/// construction instead of drifting silently.
/// </remarks>
public readonly record struct EcsCoordinationRule
{
  private EcsCoordinationRule(
      EcsCoordinationRuleKind kind,
      string key,
      string? otherKey,
      EcsServerPhase? phase,
      string text)
  {
    Kind = kind;
    Key = key;
    OtherKey = otherKey;
    Phase = phase;
    Text = text;
  }

  public EcsCoordinationRuleKind Kind { get; }

  /// <summary>The constrained key; for <see cref="EcsCoordinationRuleKind.Before"/> the earlier key.</summary>
  public string Key { get; }

  /// <summary>The later key for <see cref="EcsCoordinationRuleKind.Before"/>; null otherwise.</summary>
  public string? OtherKey { get; }

  /// <summary>The required phase; null unless the rule is phase-bound.</summary>
  public EcsServerPhase? Phase { get; }

  /// <summary>Canonical single-line text used in diagnostics.</summary>
  public string Text { get; }

  public static EcsCoordinationRule Before(string beforeKey, string afterKey)
  {
    return new EcsCoordinationRule(
        EcsCoordinationRuleKind.Before,
        RequireKey(beforeKey, nameof(beforeKey)),
        RequireKey(afterKey, nameof(afterKey)),
        phase: null,
        $"before {beforeKey} {afterKey}");
  }

  public static EcsCoordinationRule RequiresPhase(EcsServerPhase phase, string key)
  {
    if (!Enum.IsDefined(phase))
    {
      throw new ArgumentOutOfRangeException(nameof(phase));
    }

    return new EcsCoordinationRule(
        EcsCoordinationRuleKind.RequiresPhase,
        RequireKey(key, nameof(key)),
        otherKey: null,
        phase,
        $"phase {phase} {key}");
  }

  public static EcsCoordinationRule RequiresDependency(string key)
  {
    return new EcsCoordinationRule(
        EcsCoordinationRuleKind.RequiresDependency,
        RequireKey(key, nameof(key)),
        otherKey: null,
        phase: null,
        $"requires-dependency {key}");
  }

  public static EcsCoordinationRule RequiresReason(string key)
  {
    return new EcsCoordinationRule(
        EcsCoordinationRuleKind.RequiresReason,
        RequireKey(key, nameof(key)),
        otherKey: null,
        phase: null,
        $"requires-reason {key}");
  }

  /// <summary>Rejects keys that cannot round-trip through the declarative rule text.</summary>
  public static string RequireKey(string key, string parameterName)
  {
    if (string.IsNullOrWhiteSpace(key) || key != key.Trim() ||
        key.IndexOfAny([' ', '\t', '#']) >= 0)
    {
      throw new ArgumentException(
          "Coordination rule keys must contain non-whitespace text without spaces, '#' or " +
          "surrounding whitespace.",
          parameterName);
    }

    return key;
  }
}
