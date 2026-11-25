using System;
using Arch.Core;
using Arch.System;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Share.Combat;
using Terraria.Share.Entities;
using Terraria.Share.Relationships;

namespace Terraria.Share.Attributes;

/// <summary>
/// Converts periodic behavior components into Life and Combat request components.
/// </summary>
public sealed class StatusEffectPeriodicSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly IEntityRuntimeHandleResolver _handles;
  private readonly QueryDescription _healingQuery = new QueryDescription()
      .WithAll<LifeRegenerationComponent, LifeComponent>();
  private readonly QueryDescription _damageQuery = new QueryDescription()
      .WithAll<DamageOverTimeComponent, LifeComponent>();

  public StatusEffectPeriodicSystem(
      ArchWorld world,
      IEntityRuntimeHandleResolver handles)
      : base(world)
  {
    _handles = handles ?? throw new ArgumentNullException(nameof(handles));
  }

  public override void Update(in EcsServerTickContext context)
  {
    long currentTick = context.Tick;
    int currentTickInt = checked((int)currentTick);
    World.Query(
        in _healingQuery,
        (ArchEntity entity, ref LifeRegenerationComponent regeneration, ref LifeComponent life) =>
        {
          if (!_handles.TryGetHandle(entity, out EntityRuntimeHandle handle))
          {
            return;
          }

          LifeRestoreRequestQueueComponent requests = LifeRestoreRequestQueue.Require(World);
          foreach (LifeRegenerationEntry entry in regeneration.Entries)
          {
            if (!entry.IsDue(currentTick))
            {
              continue;
            }

            requests.Enqueue(new LifeRestoreRequest(
                handle,
                entry.Magnitude,
                entry.SourceId,
                currentTick));
            regeneration.ScheduleNext(entry.SourceId, currentTickInt);
          }
        });

    World.Query(
        in _damageQuery,
        (ArchEntity entity, ref DamageOverTimeComponent damage, ref LifeComponent life) =>
        {
          if (!_handles.TryGetHandle(entity, out EntityRuntimeHandle handle))
          {
            return;
          }

          CombatDamageRequestQueueComponent requests = CombatDamageRequestQueue.Require(World);
          foreach (DamageOverTimeEntry entry in damage.Entries)
          {
            if (!entry.IsDue(currentTick))
            {
              continue;
            }

            long requestId = ToRequestId(entry.SourceId);
            DamagePayload payload = new(
                entry.Magnitude,
                DamageType.DamageOverTime,
                DamageRelation.Hostile,
                allowSelfDamage: true);
            requests.Enqueue(new DamageRequest(
                handle,
                handle,
                payload,
                life,
                currentTick,
                DamageCause.Environmental,
                requestId: requestId,
                effectSourceId: entry.SourceId));
            damage.ScheduleNext(entry.SourceId, currentTickInt);
          }
        });
  }

  private static long ToRequestId(long sourceId)
  {
    long requestId = sourceId & long.MaxValue;
    return requestId == 0 ? 1 : requestId;
  }
}
