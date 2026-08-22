using System.Collections.Generic;
using Terraria.Dome.Simulation;

namespace Terraria.Dome.Server.Replication;

public sealed class TileEntityReplicationCursor
{
  private readonly Dictionary<int, TileEntityPersistentState> _states = new();

  public bool ShouldSend(TileEntityPersistentState snapshot)
  {
    return !_states.TryGetValue(snapshot.Id, out TileEntityPersistentState? current) ||
      !AreEqual(current, snapshot);
  }

  public void MarkSent(TileEntityPersistentState snapshot)
  {
    _states[snapshot.Id] = snapshot;
  }

  public bool WasSent(int entityId)
  {
    return _states.ContainsKey(entityId);
  }

  public IReadOnlyCollection<int> SentEntityIds => _states.Keys;

  public void MarkRemoved(int entityId)
  {
    _states.Remove(entityId);
  }

  public void Clear()
  {
    _states.Clear();
  }

  private static bool AreEqual(
    TileEntityPersistentState first,
    TileEntityPersistentState second)
  {
    if (first.Id != second.Id || first.Type != second.Type ||
        first.TileX != second.TileX || first.TileY != second.TileY ||
        first.IsOpaque != second.IsOpaque || first.Payload.Count != second.Payload.Count)
    {
      return false;
    }

    for (int index = 0; index < first.Payload.Count; index++)
    {
      if (first.Payload[index] != second.Payload[index])
      {
        return false;
      }
    }

    return true;
  }
}
