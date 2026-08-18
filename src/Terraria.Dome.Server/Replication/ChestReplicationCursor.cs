using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldObjects;

namespace Terraria.Dome.Server.Replication;

public sealed class ChestReplicationCursor
{
  private readonly Dictionary<int, long> _revisions = new();

  public bool ShouldSend(ChestSnapshot snapshot)
  {
    if (_revisions.TryGetValue(snapshot.ChestId, out long revision) && revision == snapshot.Revision)
    {
      return false;
    }

    _revisions[snapshot.ChestId] = snapshot.Revision;
    return true;
  }

  public void Clear()
  {
    _revisions.Clear();
  }
}
