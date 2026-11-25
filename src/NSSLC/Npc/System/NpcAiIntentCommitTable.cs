using System;
using System.Collections.Generic;
using ArchEntity = Arch.Core.Entity;
using Terraria.Share.Combat;
using Terraria.Share.Entities;
using Terraria.Share.Movement;
using Terraria.Share.Relationships;

namespace Terraria.Unshared.Npc;

/// <summary>Everything one intent commit is allowed to touch, resolved per entity per tick.</summary>
/// <remarks>
/// The context is not a service locator: it carries exactly the two request queues the AI may
/// append to, the resolver it needs to name an entity, and a reference to the NPC's own behavior
/// state. Nothing else can be reached from a commit, so a new commit cannot quietly start writing
/// another domain's state.
/// </remarks>
public ref struct NpcAiCommitContext
{
  public NpcAiCommitContext(
      ArchEntity entity,
      EntityRuntimeHandle handle,
      long tick,
      CombatAttackIntentQueueComponent attackIntents,
      MovementIntentRequestQueueComponent movementIntents,
      IEntityRuntimeHandleResolver runtimeHandles,
      CombatAttackCatalog attackDefinitions,
      ref NpcBehaviorStateComponent state)
  {
    ArgumentNullException.ThrowIfNull(attackIntents);
    ArgumentNullException.ThrowIfNull(movementIntents);
    ArgumentNullException.ThrowIfNull(runtimeHandles);
    ArgumentNullException.ThrowIfNull(attackDefinitions);
    Entity = entity;
    Handle = handle;
    Tick = tick;
    AttackIntents = attackIntents;
    MovementIntents = movementIntents;
    RuntimeHandles = runtimeHandles;
    AttackDefinitions = attackDefinitions;
    State = ref state;
  }

  /// <summary>The entity the intents belong to; used to resolve the source handle.</summary>
  public ArchEntity Entity { get; }

  /// <summary>The resolved runtime handle of <see cref="Entity"/>.</summary>
  public EntityRuntimeHandle Handle { get; }

  /// <summary>Tick the intents are committed on.</summary>
  public long Tick { get; }

  /// <summary>Combat's attack intent queue; the only Combat-facing write a commit may perform.</summary>
  public CombatAttackIntentQueueComponent AttackIntents { get; }

  /// <summary>Movement's request queue; the only Movement-facing write a commit may perform.</summary>
  public MovementIntentRequestQueueComponent MovementIntents { get; }

  public IEntityRuntimeHandleResolver RuntimeHandles { get; }

  public CombatAttackCatalog AttackDefinitions { get; }

  /// <summary>The NPC's own behavior state; written in place, never queued.</summary>
  public ref NpcBehaviorStateComponent State;
}

/// <summary>Performs one intent's write. The only code that runs inside a commit.</summary>
public delegate void NpcAiCommitHandler(ref NpcAiCommitContext context, in NpcAiIntent intent);

/// <summary>One row of the commit table: an intent kind, the component it commits to, and how.</summary>
public sealed class NpcAiIntentCommit
{
  public NpcAiIntentCommit(NpcAiIntentKind kind, Type target, NpcAiCommitHandler handler)
  {
    ArgumentNullException.ThrowIfNull(target);
    ArgumentNullException.ThrowIfNull(handler);
    Kind = kind;
    Target = target;
    Handler = handler;
  }

  /// <summary>The intent kind this row handles.</summary>
  public NpcAiIntentKind Kind { get; }

  /// <summary>
  /// The component this kind commits to: the NPC's own behavior state, or one of the
  /// <c>*RequestComponent</c> / <c>*IntentQueueComponent</c> queues it is allowed to append to.
  /// </summary>
  public Type Target { get; }

  /// <summary>The write itself.</summary>
  public NpcAiCommitHandler Handler { get; }
}

