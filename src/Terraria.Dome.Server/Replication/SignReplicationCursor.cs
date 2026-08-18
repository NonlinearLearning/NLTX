using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldObjects;

namespace Terraria.Dome.Server.Replication;

public sealed class SignReplicationCursor
{
  private readonly Dictionary<int, long> _revisions = new();

  public bool ShouldSend(SignSnapshot snapshot)
  {
    if (_revisions.TryGetValue(snapshot.SignId, out long revision) && revision == snapshot.Revision)
    {
      return false;
    }

    _revisions[snapshot.SignId] = snapshot.Revision;
    return true;
  }

  public void Clear()
  {
    _revisions.Clear();
  }
}
