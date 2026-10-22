using System.IO;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Protocol;
using MinimalImplementation.Server.World;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace MinimalImplementation.Server.Mechanisms;

/// <summary>Coordinates prepared world, TileEntity, pylon and leashed-anchor removal.</summary>
public sealed class TileEntityRemovalCoordinator
{
  private const byte SharingMessageId = 86;
  private const int MaximumSharingPayloadLength = ushort.MaxValue - 3;

  private readonly MechanismWorldPort _world;
  private readonly TileEntityRegistry _entities;
  private readonly TileEntityProjection _projection;
  private readonly TeleportPylonRegistry _pylons;
  private readonly LeashedEntityRegistry _leashedEntities;
  private readonly IMechanismIdentityAuthority _identityAuthority;
  private readonly MechanismPacketProducer _packets;

  public TileEntityRemovalCoordinator(
    MechanismWorldPort world,
    TileEntityRegistry entities,
    TileEntityProjection projection,
    TeleportPylonRegistry pylons,
    LeashedEntityRegistry leashedEntities,
    IMechanismIdentityAuthority identityAuthority,
    MechanismPacketProducer packets)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(entities);
    ArgumentNullException.ThrowIfNull(projection);
    ArgumentNullException.ThrowIfNull(pylons);
    ArgumentNullException.ThrowIfNull(leashedEntities);
    ArgumentNullException.ThrowIfNull(identityAuthority);
    ArgumentNullException.ThrowIfNull(packets);
    if (entities.WorldScope != projection.WorldScope ||
        entities.WorldScope != pylons.WorldScope ||
        entities.WorldScope != leashedEntities.WorldScope)
    {
      throw new ArgumentException("The mechanism removal owners must share one world scope.");
    }

