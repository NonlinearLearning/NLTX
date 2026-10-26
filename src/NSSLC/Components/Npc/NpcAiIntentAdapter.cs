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
  private readonly EcsServerCommandBuffer _commands;
  private readonly CombatAttackIntentBuffer _attackIntents;
  private readonly IEntityRuntimeHandleResolver _runtimeHandles;
  private readonly CombatAttackDefinitionCatalog _attackDefinitions;
  private readonly QueryDescription _query;

  public NpcAiIntentAdapter(
      ArchWorld world,
      AiEntitySystem<ArchEntity, NpcAiIntent> ai,
      IAiSnapshotProvider<ArchEntity> snapshots,
      EcsServerCommandBuffer commands,
      CombatAttackIntentBuffer attackIntents,
      IEntityRuntimeHandleResolver runtimeHandles,
      CombatAttackDefinitionCatalog attackDefinitions)
      : base(world)
  {
    ArgumentNullException.ThrowIfNull(ai);
    ArgumentNullException.ThrowIfNull(snapshots);
    ArgumentNullException.ThrowIfNull(commands);
    _ai = ai;
    _snapshots = snapshots;
    _commands = commands;
    _attackIntents = attackIntents ?? throw new ArgumentNullException(nameof(attackIntents));
    _runtimeHandles = runtimeHandles ?? throw new ArgumentNullException(nameof(runtimeHandles));
    _attackDefinitions = attackDefinitions ??
        throw new ArgumentNullException(nameof(attackDefinitions));
    _query = new QueryDescription()
        .WithAll<AiComponent, AiBehaviorSelectionComponent, NpcBehaviorStateComponent>();
  }

  public override void Update(in EcsServerTickContext context)
  {
    long tick = context.Tick;
    float deltaTime = context.DeltaTime;
    ArchWorld world = context.World;
    AiEntitySystem<ArchEntity, NpcAiIntent> ai = _ai;
    IAiSnapshotProvider<ArchEntity> snapshots = _snapshots;
    EcsServerCommandBuffer commands = _commands;
    world.Query(
        in _query,
        (ArchEntity entity,
            ref AiComponent aiComponent,
            ref AiBehaviorSelectionComponent behavior,
            ref NpcBehaviorStateComponent state) =>
        {
          if (!aiComponent.Enabled || !ai.IsAttached(entity))
          {
            return;
          }

          AiEntityResult<ArchEntity, NpcAiIntent> result = ai.Calculate(
              entity,
              in aiComponent,
              snapshots,
              deltaTime);
          foreach (NpcAiIntent intent in result.Result.Intents)
          {
            ApplyIntent(entity, tick, intent, ref state, commands);
          }
        });
  }

  private void ApplyIntent(
      ArchEntity entity,
      long tick,
      NpcAiIntent intent,
      ref NpcBehaviorStateComponent state,
      EcsServerCommandBuffer commands)
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
          TryQueueAttack(entity, tick, intent);
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
        MovementIntent movementIntent = new MovementIntent(
            intent.DesiredVelocity,
            tick,
            source: MovementIntentSource.Ai);
        SetMovementIntent(entity, movementIntent, commands);
        break;
      case NpcAiIntentKind.StopMovement:
        SetMovementIntent(entity, MovementIntent.Stop(tick), commands);
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(intent.Kind));
    }
  }

  private void TryQueueAttack(ArchEntity source, long tick, NpcAiIntent intent)
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

    _attackIntents.Enqueue(
        sourceHandle,
        intent.AttackTarget,
        definition.Id,
        tick,
        DamageCause.NpcAttack);
  }

  private void SetMovementIntent(
      ArchEntity entity,
      MovementIntent movementIntent,
      EcsServerCommandBuffer commands)
  {
    if (World.Has<MovementIntent>(entity))
    {
      ref MovementIntent currentIntent = ref World.Get<MovementIntent>(entity);
      currentIntent = movementIntent;
      return;
    }

    commands.Add(entity, movementIntent);
  }
}
