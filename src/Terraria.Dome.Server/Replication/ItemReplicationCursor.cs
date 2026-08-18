using System.Collections.Generic;
using Terraria.Dome.Simulation;

namespace Terraria.Dome.Server.Replication;

public sealed class ItemReplicationCursor
{
  private readonly Dictionary<int, long> _revisions = new();

  public void Clear()
  {
    _revisions.Clear();
  }

  public bool ShouldSend(ItemReplicationSnapshot snapshot)
  {
    if (_revisions.TryGetValue(snapshot.ReplicationId, out long revision) &&
        revision >= snapshot.Revision)
    {
      return false;
    }

    _revisions[snapshot.ReplicationId] = snapshot.Revision;
    return true;
  }
}
