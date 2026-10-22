using System;
using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Contracts;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Combat.Projectiles;

/// <summary>Resolves packet-142 tracker tuples and emits the canonical server relay.</summary>
public sealed class ProjectileTrackerPacketHandler
{
  private readonly ProjectileTrackerManager _trackers;
  private readonly ProjectileManager _projectiles;

  public ProjectileTrackerPacketHandler(
    ProjectileTrackerManager trackers,
    ProjectileManager projectiles)
  {
    ArgumentNullException.ThrowIfNull(trackers);
    ArgumentNullException.ThrowIfNull(projectiles);
    _trackers = trackers;
    _projectiles = projectiles;
    _projectiles.AttachTrackerManager(trackers);
  }

  public ProjectileTrackerDispatchResult HandleSync(
    InboundPacketEnvelope envelope,
    AuthenticatedCombatSender sender,
    SyncProjectileTrackersPacket packet)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(packet);
    if (envelope.MessageId != 142 || envelope.Epoch != sender.Epoch)
    {
      return Rejected(
        ProtocolErrorCode.Forbidden,
        "Projectile tracker synchronization must match the authenticated packet-142 session.");
    }

    if (!TryResolveReference(packet.PiggyBank, out ProjectileTrackerReference? piggyBank) ||
        !TryResolveReference(packet.VoidLens, out ProjectileTrackerReference? voidLens))
    {
      return Rejected(
        ProtocolErrorCode.MalformedFrame,
        "Packet 142 contains an invalid owner/identity/type reference shape.");
    }

    // Version4 ignores the packet's player byte and binds the update to whoAmI.
    OwnerCommitResult committed = _trackers.Apply(sender.PlayerSlot, sender.Epoch, piggyBank, voidLens);
    if (committed.Status != OwnerCommitStatus.Applied ||
        !_trackers.TryCapture(sender.PlayerSlot, sender.Epoch, _projectiles, out ProjectileTrackerSnapshotView? snapshot) ||
        snapshot is null)
    {
      return new ProjectileTrackerDispatchResult(committed, null, null);
    }

    byte[] body = ProjectileTrackerPacketProducer.EncodeBody(snapshot);
    var broadcast = new ProjectilePacketBody(142, sender.Epoch, body);
    return new ProjectileTrackerDispatchResult(committed, snapshot, broadcast);
  }

  private bool TryResolveReference(
    PacketTrackedProjectileRef reference,
    out ProjectileTrackerReference? resolved)
  {
    if (reference.Owner == -1)
    {
      if (reference.Identity.HasValue || reference.Type.HasValue)
      {
        resolved = null;
        return false;
      }

      resolved = null;
      return true;
    }

    if (!reference.Identity.HasValue || !reference.Type.HasValue)
    {
      resolved = null;
      return false;
    }

    if (!_projectiles.TryResolveTrackerReference(
          reference.Owner,
          reference.Identity.Value,
          reference.Type.Value,
          out ProjectileView? projectile) || projectile is null)
    {
      // Version4 clears an unmatched reference while preserving the packet update.
      resolved = null;
      return true;
    }

    resolved = new ProjectileTrackerReference(
      projectile.Entity,
      reference.Owner,
      reference.Identity.Value,
      reference.Type.Value);
    return true;
  }

  private static ProjectileTrackerDispatchResult Rejected(ProtocolErrorCode code, string detail)
  {
    return new ProjectileTrackerDispatchResult(OwnerCommitResult.Rejected(code, detail), null, null);
  }
}
