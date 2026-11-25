using System;
using System.Collections.Generic;
using Arch.Core;
using Arch.System;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Ai;
using Terraria.Share.Combat;
using Terraria.Share.Entities;
using Terraria.Share.Movement;
using Terraria.Share.Physics;
using Terraria.Share.Relationships;

namespace Terraria.Unshared.Npc;

/// <summary>
/// Converts validated shared AI output into NPC behavior state and one-step movement intents.
/// </summary>
/// <remarks>
/// This adapter is not an NPC AI engine. It never writes location, velocity, life, attributes or
/// Buff state; those remain owned by spatial, combat, attribute and Buff systems.
/// <para>
/// AiBehaviorSelectionComponent stays in the query filter as part of the composition contract: an
/// entity opts into NPC AI by carrying it, so the archetype remains the admission gate even though
/// this adapter reads no field from it.
/// </para>
/// </remarks>
public sealed class NpcAiIntentAdapter : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly AiEntitySystem<ArchEntity, NpcAiIntent>? _ai;
  private readonly AiRequirements? _manualRequirements;
  private readonly Func<
      AiComponent,
      NpcBehaviorStateComponent,
      AiSnapshot,
      float,
      IReadOnlyList<NpcAiIntent>>? _manualCalculator;
  private readonly IAiSnapshotProvider<ArchEntity> _snapshots;
  private readonly IEntityRuntimeHandleResolver _runtimeHandles;
  private readonly CombatAttackCatalog _attackDefinitions;
  private readonly QueryDescription _query;

  public NpcAiIntentAdapter(
      ArchWorld world,
      AiEntitySystem<ArchEntity, NpcAiIntent> ai,
      IAiSnapshotProvider<ArchEntity> snapshots,
      IEntityRuntimeHandleResolver runtimeHandles,
      CombatAttackCatalog attackDefinitions,
      NpcAiCatalog catalog)
      : this(
          world,
          ai ?? throw new ArgumentNullException(nameof(ai)),
          snapshots,
          runtimeHandles,
          attackDefinitions,
          manualRequirements: null,
          manualCalculator: null)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    // 提交表必须覆盖每个 kind 且只写允许的组件；声明说能发的 kind 必须都有行。
    NpcAiIntentCommitTable.EnsureComplete();
    foreach (NpcAiBehaviorDeclaration declaration in catalog.Declarations)
    {
      NpcAiIntentCommitTable.EnsureDispatches(
          declaration.Emits, declaration.Behavior.BehaviorType);
    }
  }

  /// <summary>Creates the NPC adapter with a direct calculator instead of the shared AI runtime.</summary>
  public NpcAiIntentAdapter(
      ArchWorld world,
      IAiSnapshotProvider<ArchEntity> snapshots,
      IEntityRuntimeHandleResolver runtimeHandles,
      CombatAttackCatalog attackDefinitions,
      AiRequirements manualRequirements,
      Func<
          AiComponent,
          NpcBehaviorStateComponent,
          AiSnapshot,
          float,
          IReadOnlyList<NpcAiIntent>> manualCalculator)
      : this(
          world,
          ai: null,
          snapshots,
          runtimeHandles,
          attackDefinitions,
          manualRequirements,
          manualCalculator)
  {
  }

  private NpcAiIntentAdapter(
      ArchWorld world,
      AiEntitySystem<ArchEntity, NpcAiIntent>? ai,
      IAiSnapshotProvider<ArchEntity> snapshots,
      IEntityRuntimeHandleResolver runtimeHandles,
      CombatAttackCatalog attackDefinitions,
      AiRequirements? manualRequirements,
      Func<
          AiComponent,
          NpcBehaviorStateComponent,
          AiSnapshot,
          float,
          IReadOnlyList<NpcAiIntent>>? manualCalculator)
      : base(world)
  {
    if ((ai is null) == (manualCalculator is null))
    {
      throw new ArgumentException(
          "Provide either the shared AI runtime or a direct manual calculator.");
    }

    if ((manualRequirements is null) != (manualCalculator is null))
    {
      throw new ArgumentException(
          "Manual AI requirements and calculator must be provided together.");
    }

    ArgumentNullException.ThrowIfNull(snapshots);
    _ai = ai;
    _manualRequirements = manualRequirements;
    _manualCalculator = manualCalculator;
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
    AiEntitySystem<ArchEntity, NpcAiIntent>? ai = _ai;
    AiRequirements? manualRequirements = _manualRequirements;
    Func<
        AiComponent,
        NpcBehaviorStateComponent,
        AiSnapshot,
        float,
        IReadOnlyList<NpcAiIntent>>? manualCalculator = _manualCalculator;
    IAiSnapshotProvider<ArchEntity> snapshots = _snapshots;
    // 在查询之外解析队列实体：Require 内部本身要跑一次查询，放进 Query 回调会形成嵌套查询。
    CombatAttackIntentQueueComponent attackIntents = CombatAttackIntentQueue.Require(World);
    MovementIntentRequestQueueComponent movementIntents = MovementIntentRequestQueue.Require(World);
    world.Query(
        in _query,
        (ArchEntity entity,
            ref AiComponent aiComponent,
            ref NpcBehaviorStateComponent state,
            ref MovementIntent movementIntent) =>
        {
          if (!aiComponent.Enabled)
          {
            return;
          }

          _ = _runtimeHandles.TryGetHandle(entity, out EntityRuntimeHandle entityHandle);
          IReadOnlyList<NpcAiIntent> intents;
          if (manualCalculator is not null)
          {
            AiRequirements requirements = manualRequirements ??
                throw new InvalidOperationException("Manual AI requirements were not configured.");
            AiSnapshot snapshot = snapshots.Capture(entity, requirements.Components);
            intents = manualCalculator(aiComponent, state, snapshot, deltaTime);
          }
          else
          {
            if (ai is null || !ai.IsAttached(entity))
            {
              return;
            }

            AiEntityResult<ArchEntity, NpcAiIntent> result = ai.Calculate(
                entity,
                in aiComponent,
                snapshots,
                deltaTime);
            intents = result.Result.Intents;
          }

          ArgumentNullException.ThrowIfNull(intents);
          foreach (NpcAiIntent intent in intents)
          {
            // 提交目标与写法都来自表：这里没有 switch，新增 kind 不可能绕过构造期校验。
            NpcAiIntentCommit commit = NpcAiIntentCommitTable.Get(intent.Kind);
            NpcAiCommitContext commitContext = new(
                entity,
                entityHandle,
                tick,
                attackIntents,
                movementIntents,
                _runtimeHandles,
                _attackDefinitions,
                ref state);
            commit.Handler(ref commitContext, in intent);
          }
        });
  }
}
