using System.Collections.Generic;
using Terraria.Dome.Simulation;

namespace Terraria.Dome.Server.Replication;

public sealed class CombatReplicationCursor
{
  private readonly Dictionary<int, long> _npcRevisions = new();
  private readonly Dictionary<int, long> _projectileRevisions = new();

  public void Clear()
  {
    _npcRevisions.Clear();
    _projectileRevisions.Clear();
  }

  public bool ShouldSend(NpcReplicationSnapshot snapshot)
  {
    return ShouldSend(_npcRevisions, snapshot.ReplicationId, snapshot.Revision);
  }

  public void MarkNpcSent(NpcReplicationSnapshot snapshot)
  {
    _npcRevisions[snapshot.ReplicationId] = snapshot.Revision;
  }

  public bool WasNpcSent(int replicationId)
  {
    return _npcRevisions.ContainsKey(replicationId);
  }

  public bool ShouldSend(ProjectileReplicationSnapshot snapshot)
  {
    return ShouldSend(_projectileRevisions, snapshot.ReplicationId, snapshot.Revision);
  }

  public bool WasProjectileSent(int replicationId)
  {
    return _projectileRevisions.ContainsKey(replicationId);
  }

  public void MarkProjectileSent(ProjectileReplicationSnapshot snapshot)
  {
    _projectileRevisions[snapshot.ReplicationId] = snapshot.Revision;
  }

  private static bool ShouldSend(Dictionary<int, long> revisions, int replicationId, long revision)
  {
    if (revisions.TryGetValue(replicationId, out long lastRevision) && lastRevision >= revision)
    {
      return false;
    }

    return true;
  }
}