/// <summary>
/// The table that replaces <c>ApplyIntent</c>'s switch: which component each intent kind commits
/// to, and the code that performs it.
/// </summary>
/// <remarks>
/// The switch used to be the only place the kind-to-target mapping existed, so a new kind could be
/// added without any construction-time check. Now the mapping is data: <see cref="EnsureComplete"/>
/// proves every enum member has exactly one row, and every row names a component the AI is allowed
/// to touch. A kind that would write another domain's state, or a target that is not a request or
/// intent queue, fails while the server starts.
/// </remarks>
public static class NpcAiIntentCommitTable
{
  /// <summary>
  /// Every component a commit is allowed to name. The AI writes its own behavior state and appends
  /// to request/intent queues; anything else would be a cross-domain write, which the AI boundary
  /// forbids by design.
  /// </summary>
  public static IReadOnlyList<Type> AllowedTargets { get; } = Array.AsReadOnly(new[]
  {
    typeof(NpcBehaviorStateComponent),
    typeof(MovementIntentRequestQueueComponent),
    typeof(CombatAttackIntentQueueComponent)
  });

  private static readonly NpcAiIntentCommit[] Entries =
  [
    // 行为状态由 AI 自己拥有，就地改写。
    new NpcAiIntentCommit(
        NpcAiIntentKind.SetBehaviorState, typeof(NpcBehaviorStateComponent), CommitBehaviorState),
    // 攻击请求只入队，伤害计算留在 Combat。
    new NpcAiIntentCommit(
        NpcAiIntentKind.BeginCombatAction,
        typeof(CombatAttackIntentQueueComponent),
        CommitBeginCombatAction),
    new NpcAiIntentCommit(
        NpcAiIntentKind.CancelCombatAction, typeof(NpcBehaviorStateComponent), CommitBehaviorState),
    // 移动意图只入队，Location/Velocity 仍只有空间协调系统写。
    new NpcAiIntentCommit(
        NpcAiIntentKind.Move, typeof(MovementIntentRequestQueueComponent), CommitMove),
    new NpcAiIntentCommit(
        NpcAiIntentKind.StopMovement,
        typeof(MovementIntentRequestQueueComponent),
        CommitStopMovement)
  ];

  private static readonly Dictionary<NpcAiIntentKind, NpcAiIntentCommit> ByKind = BuildIndex();

  /// <summary>Every row, in table order.</summary>
  public static IReadOnlyList<NpcAiIntentCommit> Commits { get; } = Array.AsReadOnly(Entries);

  /// <summary>Resolves one intent kind to its row; an unhandled kind is a construction defect.</summary>
  public static NpcAiIntentCommit Get(NpcAiIntentKind kind)
  {
    if (!ByKind.TryGetValue(kind, out NpcAiIntentCommit? commit))
    {
      throw new KeyNotFoundException(
          $"The NPC AI commit table has no row for {kind}; EnsureComplete should have caught this.");
    }

    return commit;
  }

  /// <summary>
  /// Requires the table to cover every declared intent kind exactly once and every row to name a
  /// component the AI may write. Called from the adapter's constructor, so the check runs before
  /// the first tick rather than on the tick that happens to use the missing kind.
  /// </summary>
  public static void EnsureComplete()
  {
    List<string> violations = [];
    HashSet<NpcAiIntentKind> seen = [];
    foreach (NpcAiIntentCommit commit in Entries)
    {
      if (!Enum.IsDefined(commit.Kind))
      {
        violations.Add($"row names undefined intent kind {commit.Kind}");
        continue;
      }

      if (!seen.Add(commit.Kind))
      {
        violations.Add($"{commit.Kind} is declared twice");
      }

      if (!IsAllowedTarget(commit.Target))
      {
        violations.Add(
            $"{commit.Kind} commits to {commit.Target.Name}, which is not the AI's own behavior " +
            "state or a request/intent queue");
      }
    }

    foreach (NpcAiIntentKind kind in Enum.GetValues<NpcAiIntentKind>())
    {
      if (!seen.Contains(kind))
      {
        violations.Add($"{kind} has no commit row");
      }
    }

    if (violations.Count != 0)
    {
      throw new InvalidOperationException(
          "The NPC AI commit table is incomplete: " + string.Join("; ", violations));
    }
  }

