using System;
using System.Collections.Generic;
using Arch.Core;
using Arch.System;
using ArchEntity = Arch.Core.Entity;
using ArchWorld = Arch.Core.World;
using Terraria.Combat;
using Terraria.Entity;

namespace Terraria.Attributes;

/// <summary>
/// Materializes declared behavior contributions into concrete status components.
/// </summary>
/// <remarks>
/// This system only owns behavior-component composition. Periodic behavior is emitted as target
/// domain requests by a later system; it never mutates LifeComponent or Combat state directly.
/// </remarks>
public sealed class StatusEffectBehaviorSystem : BaseSystem<ArchWorld, EcsServerTickContext>
{
  private readonly BuffDefinitionRegistry _definitions;
  private readonly QueryDescription _query = new QueryDescription()
      .WithAll<BuffListComponent>();

  public StatusEffectBehaviorSystem(ArchWorld world, BuffDefinitionRegistry definitions)
      : base(world)
  {
    _definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
  }

  public override void Update(in EcsServerTickContext context)
  {
    ArchWorld world = context.World;
    EcsServerCommandBuffer? commands = context.Commands;
    int currentTick = checked((int)context.Tick);
    world.Query(
        in _query,
        (ArchEntity entity, ref BuffListComponent activeBuffs) =>
        {
          Dictionary<long, DamageOverTimeEntry> damage = [];
          Dictionary<long, LifeRegenerationEntry> healing = [];
          HashSet<long> invisible = [];
          HashSet<long> stunned = [];
          HashSet<long> immune = [];

          foreach (ActiveBuff active in activeBuffs.Entries)
          {
            if (!_definitions.TryGet(active.Effect, out BuffDefinition definition))
            {
              continue;
            }

            foreach (BuffBehaviorContribution behavior in definition.Behaviors)
            {
              AddBehavior(
                  active,
                  behavior.Kind,
                  behavior.PeriodTicks,
                  behavior.Magnitude,
                  currentTick,
                  damage,
                  healing,
                  invisible,
                  stunned,
                  immune);
            }

            foreach (BuffPeriodicEffect periodic in definition.PeriodicEffects)
            {
              AddBehavior(
                  active,
                  periodic.Kind,
                  periodic.PeriodTicks,
                  periodic.Magnitude,
                  currentTick,
                  damage,
                  healing,
                  invisible,
                  stunned,
                  immune);
            }
          }

          SyncDamage(world, entity, damage, currentTick, commands);
          SyncHealing(world, entity, healing, currentTick, commands);
          SyncSources(
              world,
              entity,
              invisible,
              stunned,
              immune,
              commands);
        });
  }

  private static void AddBehavior(
      ActiveBuff active,
      BuffBehaviorKind kind,
      int periodTicks,
      int magnitude,
      int tick,
      IDictionary<long, DamageOverTimeEntry> damage,
      IDictionary<long, LifeRegenerationEntry> healing,
      ISet<long> invisible,
      ISet<long> stunned,
      ISet<long> immune)
  {
    int scaledMagnitude = checked(magnitude * active.Stacks);
    switch (kind)
    {
      case BuffBehaviorKind.DamageOverTime:
        damage[active.SourceId] = new DamageOverTimeEntry(
            active.SourceId,
            periodTicks,
            scaledMagnitude,
            tick + periodTicks);
        break;
      case BuffBehaviorKind.PeriodicHeal:
        healing[active.SourceId] = new LifeRegenerationEntry(
            active.SourceId,
            periodTicks,
            scaledMagnitude,
            tick + periodTicks);
        break;
      case BuffBehaviorKind.Invisibility:
        invisible.Add(active.SourceId);
        break;
      case BuffBehaviorKind.Stun:
        stunned.Add(active.SourceId);
        break;
      case BuffBehaviorKind.DamageImmunity:
        immune.Add(active.SourceId);
        break;
      case BuffBehaviorKind.MovementRestriction:
        stunned.Add(active.SourceId);
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown Buff behavior.");
    }
  }

  private static void SyncDamage(
      ArchWorld world,
      ArchEntity entity,
      IReadOnlyDictionary<long, DamageOverTimeEntry> desired,
      int tick,
      EcsServerCommandBuffer? commands)
  {
    if (!world.Has<DamageOverTimeComponent>(entity))
    {
      if (desired.Count != 0)
      {
        DamageOverTimeComponent component = new();
        foreach (DamageOverTimeEntry entry in desired.Values)
        {
          component.AddOrReplace(entry);
        }

        Add(world, entity, component, commands);
      }

      return;
    }

    DamageOverTimeComponent existing = world.Get<DamageOverTimeComponent>(entity);
    foreach (DamageOverTimeEntry entry in existing.Entries)
    {
      if (!desired.ContainsKey(entry.SourceId))
      {
        existing.Remove(entry.SourceId);
      }
    }

    foreach (DamageOverTimeEntry entry in desired.Values)
    {
      if (existing.TryGet(entry.SourceId, out DamageOverTimeEntry previous))
      {
        existing.AddOrReplace(entry with { NextTick = previous.NextTick });
      }
      else
      {
        existing.AddOrReplace(entry with { NextTick = tick + entry.PeriodTicks });
      }
    }

    if (existing.IsEmpty)
    {
      Remove<DamageOverTimeComponent>(world, entity, commands);
    }
  }

