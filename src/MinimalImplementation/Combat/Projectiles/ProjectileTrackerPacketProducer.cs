using System;
using System.IO;
using MinimalImplementation.Contracts;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace MinimalImplementation.Combat.Projectiles;

/// <summary>Builds packet-142 snapshots with the Infrastructure Pocket codec.</summary>
public static class ProjectileTrackerPacketProducer
{
  private const byte SyncProjectileTrackersMessageId = 142;

  public static bool TryProduce(
    ProjectileTrackerManager trackers,
    ProjectileManager projectiles,
    byte player,
    SessionEpoch ownerEpoch,
    out ProjectileTrackerPacketIntent? intent)
  {
    ArgumentNullException.ThrowIfNull(trackers);
    ArgumentNullException.ThrowIfNull(projectiles);
    if (!trackers.TryCapture(player, ownerEpoch, projectiles, out ProjectileTrackerSnapshotView? snapshot) ||
        snapshot is null)
    {
      intent = null;
      return false;
    }

    byte[] body = EncodeBody(snapshot);
    intent = new ProjectileTrackerPacketIntent(
      snapshot.Player,
      snapshot.OwnerEpoch,
      snapshot.Revision,
      snapshot.CommitVersion,
      new ProjectilePacketBody(SyncProjectileTrackersMessageId, null, body));
    return true;
  }

  public static byte[] EncodeBody(ProjectileTrackerSnapshotView snapshot)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    using var stream = new MemoryStream();
    var writer = new PacketWireWriter(stream);
    SyncProjectileTrackersPacket.Write(
      writer,
      snapshot.Player,
      ToPacketReference(snapshot.PiggyBank),
      ToPacketReference(snapshot.VoidLens));
    return stream.ToArray();
  }

  private static PacketTrackedProjectileRef ToPacketReference(ProjectileTrackerReference? reference)
  {
    return reference is null
      ? new PacketTrackedProjectileRef(-1, null, null)
      : new PacketTrackedProjectileRef(reference.Owner, reference.Identity, reference.Type);
  }
}
