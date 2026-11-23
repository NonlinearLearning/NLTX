using System;
using System.Collections.Generic;

namespace Terraria.Entity;

/// <summary>An ordered, immutable set of coordination rules applied to every schedule.</summary>
/// <remarks>
/// The set is evaluated after the dependency graph has been validated and sorted, so a rule can
/// only reject a schedule — never reorder it. Ordering authority stays with the explicit
/// before/after edges; rules exist to catch contracts that registration code forgot to state.
/// </remarks>
public sealed class EcsCoordinationRuleSet
{
  /// <summary>A rule set that accepts every schedule.</summary>
  public static EcsCoordinationRuleSet Empty { get; } = new([]);

  private readonly EcsCoordinationRule[] _rules;

  private EcsCoordinationRuleSet(EcsCoordinationRule[] rules)
  {
    _rules = rules;
    Rules = Array.AsReadOnly(rules);
  }

  public IReadOnlyList<EcsCoordinationRule> Rules { get; }

  public int Count => _rules.Length;

  /// <summary>Creates a rule set from an explicit rule list, preserving declaration order.</summary>
  public static EcsCoordinationRuleSet Create(IEnumerable<EcsCoordinationRule> rules)
  {
    ArgumentNullException.ThrowIfNull(rules);
    List<EcsCoordinationRule> collected = [];
    foreach (EcsCoordinationRule rule in rules)
    {
      collected.Add(rule);
    }

    return collected.Count == 0 ? Empty : new EcsCoordinationRuleSet([.. collected]);
  }

  public static EcsCoordinationRuleSet FromRules(params EcsCoordinationRule[] rules)
  {
    ArgumentNullException.ThrowIfNull(rules);
    return Create(rules);
  }
}
