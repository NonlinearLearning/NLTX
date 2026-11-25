using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Terraria.Share.Relationships;

namespace Terraria.Unshared.Projectiles;

/// <summary>Tick-scoped queue of termination requests consumed at the lifecycle commit.</summary>
/// <remarks>
/// The queue lives on a dedicated world-scoped entity, so the lifecycle owner, the hit resolution
/// adapter and the lifetime tick system all observe the same instance without the host handing a
/// mutable buffer to any of them.
/// </remarks>
public sealed class ProjectileTerminationQueueComponent
{
  private readonly List<ProjectileTerminationRequest> _pending = [];
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

  public long Enqueue(EntityRuntimeHandle handle, ProjectileTerminationReason reason, long tick)
  {
    BeginTick(tick);
    if (!_nextRequestIds.TryGetValue(handle, out long current))
    {
      current = 0;
    }

    if (current == long.MaxValue)
    {
      throw new InvalidOperationException("The termination request identity sequence is exhausted.");
    }

    long requestId = current + 1;
    _nextRequestIds[handle] = requestId;
    _pending.Add(new ProjectileTerminationRequest(handle, reason, tick, requestId));
    return requestId;
  }

  public IReadOnlyList<ProjectileTerminationRequest> Drain(long tick)
  {
    if (_tick != tick)
    {
      return Array.Empty<ProjectileTerminationRequest>();
    }

    ProjectileTerminationRequest[] requests = _pending.ToArray();
    _pending.Clear();
    return Array.AsReadOnly(requests);
  }

  public bool Contains(EntityRuntimeHandle handle, long tick)
  {
    if (!handle.IsValid || _tick != tick)
    {
      return false;
    }

    for (int index = 0; index < _pending.Count; index++)
    {
      if (_pending[index].Handle.Matches(handle))
      {
        return true;
      }
    }

    return false;
  }

  public void Clear()
  {
    _pending.Clear();
    _tick = -1;
  }
}
