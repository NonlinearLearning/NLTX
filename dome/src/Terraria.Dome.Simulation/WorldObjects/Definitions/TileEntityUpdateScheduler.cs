using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldObjects.Definitions;

public sealed class TileEntityUpdateScheduler
{
  private readonly Queue<int> _pending = new();

  public int PendingCount => _pending.Count;

  public void Enqueue(int entityId)
  {
    if (entityId <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(entityId));
    }

    _pending.Enqueue(entityId);
  }

  public IReadOnlyList<int> Drain(int maximumUpdates)
  {
    if (maximumUpdates <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumUpdates));
    }

    List<int> updates = new(Math.Min(maximumUpdates, _pending.Count));
    while (_pending.Count > 0 && updates.Count < maximumUpdates)
    {
      updates.Add(_pending.Dequeue());
    }

    return updates;
  }
}
