using System;
using System.Collections.Generic;
using Terraria.Share.Ai;
using Terraria.Share.Entities;

namespace Terraria.Unshared.Npc;

/// <summary>
/// One NPC behavior declaration: the shared registration entry plus the intent kinds that behavior
/// is allowed to emit.
/// </summary>
/// <remarks>
/// The emitted set is data, not documentation: <see cref="NpcAiIntentAdapter"/> checks it against
/// its own dispatch table while it is constructed, so "behavior emits a kind nobody handles"
/// becomes a startup failure instead of a mid-tick exception.
/// </remarks>
public sealed class NpcAiBehaviorDeclaration
{
  private readonly NpcAiIntentKind[] _emits;

  public NpcAiBehaviorDeclaration(
      AiBehaviorDeclaration<NpcAiIntent> behavior,
      IReadOnlyList<NpcAiIntentKind> emits,
      IReadOnlyList<AiAnchor> after,
      IReadOnlyList<AiAnchor> before)
  {
    ArgumentNullException.ThrowIfNull(behavior);
    ArgumentNullException.ThrowIfNull(emits);
    ArgumentNullException.ThrowIfNull(after);
    ArgumentNullException.ThrowIfNull(before);
    if (emits.Count == 0)
    {
      throw new ArgumentException(
          "A declared NPC AI behavior must emit at least one intent kind.", nameof(emits));
    }

    _emits = new NpcAiIntentKind[emits.Count];
    HashSet<NpcAiIntentKind> seen = [];
    for (int i = 0; i < emits.Count; i++)
    {
      NpcAiIntentKind kind = emits[i];
      if (!Enum.IsDefined(kind))
      {
        throw new ArgumentOutOfRangeException(nameof(emits));
      }

      if (!seen.Add(kind))
      {
        throw new ArgumentException(
            $"{behavior.BehaviorType.FullName} declares {kind} twice.", nameof(emits));
      }

      _emits[i] = kind;
    }

    After = CopyAnchors(after, "after", behavior.BehaviorType);
    Before = CopyAnchors(before, "before", behavior.BehaviorType);
    HashSet<AiAnchor> overlap = [.. After];
    overlap.IntersectWith(Before);
    if (overlap.Count != 0)
    {
      throw new ArgumentException(
          $"{behavior.BehaviorType.FullName} declares " + string.Join(", ", overlap) +
          " as both a predecessor and a successor anchor; one anchor cannot constrain a system " +
          "in both directions.",
          nameof(before));
    }

    Behavior = behavior;
    Emits = Array.AsReadOnly(_emits);
    // 提交目标是投影，不是第二个手写字段：声明无法与提交表漂移。
    Commits = NpcAiIntentCommitTable.TargetsOf(_emits);
  }

  /// <summary>The shared AI declaration this entry registers.</summary>
  public AiBehaviorDeclaration<NpcAiIntent> Behavior { get; }

  /// <summary>Intent kinds the behavior may emit; never empty.</summary>
  public IReadOnlyList<NpcAiIntentKind> Emits { get; }

  /// <summary>
  /// The components <see cref="Emits"/> commits to, projected through
  /// <see cref="NpcAiIntentCommitTable"/>. There is no second field to keep in sync: a behavior
  /// that adds an emitted kind changes this set by construction.
  /// </summary>
  public IReadOnlyList<Type> Commits { get; }

  /// <summary>Capability roles this behavior must run after.</summary>
  public IReadOnlyList<AiAnchor> After { get; }

  /// <summary>Capability roles this behavior must run before.</summary>
  public IReadOnlyList<AiAnchor> Before { get; }

  private static IReadOnlyList<AiAnchor> CopyAnchors(
      IReadOnlyList<AiAnchor> anchors,
      string role,
      Type behaviorType)
  {
    AiAnchor[] copied = new AiAnchor[anchors.Count];
    HashSet<AiAnchor> seen = [];
    for (int i = 0; i < anchors.Count; i++)
    {
      AiAnchor anchor = anchors[i];
      if (!Enum.IsDefined(anchor))
      {
        throw new ArgumentOutOfRangeException(
            role, anchor, $"{behaviorType.FullName} names an undefined anchor.");
      }

      if (!seen.Add(anchor))
      {
        throw new ArgumentException(
            $"{behaviorType.FullName} declares anchor {anchor} twice in {role}.");
      }

      copied[i] = anchor;
    }

    return Array.AsReadOnly(copied);
  }
}

/// <summary>One derived schedule edge: <see cref="BeforeKey"/> must run before <see cref="AfterKey"/>.</summary>
public readonly record struct NpcAiOrderingEdge
{
  public NpcAiOrderingEdge(string beforeKey, string afterKey)
  {
    BeforeKey = EcsCoordinationRule.RequireKey(beforeKey, nameof(beforeKey));
    AfterKey = EcsCoordinationRule.RequireKey(afterKey, nameof(afterKey));
    if (beforeKey == afterKey)
    {
      throw new ArgumentException(
          "An AI ordering edge cannot name the same registration on both sides.",
          nameof(afterKey));
    }
  }

  /// <summary>The earlier registration key.</summary>
  public string BeforeKey { get; }

  /// <summary>The later registration key.</summary>
  public string AfterKey { get; }
}

/// <summary>
/// Composition-root catalog of NPC AI behaviors. It carries both halves of one declaration: how the
/// runtime is built, and what it is allowed to emit.
/// </summary>
public sealed class NpcAiCatalog
{
  private readonly List<NpcAiBehaviorDeclaration> _ordered = [];
  private readonly Dictionary<Type, NpcAiBehaviorDeclaration> _byType = [];

  public NpcAiCatalog()
  {
    Declarations = _ordered.AsReadOnly();
  }

