using System.IO;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.World;
using Terraria.Items;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;
using Terraria.WorldStorage;
using WorldTileCoordinate = MinimalImplementation.Server.World.TileCoordinate;

namespace MinimalImplementation.Server.Mechanisms;

/// <summary>Handles server packet 87 and produces the matching packet 86 sharing intent.</summary>
public sealed class TileEntityPacketHandler
{
  private const byte PlacementMessageId = 87;
  private const byte SharingMessageId = 86;
  private const int MaximumSharingPayloadLength = ushort.MaxValue - 3;

  private readonly MechanismFactsCatalog _facts;
  private readonly TileEntityRegistry _entities;
  private readonly TileEntityProjection _projection;
  private readonly TeleportPylonRegistry _pylons;
  private readonly IMechanismIdentityAuthority _identityAuthority;
  private readonly Func<MechanismEntityHandle> _identityFactory;
  private readonly MechanismWorldPort? _world;

  public TileEntityPacketHandler(
    MechanismFactsCatalog facts,
    TileEntityRegistry entities,
    TileEntityProjection projection,
    TeleportPylonRegistry pylons,
    IMechanismIdentityAuthority identityAuthority,
    Func<MechanismEntityHandle> identityFactory,
    MechanismWorldPort? world = null)
  {
    _facts = facts ?? throw new ArgumentNullException(nameof(facts));
    _entities = entities ?? throw new ArgumentNullException(nameof(entities));
    _projection = projection ?? throw new ArgumentNullException(nameof(projection));
    _pylons = pylons ?? throw new ArgumentNullException(nameof(pylons));
    _identityAuthority = identityAuthority
      ?? throw new ArgumentNullException(nameof(identityAuthority));
    _identityFactory = identityFactory ?? throw new ArgumentNullException(nameof(identityFactory));
    _world = world;
    if (entities.WorldScope != pylons.WorldScope || entities.WorldScope != projection.WorldScope)
    {
      throw new ArgumentException("TileEntity services must share one world scope.");
    }
  }