    _world = world;
    _entities = entities;
    _projection = projection;
    _pylons = pylons;
    _leashedEntities = leashedEntities;
    _identityAuthority = identityAuthority;
    _packets = packets;
  }

  /// <summary>
  /// Prepares and encodes every participant before retiring the TE identity. The caller must
  /// retain its exclusive ServerLoop window through the final publication and enqueue returned
  /// intents only after this method reports Removed.
  /// </summary>
  public TileEntityRemovalDispatchResult Remove(
    MechanismEntityHandle handle,
    WorldSnapshot before,
    IReadOnlyList<WorldTilePatch> tilePatches,
    IReadOnlyCollection<SessionEpoch> recipients,
    CommitVersion expectedVersion)
  {
    ArgumentNullException.ThrowIfNull(before);
    ArgumentNullException.ThrowIfNull(tilePatches);
    ArgumentNullException.ThrowIfNull(recipients);
    if (!handle.IsValid || handle.WorldScope != _entities.WorldScope ||
        !expectedVersion.IsValid || before.Version != expectedVersion)
    {
      return TileEntityRemovalDispatchResult.Rejected(
        TileEntityRemovalStatus.InvalidRequest,
        "The removal handle, world scope or snapshot version is invalid.");
    }

    SessionEpoch[] targets = recipients.Distinct().ToArray();
    if (!_entities.TryGet(handle, out TileEntityRuntimeState? state) || state is null)
    {
      return TileEntityRemovalDispatchResult.Rejected(
        TileEntityRemovalStatus.NotFound,
        "The TileEntity identity is not active in this world.");
    }

    if (!_world.TryPrepareTilePatches(
      before.WorldEpoch,
      expectedVersion,
      tilePatches,
      producesImmediateBroadcast: true,
      out WorldTileMutationPreparation? worldPreparation,
      out _) || worldPreparation is null ||
        worldPreparation.Result.Version is not CommitVersion commitVersion)
    {
      return TileEntityRemovalDispatchResult.Rejected(
        TileEntityRemovalStatus.WorldCommitRejected,
        "The P03 world rejected the TileEntity removal preparation.");
    }

    TileEntityRegistry.RemovalPreparation entityPreparation = _entities.PrepareRemove(handle);
    TeleportPylonRegistry.RemovalPreparation? pylonPreparation = null;
    if (state.Definition.Type == 7)
    {
      if (!_pylons.TryGet(handle, out _))
      {
        return TileEntityRemovalDispatchResult.Rejected(
          TileEntityRemovalStatus.RegistryInconsistent,
          "The active pylon TileEntity has no matching pylon index.");
      }
      pylonPreparation = _pylons.PrepareRemove(handle);
    }
    else if (_pylons.TryGet(handle, out _))
    {
      return TileEntityRemovalDispatchResult.Rejected(
        TileEntityRemovalStatus.RegistryInconsistent,
        "A non-pylon TileEntity is present in the pylon index.");
    }

    LeashedEntityRegistry.DeactivationPreparation? leashedPreparation = null;
    if (state.Definition.Type is 9 or 10)
    {
      if (_leashedEntities.TryGetAt(
            state.Anchor,
            out LeashedEntityRuntimeSnapshot? leashedEntity) &&
          leashedEntity is not null &&
          (state.Definition.Type == 9
            ? leashedEntity.Type != 1
            : leashedEntity.Type is < 2 or > 19))
      {
        return TileEntityRemovalDispatchResult.Rejected(
          TileEntityRemovalStatus.RegistryInconsistent,
          "The leashed entity type does not match its anchor TileEntity.");
      }
      leashedPreparation = _leashedEntities.PrepareDeactivateAt(state.Anchor);
    }

    TileEntitySharingPacket removal =
      _projection.CreateRemovalMessage(entityPreparation);
    var intents = new List<OutboundPacketIntent>();
    if (targets.Length > 0)
    {
      byte[] removalPayload = EncodeSharing(removal);
      intents.Add(new OutboundPacketIntent(
        targets,
        ignoreEpoch: null,
        SharingMessageId,
        removalPayload,
        commitVersion));
      intents.AddRange(_packets.CreateTileChangeIntents(before, worldPreparation, targets));
    }
    var result = new TileEntityRemovalDispatchResult(
      TileEntityRemovalStatus.Removed,
      entityPreparation.Result,
      pylonPreparation?.Snapshot,
      leashedPreparation?.Previous is not null,
      commitVersion,
      Array.AsReadOnly(intents.ToArray()),
      null);

    if (!worldPreparation.Validate().IsValid || !entityPreparation.Validate() ||
        (pylonPreparation is not null && !pylonPreparation.Validate()) ||
        (leashedPreparation is not null && !leashedPreparation.Validate()))
    {
      return TileEntityRemovalDispatchResult.Rejected(
        TileEntityRemovalStatus.StalePreparation,
        "A prepared removal participant changed before publication.");
    }
    if (!_identityAuthority.TryRetireMany([handle]))
    {
      return TileEntityRemovalDispatchResult.Rejected(
        TileEntityRemovalStatus.IdentityRetirementRejected,
        "The identity owner rejected TileEntity retirement.");
    }

    // Identity retirement is the only rejecting commit gate; remaining commits publish prepared
    // references in this exclusive owner window and perform no business validation or allocation.
    worldPreparation.Commit();
    entityPreparation.CommitAfterRetirement();
    pylonPreparation?.Commit();
    leashedPreparation?.Commit();
    return result;
  }

  private static byte[] EncodeSharing(TileEntitySharingPacket packet)
  {
    using MemoryStream stream = new();
    var writer = new PacketWireWriter(stream, MaximumSharingPayloadLength);
    TileEntitySharingPacket.Write(
      writer,
      packet.EntityId,
      packet.Exists,
      packet.Entity,
      PacketTileEntityCodecsV4.Create());
    return stream.ToArray();
  }
}

public enum TileEntityRemovalStatus
{
  Removed,
  InvalidRequest,
  NoRecipients,
  NotFound,
  WorldCommitRejected,
  RegistryInconsistent,
  StalePreparation,
  IdentityRetirementRejected
}

public sealed record TileEntityRemovalDispatchResult(
  TileEntityRemovalStatus Status,
  TileEntityRemovalResult? Removal,
  TeleportPylonSnapshot? RemovedPylon,
  bool LeashedEntityDeactivated,
  CommitVersion? CommitVersion,
  IReadOnlyList<OutboundPacketIntent> OutboundIntents,
  string? Detail)
{
  public static TileEntityRemovalDispatchResult Rejected(
    TileEntityRemovalStatus status,
    string detail) => new(
      status,
      null,
      null,
      false,
      null,
      Array.Empty<OutboundPacketIntent>(),
      detail);
}
