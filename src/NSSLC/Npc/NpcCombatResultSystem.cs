using System;
using System.Collections.Generic;
using Arch.Core;
using Arch.System;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Combat;
using Terraria.Entity;
using Terraria.Relationships;

namespace Terraria.Npc;

/// <summary>Consumes every NPC-owned Combat result without a single-result entity slot.</summary>
public sealed class NpcCombatResultSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly IEntityRuntimeHandleResolver _handles;
  private readonly QueryDescription _query = new QueryDescription().WithAll<
      NpcBehaviorStateComponent>();

  public NpcCombatResultSystem(
      ArchWorld world,
      IEntityRuntimeHandleResolver handles)
      : base(world ?? throw new ArgumentNullException(nameof(world)))
  {
    _handles = handles ?? throw new ArgumentNullException(nameof(handles));
  }

  public override void Update(in EcsServerTickContext context)
  {
    long tick = context.Tick;
    CombatDamageResultQueueComponent resultQueue = CombatDamageResultQueue.Require(World);
    World.Query(
        in _query,
        (ArchEntity entity, ref NpcBehaviorStateComponent state) =>
        {
          IReadOnlyList<CombatDamageResult> results =
              CombatResultDrain.Drain(resultQueue, _handles, entity);
          for (int index = 0; index < results.Count; index++)
          {
            if (results[index].Result.Applied)
            {
              state = state.WithDecision(
                  NpcBehaviorPhase.Attack,
                  tick,
                  state.DecisionCooldown,
                  state.ActionPhase);
            }
          }
        });
  }
}