  public TileEntityPlacementDispatchResult HandlePlacement(
    InboundPacketEnvelope envelope,
    byte authenticatedPlayerSlot,
    TileEntityPlacementPacket packet,
    WorldSnapshot world,
    IReadOnlyCollection<SessionEpoch> activeSessionEpochs,
    CommitVersion commitVersion)
  {
    ValidateEnvelope(envelope, authenticatedPlayerSlot, world, commitVersion);
    ArgumentNullException.ThrowIfNull(packet);
    ArgumentNullException.ThrowIfNull(activeSessionEpochs);
    SessionEpoch[] recipients = activeSessionEpochs.Distinct().ToArray();
    if (recipients.Length == 0 || !recipients.Contains(envelope.Epoch))
    {
      return new TileEntityPlacementDispatchResult(
        TileEntityPlacementStatus.SessionUnavailable,
        null,
        Array.Empty<OutboundPacketIntent>());
    }
    if (envelope.WorldScope != _entities.WorldScope || packet.X < 0 || packet.Y < 0 ||
        !world.Header.Geometry.Contains(new WorldTileCoordinate(packet.X, packet.Y)))
    {
      return TileEntityPlacementDispatchResult.NoChange;
    }

    var anchor = new Terraria.WorldStorage.TileCoordinate(packet.X, packet.Y);
    if (_entities.TryGetByAnchor(anchor, out _))
    {
      return new TileEntityPlacementDispatchResult(
        TileEntityPlacementStatus.AlreadyOccupied,
        null,
        Array.Empty<OutboundPacketIntent>());
    }
    if (!_facts.TileEntities.TryGetValue(packet.EntityType, out TileEntityDefinition? definition))
    {
      return new TileEntityPlacementDispatchResult(
        TileEntityPlacementStatus.UnknownType,
        null,
        Array.Empty<OutboundPacketIntent>());
    }

    WorldTile tile = world.GetTile(new WorldTileCoordinate(packet.X, packet.Y));
    var anchorTile = new MechanismTileSnapshot(tile.Active, tile.Type, tile.FrameX, tile.FrameY);
    if (packet.EntityType == 7 && IsPylonPlacementRejected(tile, definition))
    {
      OutboundPacketIntent correction = CreatePylonTileCorrection(
        packet.X, packet.Y, recipients, commitVersion);
      return new TileEntityPlacementDispatchResult(
        TileEntityPlacementStatus.PylonTileRejected,
        null,
        [correction]);
    }

    if (!IsValidAnchorTile(definition, anchorTile))
    {
      return new TileEntityPlacementDispatchResult(
        TileEntityPlacementStatus.InvalidAnchorTile,
        null,
        Array.Empty<OutboundPacketIntent>());
    }

    // Packet 79 has already placed the tile. Packet 87 attaches domain-owned TE state;
    // an empty world preparation binds that attachment to the checked world snapshot.
    if (_world is null || !_world.TryPrepareTilePatches(
        world.WorldEpoch, world.Version, Array.Empty<WorldTilePatch>(),
        producesImmediateBroadcast: false, out WorldTileMutationPreparation? worldPreparation,
        out _) || worldPreparation is null)
    {
      return new TileEntityPlacementDispatchResult(
        TileEntityPlacementStatus.WorldCommitRejected,
        null,
        Array.Empty<OutboundPacketIntent>());
    }

    MechanismSectionCoordinate section = GetSection(world, packet.X, packet.Y);
    ItemState[] emptyItems = Enumerable.Repeat(
      new ItemState(0, 0, 0), definition.ItemSlotCount).ToArray();
    MechanismEntityHandle identity = _identityFactory.Invoke();
    if (!identity.IsValid || identity.WorldScope != _entities.WorldScope ||
        !_identityAuthority.IsActive(identity) || _entities.TryGet(identity, out _) ||
        _pylons.TryGet(identity, out _))
    {
      return new TileEntityPlacementDispatchResult(
        TileEntityPlacementStatus.IdentityUnavailable,
        null,
        Array.Empty<OutboundPacketIntent>());
    }

    // The factory transfers ownership of one fresh active identity to this operation.
    // All potentially failing preparation and encoding happens before publication.
    bool committed = false;
    try
    {
      TileEntityRegistry.CreationPreparation entityPreparation = _entities.PrepareCreate(
        identity, new TileEntityCreateRequest(
        packet.EntityType,
        anchor,
        section,
        anchorTile,
        Array.AsReadOnly(emptyItems)));
      TileEntityRuntimeState created = entityPreparation.State;
      TeleportPylonRegistry.RegistrationPreparation? pylonPreparation =
        created.Definition.Type == 7 ? _pylons.PrepareRegister(created, anchorTile) : null;
      TileEntitySharingPacket sharing = _projection.CreateSharingMessage(created);
      byte[] payload = EncodeSharing(sharing);
      var intent = new OutboundPacketIntent(
        recipients,
        ignoreEpoch: null,
        SharingMessageId,
        payload,
        commitVersion);
      var result = new TileEntityPlacementDispatchResult(
        TileEntityPlacementStatus.Created,
        created,
        [intent]);

      // The caller owns the exclusive ServerLoop window through the final publication.
      if (!worldPreparation.Validate().IsValid || !entityPreparation.Validate() ||
          (pylonPreparation is not null && !pylonPreparation.Validate()))
      {
        return new TileEntityPlacementDispatchResult(
          TileEntityPlacementStatus.WorldCommitRejected,
          null,
          Array.Empty<OutboundPacketIntent>());
      }
      worldPreparation.Commit();
      entityPreparation.Commit();
      pylonPreparation?.Commit();
      committed = true;
      return result;
    }
    finally
    {
      if (!committed && !_identityAuthority.TryRetireMany([identity]))
      {
        throw new InvalidOperationException("The unused TileEntity identity could not be retired.");
      }
    }
  }

