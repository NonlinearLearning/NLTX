using System;
using Arch.Core;
using Arch.System;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Ai;
using Terraria.Combat;
using Terraria.Entity;
using Terraria.Movement;
using Terraria.Physics;
using Terraria.Relationships;

namespace Terraria.Npc;

/// <summary>
/// Converts validated shared AI output into NPC behavior state and one-step movement intents.
/// </summary>
/// <remarks>
/// This adapter is not an NPC AI engine. It never writes location, velocity, life, attributes or
/// Buff state; those remain owned by spatial, combat, attribute and Buff systems.
/// </remarks>
public sealed class NpcAiIntentAdapter : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly AiEntitySystem<ArchEntity, NpcAiIntent> _ai;
  private readonly IAiSnapshotProvider<ArchEntity> _snapshots;
  private readonly IEntityRuntimeHandleResolver _runtimeHandles;
  private readonly CombatAttackDefinitionCatalog _attackDefinitions;
  private readonly QueryDescription _query;

  public NpcAiIntentAdapter(
      ArchWorld world,
      AiEntitySystem<ArchEntity, NpcAiIntent> ai,
      IAiSnapshotProvider<ArchEntity> snapshots,
      IEntityRuntimeHandleResolver runtimeHandles,
      CombatAttackDefinitionCatalog attackDefinitions)
      : base(world)
  {
    ArgumentNullException.ThrowIfNull(ai);
    ArgumentNullException.ThrowIfNull(snapshots);
    _ai = ai;
    _snapshots = snapshots;
    _runtimeHandles = runtimeHandles ?? throw new ArgumentNullException(nameof(runtimeHandles));
    _attackDefinitions = attackDefinitions ??
        throw new ArgumentNullException(nameof(attackDefinitions));
    _query = new QueryDescription()
        .WithAll<
            AiComponent,
            AiBehaviorSelectionComponent,
            NpcBehaviorStateComponent,
            MovementIntent>();
  }

  public override void Update(in EcsServerTickContext context)
  {
    long tick = context.Tick;
    float deltaTime = context.DeltaTime;
    ArchWorld world = context.World;
    AiEntitySystem<ArchEntity, NpcAiIntent> ai = _ai;
    IAiSnapshotProvider<ArchEntity> snapshots = _snapshots;
    // 在查询之外解析队列实体：Require 内部本身要跑一次查询，放进 Query 回调会形成嵌套查询。
    CombatAttackIntentQueueComponent attackIntents = CombatAttackIntentQueue.Require(World);
    MovementIntentRequestQueueComponent movementIntents = MovementIntentRequestQueue.Require(World);
    world.Query(
        in _query,
        (ArchEntity entity,
            ref AiComponent aiComponent,
            ref AiBehaviorSelectionComponent behavior,
            ref NpcBehaviorStateComponent state,
            ref MovementIntent movementIntent) =>
        {
          if (!aiComponent.Enabled || !ai.IsAttached(entity))
          {
            return;
          }

          _ = _runtimeHandles.TryGetHandle(entity, out EntityRuntimeHandle entityHandle);
          AiEntityResult<ArchEntity, NpcAiIntent> result = ai.Calculate(
              entity,
              in aiComponent,
              snapshots,
              deltaTime);
          foreach (NpcAiIntent intent in result.Result.Intents)
          {
            ApplyIntent(
                entity,
                entityHandle,
                tick,
                intent,
                attackIntents,
                movementIntents,
                ref state);
          }
        });
  }

  private void ApplyIntent(
      ArchEntity entity,
      EntityRuntimeHandle entityHandle,
      long tick,
      NpcAiIntent intent,
      CombatAttackIntentQueueComponent attackIntents,
      MovementIntentRequestQueueComponent movementIntents,
      ref NpcBehaviorStateComponent state)
  {
    switch (intent.Kind)
    {
      case NpcAiIntentKind.SetBehaviorState:
        state = state.WithDecision(
            intent.Phase,
            tick,
            state.DecisionCooldown,
            intent.ActionPhase,
            intent.StateSlot0,
            intent.StateSlot1);
        break;
      case NpcAiIntentKind.BeginCombatAction:
        state = state.WithDecision(
            intent.Phase,
            tick,
            state.DecisionCooldown,
            intent.ActionPhase,
            intent.StateSlot0,
            intent.StateSlot1);
        if (intent.Phase == NpcBehaviorPhase.Attack)
        {
          TryQueueAttack(entity, tick, intent, attackIntents);
        }
        break;
      case NpcAiIntentKind.CancelCombatAction:
        state = state.WithDecision(
            intent.Phase,
            tick,
            state.DecisionCooldown,
            intent.ActionPhase,
            intent.StateSlot0,
            intent.StateSlot1);
        break;
      case NpcAiIntentKind.Move:
        // MovementIntent 归 Movement 域所有：这里只提交意图。
        movementIntents.Enqueue(new MovementIntentRequest(
            entityHandle,
            new MovementIntent(
                intent.DesiredVelocity,
                tick,
                jumpRequested: intent.JumpRequested,
                source: MovementIntentSource.Ai)));
        break;
      case NpcAiIntentKind.StopMovement:
        movementIntents.Enqueue(new MovementIntentRequest(
            entityHandle,
            MovementIntent.Stop(tick)));
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(intent.Kind));
    }
  }

  private void TryQueueAttack(
      ArchEntity source,
      long tick,
      NpcAiIntent intent,
      CombatAttackIntentQueueComponent attackIntents)
  {
    if (!intent.AttackTarget.IsValid || intent.AttackDefinitionKey == 0 ||
        !_attackDefinitions.TryGet(intent.AttackDefinitionKey, out CombatAttackDefinition? definition))
    {
      return;
    }

    if (!_runtimeHandles.TryGetHandle(source, out EntityRuntimeHandle sourceHandle))
    {
      return;
    }

    attackIntents.Enqueue(
        sourceHandle,
        intent.AttackTarget,
        definition.Id,
        tick,
        DamageCause.NpcAttack);
  }

}
