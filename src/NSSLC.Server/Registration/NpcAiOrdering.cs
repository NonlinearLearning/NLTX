using System;
using System.Collections.Generic;
using Terraria.Share.Ai;
using Terraria.Share.Entities;
using Terraria.Unshared.Npc;

namespace Terraria.Server;

/// <summary>
/// The AI ordering the host derives from declarations: anchor bindings, the schedule edges they
/// resolve to, and the coordination rules that make the same claim checkable.
/// </summary>
/// <remarks>
/// Only the composition root knows both vocabularies. Declarations name capability roles because
/// registration keys are `internal` to this assembly; the schedule knows keys but nothing about
/// intents. This type is the seam where the two meet, and it is built once, before any
/// registration is created.
/// </remarks>
internal sealed class NpcAiOrdering
{
  private NpcAiOrdering(
      AiAnchorBindings bindings,
      IReadOnlyList<NpcAiOrderingEdge> edges,
      IReadOnlyList<EcsCoordinationRule> rules)
  {
    Bindings = bindings;
    Edges = edges;
    Rules = rules;
  }

  /// <summary>Anchor-to-registration-key table used to resolve declarations.</summary>
  internal AiAnchorBindings Bindings { get; }

  /// <summary>The schedule edges the declarations imply.</summary>
  internal IReadOnlyList<NpcAiOrderingEdge> Edges { get; }

  /// <summary>The same edges as coordination rules, for the host rule set.</summary>
  internal IReadOnlyList<EcsCoordinationRule> Rules { get; }

  /// <summary>
  /// Builds the ordering for the NPC AI domain, binding every anchor and projecting the
  /// declarations of <paramref name="catalog"/> into edges.
  /// </summary>
  internal static NpcAiOrdering Create(NpcAiCatalog catalog)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    AiAnchorBindings bindings = AiAnchorBindings.Create(new Dictionary<AiAnchor, string>
    {
      // 外部输入已经落成领域状态。
      [AiAnchor.InputApply] = EcsServerRegistrationKeys.PlayerInputApply,
      // 移动意图已经装入组件，空间求解读的就是它。
      [AiAnchor.MovementCommit] = EcsServerRegistrationKeys.MovementIntentApply
    });
    // 契约成对时才能用：同一条注册被要求既在某键之前又在某键之后，调度无法满足。
    catalog.EnsureConsistent(bindings, EcsServerRegistrationKeys.NpcAiIntent);
    return new NpcAiOrdering(
        bindings,
        catalog.Edges(bindings, EcsServerRegistrationKeys.NpcAiIntent),
        catalog.OrderingRules(bindings, EcsServerRegistrationKeys.NpcAiIntent));
  }

  /// <summary>
  /// The generated predecessors of one registration key, so a registration's `after` list is
  /// derived from AI declarations instead of restating them by hand.
  /// </summary>
  internal string[] AfterKeysOf(string registrationKey)
  {
    List<string> keys = [];
    foreach (NpcAiOrderingEdge edge in Edges)
    {
      if (string.Equals(edge.AfterKey, registrationKey, StringComparison.Ordinal))
      {
        keys.Add(edge.BeforeKey);
      }
    }

    return [.. keys];
  }

  /// <summary>Fails when a bound key names no registration in the built schedule.</summary>
  /// <remarks>
  /// Run against <see cref="EcsServer.OrderedRegistrationKeys"/> rather than a copy of the key
  /// list: a registration that is renamed or dropped then breaks the next startup instead of
  /// silently dropping an ordering constraint.
  /// </remarks>
  internal void EnsureResolvable(IReadOnlyList<string> registrationKeys)
  {
    Bindings.EnsureResolvable(registrationKeys);
  }
}
