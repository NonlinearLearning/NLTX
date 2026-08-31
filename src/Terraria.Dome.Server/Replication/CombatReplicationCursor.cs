using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation;

namespace Terraria.Dome.Server.Replication;

public readonly record struct NpcProjectileCursorEntry(int ReplicationId, long Revision);

public sealed class CombatReplicationCursor
{
  private readonly Dictionary<int, long> _npcRevisions = new();
  private readonly Dictionary<int, long> _projectileRevisions = new();
  private readonly Dictionary<int, long> _npcProjectileRevisions = new();
  private readonly HashSet<int> _skippedProjectiles = new();
  private readonly HashSet<int> _skippedNpcProjectiles = new();

  public void Clear()
  {
    _npcRevisions.Clear();
    _projectileRevisions.Clear();
    _npcProjectileRevisions.Clear();
    _skippedProjectiles.Clear();
    _skippedNpcProjectiles.Clear();
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

  public void MarkProjectileSkipped(ProjectileReplicationSnapshot snapshot)
  {
    _skippedProjectiles.Add(snapshot.ReplicationId);
  }

  public bool WasProjectileSkipped(int replicationId)
  {
    return _skippedProjectiles.Contains(replicationId);
  }

  public void MarkProjectileSent(ProjectileReplicationSnapshot snapshot)
  {
    _projectileRevisions[snapshot.ReplicationId] = snapshot.Revision;
    _skippedProjectiles.Remove(snapshot.ReplicationId);
  }

  public void ForgetProjectile(int replicationId)
  {
    _projectileRevisions.Remove(replicationId);
    _skippedProjectiles.Remove(replicationId);
  }

  public void PruneProjectileSnapshot(
    IReadOnlyList<ProjectileReplicationSnapshot> authoritative,
    long currentTick)
  {
    ArgumentNullException.ThrowIfNull(authoritative);
    if (currentTick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(currentTick));
    }

    HashSet<int> retained = new();
    for (int index = 0; index < authoritative.Count; index++)
    {
      ProjectileReplicationSnapshot snapshot = authoritative[index];
      if (snapshot.IsActive || snapshot.TombstoneRetainedUntilTick <= 0 ||
          currentTick < snapshot.TombstoneRetainedUntilTick)
      {
        retained.Add(snapshot.ReplicationId);
      }
    }

    List<int> stale = new();
    foreach (int replicationId in _projectileRevisions.Keys)
    {
      if (!retained.Contains(replicationId))
      {
        stale.Add(replicationId);
      }
    }

    for (int index = 0; index < stale.Count; index++)
    {
      _projectileRevisions.Remove(stale[index]);
    }

    stale.Clear();
    foreach (int replicationId in _skippedProjectiles)
    {
      if (!retained.Contains(replicationId))
      {
        stale.Add(replicationId);
      }
    }

    for (int index = 0; index < stale.Count; index++)
    {
      _skippedProjectiles.Remove(stale[index]);
    }
  }

  public bool ShouldSend(NpcProjectileReplicationSnapshot snapshot)
  {
    return ShouldSend(_npcProjectileRevisions, snapshot.ReplicationId, snapshot.Revision);
  }

  public void MarkNpcProjectileSent(NpcProjectileReplicationSnapshot snapshot)
  {
    _npcProjectileRevisions[snapshot.ReplicationId] = snapshot.Revision;
    _skippedNpcProjectiles.Remove(snapshot.ReplicationId);
  }

  public void ForgetNpcProjectile(int replicationId)
  {
    _npcProjectileRevisions.Remove(replicationId);
    _skippedNpcProjectiles.Remove(replicationId);
  }

  public bool WasNpcProjectileSent(int replicationId)
  {
    return _npcProjectileRevisions.ContainsKey(replicationId);
  }

  public void MarkNpcProjectileSkipped(NpcProjectileReplicationSnapshot snapshot)
  {
    _skippedNpcProjectiles.Add(snapshot.ReplicationId);
  }

  public bool WasNpcProjectileSkipped(int replicationId)
  {
    return _skippedNpcProjectiles.Contains(replicationId);
  }

  public IReadOnlyList<NpcProjectileCursorEntry> CreateNpcProjectileSnapshot()
  {
    List<NpcProjectileCursorEntry> entries = new(_npcProjectileRevisions.Count);
    foreach (KeyValuePair<int, long> entry in _npcProjectileRevisions)
    {
      entries.Add(new NpcProjectileCursorEntry(entry.Key, entry.Value));
    }

    entries.Sort(static (first, second) => first.ReplicationId.CompareTo(second.ReplicationId));
    return entries;
  }

  public void RestoreNpcProjectileSnapshot(
    IReadOnlyList<NpcProjectileCursorEntry> entries)
  {
    ArgumentNullException.ThrowIfNull(entries);
    _npcProjectileRevisions.Clear();
    for (int index = 0; index < entries.Count; index++)
    {
      NpcProjectileCursorEntry entry = entries[index];
      if (entry.ReplicationId <= 0 || entry.Revision < 0 ||
          !_npcProjectileRevisions.TryAdd(entry.ReplicationId, entry.Revision))
      {
        _npcProjectileRevisions.Clear();
        throw new ArgumentException(
          "NPC projectile cursor snapshot contains invalid or duplicate state.",
          nameof(entries));
      }
    }
  }

  public void PruneNpcProjectileSnapshot(
    IReadOnlyList<NpcProjectileReplicationSnapshot> authoritative,
    long currentTick)
  {
    ArgumentNullException.ThrowIfNull(authoritative);
    if (currentTick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(currentTick));
    }

    HashSet<int> retained = new();
    for (int index = 0; index < authoritative.Count; index++)
    {
      NpcProjectileReplicationSnapshot snapshot = authoritative[index];
      if (snapshot.IsActive || snapshot.TombstoneRetainedUntilTick <= 0 ||
          currentTick < snapshot.TombstoneRetainedUntilTick)
      {
        retained.Add(snapshot.ReplicationId);
      }
    }

    List<int> stale = new();
    foreach (int replicationId in _npcProjectileRevisions.Keys)
    {
      if (!retained.Contains(replicationId))
      {
        stale.Add(replicationId);
      }
    }

    for (int index = 0; index < stale.Count; index++)
    {
      _npcProjectileRevisions.Remove(stale[index]);
    }

    stale.Clear();
    foreach (int replicationId in _skippedNpcProjectiles)
    {
      if (!retained.Contains(replicationId))
      {
        stale.Add(replicationId);
      }
    }

    for (int index = 0; index < stale.Count; index++)
    {
      _skippedNpcProjectiles.Remove(stale[index]);
    }
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
