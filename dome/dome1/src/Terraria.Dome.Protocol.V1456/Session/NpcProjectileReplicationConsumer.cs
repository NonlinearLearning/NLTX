using System;
using System.Collections.Generic;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Simulation;

namespace Terraria.Dome.Protocol.V1456.Session;

public readonly record struct NpcProjectileReplicationConsumerSnapshot(
  IReadOnlyList<NpcProjectileReplicationSnapshot> Projectiles);

public sealed class NpcProjectileReplicationConsumer
{
  private readonly Dictionary<int, NpcProjectileReplicationSnapshot> _snapshots = new();

  public IReadOnlyCollection<NpcProjectileReplicationSnapshot> Snapshots => _snapshots.Values;

  public bool TryApply(
    ReadOnlySpan<byte> frameBytes,
    long currentTick,
    out NpcProjectileReplicationSnapshot snapshot)
  {
    snapshot = ContractExtensionCodec.IsNpcProjectileReplicationV3(frameBytes)
      ? ContractExtensionCodec.DecodeNpcProjectileReplicationV3(frameBytes)
      : ContractExtensionCodec.IsNpcProjectileReplicationV2(frameBytes)
        ? ContractExtensionCodec.DecodeNpcProjectileReplicationV2(frameBytes)
        : ContractExtensionCodec.DecodeNpcProjectileReplication(frameBytes);
    if (currentTick < 0 ||
        (!snapshot.IsActive && snapshot.TombstoneRetainedUntilTick > 0 &&
          currentTick >= snapshot.TombstoneRetainedUntilTick) ||
        (_snapshots.TryGetValue(snapshot.ReplicationId, out NpcProjectileReplicationSnapshot current) &&
          current.Revision >= snapshot.Revision))
    {
      snapshot = default;
      return false;
    }

    _snapshots[snapshot.ReplicationId] = snapshot;
    return true;
  }

  public NpcProjectileReplicationConsumerSnapshot CreateSnapshot()
  {
    List<NpcProjectileReplicationSnapshot> snapshots = new(_snapshots.Values);
    snapshots.Sort(static (first, second) => first.ReplicationId.CompareTo(second.ReplicationId));
    return new NpcProjectileReplicationConsumerSnapshot(snapshots);
  }

  public void Restore(NpcProjectileReplicationConsumerSnapshot snapshot, long currentTick)
  {
    if (currentTick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(currentTick));
    }

    _snapshots.Clear();
    ArgumentNullException.ThrowIfNull(snapshot.Projectiles);
    for (int index = 0; index < snapshot.Projectiles.Count; index++)
    {
      NpcProjectileReplicationSnapshot projectile = snapshot.Projectiles[index];
      if (projectile.ReplicationId <= 0 || projectile.Revision < 0 ||
          !projectile.Owner.IsValid || projectile.Identity <= 0 ||
          (!projectile.IsActive && projectile.TombstoneRetainedUntilTick > 0 &&
            currentTick >= projectile.TombstoneRetainedUntilTick) ||
          (projectile.IsActive && projectile.TombstoneReason != ProjectileTombstoneReason.None) ||
          (!projectile.IsActive && projectile.TombstoneReason == ProjectileTombstoneReason.None) ||
          !_snapshots.TryAdd(projectile.ReplicationId, projectile))
      {
        _snapshots.Clear();
        throw new ArgumentException(
          "NPC projectile consumer snapshot contains invalid or duplicate state.",
          nameof(snapshot));
      }
    }
  }

  public void Clear()
  {
    _snapshots.Clear();
  }
}
