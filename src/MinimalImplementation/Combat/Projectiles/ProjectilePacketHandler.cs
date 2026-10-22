using System;
using MinimalImplementation.Contracts;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Combat.Projectiles;

/// <summary>
/// Applies decoded projectile commands on the server writer and returns packet bodies for P01/P02 routing.
/// </summary>
public sealed class ProjectilePacketHandler
{
  private const byte SyncProjectileMessageId = 27;
  private const byte KillProjectileMessageId = 29;

  private readonly ProjectileManager _manager;
  private readonly ProjectileDefinitionCatalog _definitions;
  private readonly ProjectileTrackerManager? _trackers;

  public ProjectilePacketHandler(
    ProjectileManager manager,
    ProjectileDefinitionCatalog definitions,
    ProjectileTrackerManager? trackers = null)
  {
    ArgumentNullException.ThrowIfNull(manager);
    ArgumentNullException.ThrowIfNull(definitions);
    _manager = manager;
    _definitions = definitions;
    _trackers = trackers;
    if (trackers is not null)
    {
      _manager.AttachTrackerManager(trackers);
    }
  }

  public ProjectileSyncDispatchResult HandleSync(
    InboundPacketEnvelope envelope,
    AuthenticatedCombatSender sender,
    SyncProjectilePacket packet)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(packet);
    var request = new ProjectileSyncRequest(envelope, sender, packet);
    ProjectileSyncPreparation preparation = _manager.PrepareSync(request);
    if (!preparation.IsPrepared)
    {
      return new ProjectileSyncDispatchResult(preparation.Result, null, null, null);
    }

    ProjectileSyncCommitResult committed = _manager.Commit(preparation.Plan!);
    if (committed.Result.Status != OwnerCommitStatus.Applied || committed.Projectile is null)
    {
      return new ProjectileSyncDispatchResult(committed.Result, null, committed.EvictedProjectile, null);
    }

    byte[] body = ProjectilePacketPayloadCodec.EncodeSyncBody(committed.Projectile, _definitions);
    var broadcast = new ProjectilePacketBody(SyncProjectileMessageId, sender.Epoch, body);
    OwnerCommitResult? trackerCleanup = null;
    if (committed.EvictedProjectile is not null && _trackers is not null)
    {
      trackerCleanup = _trackers.RemoveEntity(committed.EvictedProjectile.Entity);
    }

    return new ProjectileSyncDispatchResult(
      committed.Result,
      committed.Projectile,
      committed.EvictedProjectile,
      broadcast,
      trackerCleanup);
  }

  public ProjectileTerminationDispatchResult HandleTermination(
    InboundPacketEnvelope envelope,
    AuthenticatedCombatSender sender,
    KillProjectilePacket packet)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(packet);
    ProjectileTerminationPreparation preparation = _manager.PrepareTermination(
      packet.ProjectileIdentity,
      packet.Owner,
      envelope,
      sender);
    if (preparation.IsNoOp)
    {
      return new ProjectileTerminationDispatchResult(preparation.Result, true, null, null);
    }

    if (preparation.Plan is null)
    {
      return new ProjectileTerminationDispatchResult(preparation.Result, false, null, null);
    }

    ProjectileTerminationCommitResult committed = _manager.Commit(preparation.Plan);
    if (committed.Result.Status != OwnerCommitStatus.Applied || committed.RetiredProjectile is null)
    {
      return new ProjectileTerminationDispatchResult(committed.Result, false, null, null);
    }

    byte[] body = ProjectilePacketPayloadCodec.EncodeKillBody(committed.RetiredProjectile);
    var broadcast = new ProjectilePacketBody(KillProjectileMessageId, sender.Epoch, body);
    OwnerCommitResult? trackerCleanup = _trackers?.RemoveEntity(committed.RetiredProjectile.Entity);
    return new ProjectileTerminationDispatchResult(
      committed.Result,
      false,
      committed.RetiredProjectile,
      broadcast,
      trackerCleanup);
  }

  public ProjectilePortalTerminationDispatchResult HandlePortalTermination(
    InboundPacketEnvelope envelope,
    AuthenticatedCombatSender sender,
    MurderSomeoneElsesPortalPacket packet)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(packet);
    ProjectilePortalTerminationPreparation preparation = _manager.PreparePortalTermination(
      packet.ProjectileOwner,
      packet.PortalIndex,
      envelope,
      sender);
    if (preparation.IsNoOp)
    {
      return new ProjectilePortalTerminationDispatchResult(preparation.Result, true, null, null, null);
    }

    if (preparation.Plan is null)
    {
      return new ProjectilePortalTerminationDispatchResult(preparation.Result, false, null, null, null);
    }

    ProjectileTerminationCommitResult committed = _manager.Commit(preparation.Plan);
    if (committed.Result.Status != OwnerCommitStatus.Applied || committed.RetiredProjectile is null)
    {
      return new ProjectilePortalTerminationDispatchResult(committed.Result, false, null, null, null);
    }

    OwnerCommitResult? trackerCleanup = _trackers?.RemoveEntity(committed.RetiredProjectile.Entity);
    byte[] body = ProjectilePacketPayloadCodec.EncodeKillBody(committed.RetiredProjectile);
    // Version4 sends packet 29 with fromWho = -1, so the requester also receives this kill.
    var broadcast = new ProjectilePacketBody(KillProjectileMessageId, null, body);
    return new ProjectilePortalTerminationDispatchResult(
      committed.Result,
      false,
      committed.RetiredProjectile,
      broadcast,
      trackerCleanup);
  }
}