  /// <summary>Declarations in addition order; a live view of the catalog contents.</summary>
  public IReadOnlyList<NpcAiBehaviorDeclaration> Declarations { get; }

  public bool IsSealed { get; private set; }

  public NpcAiCatalog Add(NpcAiBehaviorDeclaration declaration)
  {
    ArgumentNullException.ThrowIfNull(declaration);
    if (IsSealed)
    {
      throw new InvalidOperationException("The NPC AI catalog is sealed.");
    }

    Type behaviorType = declaration.Behavior.BehaviorType;
    if (!_byType.TryAdd(behaviorType, declaration))
    {
      throw new InvalidOperationException($"AI behavior {behaviorType.FullName} is declared twice.");
    }

    _ordered.Add(declaration);
    return this;
  }

  public void Seal()
  {
    IsSealed = true;
  }

  /// <summary>
  /// Every component any declared behavior commits to, in declaration order. This is the whole
  /// write set of the NPC AI domain, derived from the commit table rather than restated.
  /// </summary>
  public IReadOnlyList<Type> DeclaredCommits()
  {
    List<Type> targets = [];
    foreach (NpcAiBehaviorDeclaration declaration in _ordered)
    {
      foreach (Type target in declaration.Commits)
      {
        if (!targets.Contains(target))
        {
          targets.Add(target);
        }
      }
    }

    return targets.AsReadOnly();
  }

  /// <summary>
  /// Rejects a declaration set in which one registration is required to run both before and after
  /// the same key: with a single AI registration, per-declaration anchor checks cannot see that
  /// conflict, but the schedule would be unsatisfiable.
  /// </summary>
  /// <remarks>
  /// Anchors are only meaningful relative to the registration that carries them, so this check is
  /// deliberately expressed on the generated edges rather than on the anchor lists: two behaviors
  /// with contradictory anchors are fine as long as they land on different registrations.
  /// </remarks>
  public void EnsureConsistent(AiAnchorBindings bindings, string aiRegistrationKey)
  {
    ArgumentNullException.ThrowIfNull(bindings);
    IReadOnlyList<NpcAiOrderingEdge> edges = Edges(bindings, aiRegistrationKey);
    foreach (NpcAiOrderingEdge forward in edges)
    {
      foreach (NpcAiOrderingEdge backward in edges)
      {
        if (string.Equals(forward.BeforeKey, backward.AfterKey, StringComparison.Ordinal) &&
            string.Equals(forward.AfterKey, backward.BeforeKey, StringComparison.Ordinal))
        {
          throw new InvalidOperationException(
              $"AI declarations require {forward.BeforeKey} both before and after " +
              $"{forward.AfterKey}; the schedule cannot satisfy both.");
        }
      }
    }
  }

  /// <summary>
  /// The schedule edges the declarations imply, resolved through the host binding table.
  /// </summary>
  /// <remarks>
  /// Edges are derived, never typed by hand: a declaration names capability roles, this method
  /// resolves them to the keys the host actually registered. Guide and Slime share one
  /// registration, so their identical anchors collapse into one edge instead of producing the same
  /// constraint twice.
  /// </remarks>
  public IReadOnlyList<NpcAiOrderingEdge> Edges(AiAnchorBindings bindings, string aiRegistrationKey)
  {
    ArgumentNullException.ThrowIfNull(bindings);
    if (string.IsNullOrWhiteSpace(aiRegistrationKey) || aiRegistrationKey != aiRegistrationKey.Trim())
    {
      throw new ArgumentException(
          "The AI registration key is required to generate ordering edges.",
          nameof(aiRegistrationKey));
    }

    List<NpcAiOrderingEdge> edges = [];
    HashSet<(string, string)> emitted = [];
    foreach (NpcAiBehaviorDeclaration declaration in _ordered)
    {
      foreach (AiAnchor anchor in declaration.After)
      {
        Add(edges, emitted, bindings.KeyOf(anchor), aiRegistrationKey);
      }

      foreach (AiAnchor anchor in declaration.Before)
      {
        Add(edges, emitted, aiRegistrationKey, bindings.KeyOf(anchor));
      }
    }

    return edges.AsReadOnly();
  }

  /// <summary>
  /// Projects <see cref="Edges"/> into the same rule shape the declarative rule file uses, so the
  /// generated contract and the hand-written one are validated by one mechanism.
  /// </summary>
  public IReadOnlyList<EcsCoordinationRule> OrderingRules(
      AiAnchorBindings bindings,
      string aiRegistrationKey)
  {
    List<EcsCoordinationRule> rules = [];
    foreach (NpcAiOrderingEdge edge in Edges(bindings, aiRegistrationKey))
    {
      rules.Add(EcsCoordinationRule.Before(edge.BeforeKey, edge.AfterKey));
    }

    return rules.AsReadOnly();
  }

  private static void Add(
      List<NpcAiOrderingEdge> edges,
      HashSet<(string, string)> emitted,
      string beforeKey,
      string afterKey)
  {
    if (emitted.Add((beforeKey, afterKey)))
    {
      edges.Add(new NpcAiOrderingEdge(beforeKey, afterKey));
    }
  }

  /// <summary>Every intent kind any declared behavior may emit, in declaration order.</summary>
  public IReadOnlyList<NpcAiIntentKind> DeclaredEmits()
  {
    List<NpcAiIntentKind> kinds = [];
    HashSet<NpcAiIntentKind> seen = [];
    foreach (NpcAiBehaviorDeclaration declaration in _ordered)
    {
      foreach (NpcAiIntentKind kind in declaration.Emits)
      {
        if (seen.Add(kind))
        {
          kinds.Add(kind);
        }
      }
    }

    return kinds.AsReadOnly();
  }
}
