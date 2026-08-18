using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Server.Replication;

public sealed class DoorReplicationCursor
{
  private readonly Dictionary<int, long> _revisions = new();

  public bool ShouldSend(DoorSnapshot snapshot)
  {
    if (_revisions.TryGetValue(snapshot.DoorId, out long revision) && revision == snapshot.Revision)
    {
      return false;
    }

    _revisions[snapshot.DoorId] = snapshot.Revision;
    return true;
  }

  public void Clear()
  {
    _revisions.Clear();
  }
}