  private bool IsPylonPlacementRejected(
    WorldTile tile,
    TileEntityDefinition definition)
  {
    if (!tile.Active || tile.Type != definition.TileType || tile.FrameX < 0 ||
        tile.FrameY != 0 || tile.FrameX % definition.FrameXModulo != 0)
    {
      return true;
    }
    int pylonType = tile.FrameX / definition.FrameXModulo;
    return pylonType > byte.MaxValue || !_facts.Pylons.ContainsKey((byte)pylonType);
  }

  private static bool IsValidAnchorTile(
    TileEntityDefinition definition,
    MechanismTileSnapshot tile)
  {
    return tile.Active && tile.Type == definition.TileType && tile.FrameX >= 0 &&
      tile.FrameY >= 0 &&
      (definition.FrameXModulo == 0 || tile.FrameX % definition.FrameXModulo == 0) &&
      (definition.FrameYModulo == 0 || tile.FrameY % definition.FrameYModulo == 0) &&
      (!definition.RequiresFrameYZero || tile.FrameY == 0);
  }

  private static MechanismSectionCoordinate GetSection(WorldSnapshot world, int x, int y)
  {
    WorldGeometry geometry = world.Header.Geometry;
    if (!geometry.ContainsSection(new WorldSectionCoordinate(
          x / WorldGeometry.SectionWidth,
          y / WorldGeometry.SectionHeight)))
    {
      throw new InvalidOperationException("The TileEntity anchor has no containing section.");
    }
    return new MechanismSectionCoordinate(
      x / WorldGeometry.SectionWidth,
      y / WorldGeometry.SectionHeight);
  }

  private static OutboundPacketIntent CreatePylonTileCorrection(
    short x,
    short y,
    IReadOnlyCollection<SessionEpoch> activeSessionEpochs,
    CommitVersion commitVersion)
  {
    using MemoryStream stream = new();
    var writer = new PacketWireWriter(stream, MaximumSharingPayloadLength);
    TileManipulationPacket.Write(writer, action: 0, x, y, tileOrWallType: 0, style: 0);
    return new OutboundPacketIntent(
      activeSessionEpochs,
      ignoreEpoch: null,
      messageId: 17,
      stream.ToArray(),
      commitVersion);
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

  private void ValidateEnvelope(
    InboundPacketEnvelope envelope,
    byte authenticatedPlayerSlot,
    WorldSnapshot world,
    CommitVersion commitVersion)
  {
    ArgumentNullException.ThrowIfNull(envelope);
    ArgumentNullException.ThrowIfNull(world);
    if (envelope.MessageId != PlacementMessageId)
    {
      throw new ArgumentException("The envelope message id does not match packet 87.",
        nameof(envelope));
    }
    if (authenticatedPlayerSlot == byte.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(authenticatedPlayerSlot));
    }
    if (!commitVersion.IsValid || world.Version != commitVersion)
    {
      throw new ArgumentException(
        "TileEntity placement requires the matching committed world snapshot.",
        nameof(commitVersion));
    }
  }
}

public enum TileEntityPlacementStatus
{
  Created,
  SessionUnavailable,
  AlreadyOccupied,
  UnknownType,
  InvalidAnchorTile,
  PylonTileRejected,
  IdentityUnavailable,
  WorldCommitRejected
}

public sealed record TileEntityPlacementDispatchResult(
  TileEntityPlacementStatus Status,
  TileEntityRuntimeState? CreatedEntity,
  IReadOnlyList<OutboundPacketIntent> OutboundIntents)
{
  public static TileEntityPlacementDispatchResult NoChange { get; } = new(
    TileEntityPlacementStatus.InvalidAnchorTile,
    null,
    Array.Empty<OutboundPacketIntent>());
}