  private static void SyncHealing(
      ArchWorld world,
      ArchEntity entity,
      IReadOnlyDictionary<long, LifeRegenerationEntry> desired,
      int tick,
      EcsServerCommandBuffer? commands)
  {
    if (!world.Has<LifeRegenerationComponent>(entity))
    {
      if (desired.Count != 0)
      {
        LifeRegenerationComponent component = new();
        foreach (LifeRegenerationEntry entry in desired.Values)
        {
          component.AddOrReplace(entry);
        }

        Add(world, entity, component, commands);
      }

      return;
    }

    LifeRegenerationComponent existing = world.Get<LifeRegenerationComponent>(entity);
    foreach (LifeRegenerationEntry entry in existing.Entries)
    {
      if (!desired.ContainsKey(entry.SourceId))
      {
        existing.Remove(entry.SourceId);
      }
    }

    foreach (LifeRegenerationEntry entry in desired.Values)
    {
      if (existing.TryGet(entry.SourceId, out LifeRegenerationEntry previous))
      {
        existing.AddOrReplace(entry with { NextTick = previous.NextTick });
      }
      else
      {
        existing.AddOrReplace(entry with { NextTick = tick + entry.PeriodTicks });
      }
    }

    if (existing.IsEmpty)
    {
      Remove<LifeRegenerationComponent>(world, entity, commands);
    }
  }

  private static void SyncSources(
      ArchWorld world,
      ArchEntity entity,
      ISet<long> invisible,
      ISet<long> stunned,
      ISet<long> immune,
      EcsServerCommandBuffer? commands)
  {
    SyncSourceComponent(
        world,
        entity,
        invisible,
        static () => new InvisibilityComponent(),
        static component => component.SourceIds,
        static (component, source) => component.Add(source),
        static (component, source) => component.Remove(source),
        commands);
    SyncSourceComponent(
        world,
        entity,
        stunned,
        static () => new StunComponent(),
        static component => component.SourceIds,
        static (component, source) => component.Add(source),
        static (component, source) => component.Remove(source),
        commands);
    SyncSourceComponent(
        world,
        entity,
        immune,
        static () => new DamageImmunityStatusComponent(),
        static component => component.SourceIds,
        static (component, source) => component.Add(source),
        static (component, source) => component.Remove(source),
        commands);
  }

  private static void SyncSourceComponent<TComponent>(
      ArchWorld world,
      ArchEntity entity,
      ISet<long> desired,
      Func<TComponent> create,
      Func<TComponent, IReadOnlyCollection<long>> sources,
      Action<TComponent, long> add,
      Func<TComponent, long, bool> remove,
      EcsServerCommandBuffer? commands)
      where TComponent : class
  {
    if (!world.Has<TComponent>(entity))
    {
      if (desired.Count == 0)
      {
        return;
      }

      TComponent component = create();
      foreach (long source in desired)
      {
        add(component, source);
      }

      Add(world, entity, component, commands);
      return;
    }

    TComponent existing = world.Get<TComponent>(entity);
    long[] existingSources = sources(existing).ToArray();
    foreach (long source in existingSources)
    {
      if (!desired.Contains(source))
      {
        remove(existing, source);
      }
    }

    foreach (long source in desired)
    {
      add(existing, source);
    }

    if (sources(existing).Count == 0)
    {
      Remove<TComponent>(world, entity, commands);
    }
  }

  private static void Add<TComponent>(
      ArchWorld world,
      ArchEntity entity,
      TComponent component,
      EcsServerCommandBuffer? commands)
      where TComponent : class
  {
    if (commands is not null)
    {
      commands.Add(entity, component);
    }
    else
    {
      world.Add(entity, component);
    }
  }

  private static void Remove<TComponent>(
      ArchWorld world,
      ArchEntity entity,
      EcsServerCommandBuffer? commands)
      where TComponent : class
  {
    if (commands is not null)
    {
      commands.Remove<TComponent>(entity);
    }
    else
    {
      world.Remove<TComponent>(entity);
    }
  }
}
