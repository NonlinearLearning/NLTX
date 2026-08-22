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

    return true;
  }

  public void MarkSent(ChestSnapshot snapshot)
  {
    _revisions[snapshot.ChestId] = snapshot.Revision;
  }

  public bool TryGetRevision(int chestId, out long revision)
  {
    return _revisions.TryGetValue(chestId, out revision);
  }

  public void Clear()
  {
    _revisions.Clear();
  }
}