  /// <summary>
  /// Requires every kind a declaration may emit to have a row. Called from the adapter, so a
  /// catalog claiming an uncommittable kind cannot be wired into a running host.
  /// </summary>
  public static void EnsureDispatches(IReadOnlyList<NpcAiIntentKind> emits, Type behaviorType)
  {
    ArgumentNullException.ThrowIfNull(emits);
    ArgumentNullException.ThrowIfNull(behaviorType);
    foreach (NpcAiIntentKind kind in emits)
    {
      if (!ByKind.ContainsKey(kind))
      {
        throw new InvalidOperationException(
            $"{behaviorType.FullName} declares it can emit {kind}, but the NPC AI commit table " +
            "has no row for that kind.");
      }
    }
  }

  /// <summary>
  /// Projects a set of emitted kinds onto the components they commit to, in first-emission order.
  /// This is what a declaration's <c>Commits</c> is: derived from the table, never typed by hand.
  /// </summary>
  public static IReadOnlyList<Type> TargetsOf(IReadOnlyList<NpcAiIntentKind> emits)
  {
    ArgumentNullException.ThrowIfNull(emits);
    List<Type> targets = [];
    foreach (NpcAiIntentKind kind in emits)
    {
      Type target = Get(kind).Target;
      if (!targets.Contains(target))
      {
        targets.Add(target);
      }
    }

    return targets.AsReadOnly();
  }

  private static bool IsAllowedTarget(Type target)
  {
    for (int i = 0; i < AllowedTargets.Count; i++)
    {
      if (AllowedTargets[i] == target)
      {
        return true;
      }
    }

    return false;
  }

  private static Dictionary<NpcAiIntentKind, NpcAiIntentCommit> BuildIndex()
  {
    EnsureComplete();
    Dictionary<NpcAiIntentKind, NpcAiIntentCommit> index = [];
    foreach (NpcAiIntentCommit commit in Entries)
    {
      index.Add(commit.Kind, commit);
    }

    return index;
  }

  private static void CommitBehaviorState(ref NpcAiCommitContext context, in NpcAiIntent intent)
  {
    context.State = context.State.WithDecision(
        intent.Phase,
        context.Tick,
        context.State.DecisionCooldown,
        intent.ActionPhase,
        intent.StateSlot0,
        intent.StateSlot1);
  }

  private static void CommitBeginCombatAction(ref NpcAiCommitContext context, in NpcAiIntent intent)
  {
    CommitBehaviorState(ref context, in intent);
    if (intent.Phase == NpcBehaviorPhase.Attack)
    {
      TryQueueAttack(ref context, in intent);
    }
  }

  private static void CommitMove(ref NpcAiCommitContext context, in NpcAiIntent intent)
  {
    context.MovementIntents.Enqueue(new MovementIntentRequest(
        context.Handle,
        new MovementIntent(
            intent.DesiredVelocity,
            context.Tick,
            jumpRequested: intent.JumpRequested,
            source: MovementIntentSource.Ai,
            jumpSpeed: intent.JumpSpeed)));
  }

  private static void CommitStopMovement(ref NpcAiCommitContext context, in NpcAiIntent intent)
  {
    context.MovementIntents.Enqueue(new MovementIntentRequest(
        context.Handle,
        MovementIntent.Stop(context.Tick)));
  }

  private static void TryQueueAttack(ref NpcAiCommitContext context, in NpcAiIntent intent)
  {
    if (!intent.AttackTarget.IsValid || intent.AttackDefinitionKey == 0 ||
        !context.AttackDefinitions.TryGet(intent.AttackDefinitionKey, out CombatAttack? definition))
    {
      return;
    }

    if (!context.RuntimeHandles.TryGetHandle(context.Entity, out EntityRuntimeHandle sourceHandle))
    {
      return;
    }

    context.AttackIntents.Enqueue(
        sourceHandle,
        intent.AttackTarget,
        definition.Id,
        context.Tick,
        DamageCause.NpcAttack);
  }
}
