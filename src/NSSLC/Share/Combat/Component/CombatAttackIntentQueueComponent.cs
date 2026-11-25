using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Terraria.Share.Relationships;

namespace Terraria.Share.Combat;

/// <summary>Tick-scoped append-only queue of Player, NPC and Projectile attack intents.</summary>
/// <remarks>
/// The queue lives on a dedicated world-scoped entity instead of being handed to producers by the
/// host. <see cref="Enqueue"/> starts a new frame when the tick changes, and <see cref="Drain"/>
/// returns nothing for a stale tick, preserving the previous injected-buffer semantics.
/// </remarks>
public sealed class CombatAttackIntentQueueComponent
{
  private readonly List<CombatAttackIntent> _pending = [];
  private readonly Dictionary<EntityRuntimeHandle, long> _nextRequestIds = [];
  private long _tick = -1;

  public int Count => _pending.Count;

  public void BeginTick(long tick)
  {
    if (tick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tick));
    }

    if (_tick != tick)
    {
      _pending.Clear();
      _tick = tick;
    }
  }

  public long Enqueue(
      EntityRuntimeHandle source,
      EntityRuntimeHandle target,
      CombatAttackId definitionId,
      long tick,
      DamageCause cause,
      DamageContact? contact = null)
  {
    BeginTick(tick);
    long requestId = NextRequestId(source);
    _pending.Add(new CombatAttackIntent(
        source,
        target,
        definitionId,
        tick,
        cause,
        contact,
        requestId));
    return requestId;
  }

  public IReadOnlyList<CombatAttackIntent> Drain(long tick)
  {
    if (_tick != tick)
    {
      return Array.Empty<CombatAttackIntent>();
    }

    CombatAttackIntent[] intents = _pending.ToArray();
    _pending.Clear();
    return Array.AsReadOnly(intents);
  }

  public void Clear()
  {
    _pending.Clear();
    _tick = -1;
  }

  private long NextRequestId(EntityRuntimeHandle source)
  {
    if (!_nextRequestIds.TryGetValue(source, out long current))
    {
      current = 0;
    }

    if (current == long.MaxValue)
    {
      throw new InvalidOperationException("The attack request identity sequence is exhausted.");
    }

    long next = current + 1;
    _nextRequestIds[source] = next;
    return next;
  }
}
