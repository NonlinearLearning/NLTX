using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldObjects;
using Terraria.Dome.Simulation.WorldObjects.Sign;

namespace Terraria.Dome.Server.Replication;

public sealed class SignReplicationCursor
{
  private readonly Dictionary<int, long> _revisions = new();
  private readonly Dictionary<int, long> _tombstoneRevisions = new();

  public bool ShouldSend(SignSnapshot snapshot)
  {
    if (_revisions.TryGetValue(snapshot.SignId, out long revision) && revision == snapshot.Revision)
    {
      return false;
    }

    return true;
  }

  public void MarkSent(SignSnapshot snapshot)
  {
    _revisions[snapshot.SignId] = snapshot.Revision;
  }

  public void Clear()
  {
    _revisions.Clear();
    _tombstoneRevisions.Clear();
  }

  public bool ShouldSend(SignTombstoneSnapshot snapshot)
  {
    return !_tombstoneRevisions.TryGetValue(snapshot.SignId, out long revision) ||
      snapshot.Revision > revision;
  }

  public void MarkTombstoneSent(SignTombstoneSnapshot snapshot)
  {
    _tombstoneRevisions[snapshot.SignId] = snapshot.Revision;
  }
}
