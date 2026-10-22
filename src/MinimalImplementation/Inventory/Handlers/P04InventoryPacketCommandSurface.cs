using System.Numerics;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Inventory.Content;
using MinimalImplementation.Server.Inventory.Protocol;
using MinimalImplementation.Server.Inventory.Rules;
using MinimalImplementation.Server.Inventory.State;
using MinimalImplementation.Server.World;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Inventory.Handlers;

/// <summary>
/// Domain-facing P04 packet command surface. It accepts decoded DTOs but owns no
/// socket, transport registry, or cross-owner policy implementation.
/// </summary>
public sealed class P04InventoryPacketCommandSurface
{
  private readonly InventoryStore _inventories;
  private readonly WorldItemStore _worldItems;
  private readonly WorldItemReservationStore _reservations;
  private readonly ChestStore _chests;
  private readonly ChestAccessService _chestAccess;
  private readonly InstancedItemStore _instancedItems;
  private readonly InventoryPaymentCoordinator _payments;
  private readonly ItemValidationPolicy _items;
  private readonly Func<int, bool> _isAllowedChestSize;
  private readonly ActiveSessionPlayerSlotResolver _playerSlots;
  private readonly Func<EntityUuid, bool> _allowCreativeEquipment;
  private readonly WorldItemPickupCoordinator? _pickups;
  private readonly WorldItemOwnershipCoordinator? _ownership;
  private readonly Func<WorldItemSnapshot, EntityUuid, SessionEpoch,
    IWorldItemPickupPreparation?>? _preparePickup;

  public P04InventoryPacketCommandSurface(
    InventoryStore inventories,
    WorldItemStore worldItems,
    WorldItemReservationStore reservations,
    ChestStore chests,
    ChestAccessService chestAccess,
    InstancedItemStore instancedItems,
    InventoryPaymentCoordinator payments,
    ItemValidationPolicy items,
    Func<int, bool> isAllowedChestSize,
    ActiveSessionPlayerSlotResolver playerSlots,
    Func<EntityUuid, bool>? allowCreativeEquipment = null,
    WorldItemPickupCoordinator? pickups = null,
    Func<WorldItemSnapshot, EntityUuid, SessionEpoch,
      IWorldItemPickupPreparation?>? preparePickup = null,
    WorldItemOwnershipCoordinator? ownership = null)
  {
    ArgumentNullException.ThrowIfNull(inventories);
    ArgumentNullException.ThrowIfNull(worldItems);
    ArgumentNullException.ThrowIfNull(reservations);
    ArgumentNullException.ThrowIfNull(chests);
    ArgumentNullException.ThrowIfNull(chestAccess);
    ArgumentNullException.ThrowIfNull(instancedItems);
    ArgumentNullException.ThrowIfNull(payments);
    ArgumentNullException.ThrowIfNull(items);
    ArgumentNullException.ThrowIfNull(isAllowedChestSize);
    ArgumentNullException.ThrowIfNull(playerSlots);
    if ((pickups is null) != (preparePickup is null))
    {
      throw new ArgumentException("Pickup coordinator and Player preparation must be supplied together.");
    }
    if (ownership is not null && (!ReferenceEquals(ownership.Items, worldItems)
      || !ReferenceEquals(ownership.Reservations, reservations)))
    {
      throw new ArgumentException("Ownership and command handling must use the same stores.");
    }
    _inventories = inventories;
    _worldItems = worldItems;
    _reservations = reservations;
    _chests = chests;
    _chestAccess = chestAccess;
    _instancedItems = instancedItems;
    _payments = payments;
    _items = items;
    _isAllowedChestSize = isAllowedChestSize;
    _playerSlots = playerSlots;
    _allowCreativeEquipment = allowCreativeEquipment ?? (_ => false);
    _pickups = pickups;
    _ownership = ownership;
    _preparePickup = preparePickup;
  }

  /// <summary>Handles packet 5 using the authenticated player rather than its player byte.</summary>
  public P04InventoryCommitResult HandleEquipment(
    EntityUuid authenticatedPlayer,
    CommitVersion expectedRevision,
    SyncEquipmentPacket packet)
  {
    ArgumentNullException.ThrowIfNull(packet);
    if (!authenticatedPlayer.IsValid || packet.Slot < 0 || packet.Stack < 0 || packet.ItemType < 0)
    {
      return Rejected(ProtocolErrorCode.Forbidden, "The equipment packet identity or slot values are invalid.");
    }

    if (packet.IsCreativeItem && !_allowCreativeEquipment(authenticatedPlayer))
    {
      return Rejected(ProtocolErrorCode.Forbidden, "The equipment packet requests an unavailable creative item.");
    }

    if (!_inventories.TryCapture(authenticatedPlayer, out InventorySnapshot? current)
      || current is null
      || packet.Slot >= current.Slots.Count)
    {
      return Rejected(ProtocolErrorCode.NotReady, "The authenticated player inventory or equipment slot is unavailable.");
    }

    InventoryItemState item = ToItem(packet.ItemType, packet.Stack, packet.Prefix, packet.Favorited);
    if (!_items.IsValid(item))
    {
      return Rejected(ProtocolErrorCode.Forbidden, "The equipment packet contains an invalid item.");
    }

    OwnerCommitResult commit = _inventories.Apply(
      authenticatedPlayer,
      expectedRevision,
      [new InventoryMutation(packet.Slot, current.Slots[packet.Slot].Generation, item)],
      out InventorySnapshot? updated);
    return new P04InventoryCommitResult(commit, updated);
  }

  /// <summary>Handles packet 21 with a server-captured world-item handle and revision.</summary>
  public P04InventoryCommitResult HandleWorldItemSync(
    WorldItemHandle handle,
    CommitVersion expectedRevision,
    SyncItemPacket packet, SessionEpoch? ignoreEpoch = null)
  {
    ArgumentNullException.ThrowIfNull(packet);
    if (packet.ItemIndex != handle.SlotId
      || packet.Stack <= 0
      || packet.ItemType <= 0)
    {
      return Rejected(ProtocolErrorCode.Forbidden, "The world-item packet does not describe a live item slot.");
    }

    if (!_worldItems.TryCapture(handle.SlotId, out WorldItemSnapshot? current)
      || current is null)
    {
      return Rejected(ProtocolErrorCode.NotReady, "The world item is not active.");
    }

    var mutation = new WorldItemMutation(
      ToItem(packet.ItemType, packet.Stack, packet.Prefix, false),
      new Vector2(packet.PositionX, packet.PositionY),
      new Vector2(packet.VelocityX, packet.VelocityY),
      current.TimeLeft,
      packet.Shimmered ?? false,
      packet.EnemyGrabDelayTime ?? 0,
      packet.ShimmerTime ?? 0,
      current.Runtime);
    OwnerCommitResult commit = _worldItems.Update(handle, expectedRevision, mutation,
      out WorldItemSnapshot? updated, ignoreEpoch);
    return new P04InventoryCommitResult(commit, WorldItem: updated);
  }

  /// <summary>Legacy duration helper; published stores require the frame overload.</summary>
  public ReservationResult HandleItemOwner(
    WorldItemHandle handle,
    EntityUuid playerId,
    SessionEpoch epoch,
    DateTimeOffset now,
    TimeSpan duration)
  {
    lock (_worldItems.CommitGate.SyncRoot)
    {
      if (!_worldItems.TryCapture(handle.SlotId, out WorldItemSnapshot? item)
        || item is null || item.Handle != handle)
      {
        return ReservationResult.Unavailable("The world item is not active.");
      }

      if (!_playerSlots.TryCapture(playerId, epoch, handle.WorldScope, out var owner)
        || owner is null)
      {
        return ReservationResult.Unavailable("The authenticated owner binding is not active.");
      }

      return _reservations.TryReserve(item, owner, now, duration);
    }
  }

  /// <summary>Server-authored assignment. Packet22 itself is server-to-client only.</summary>
  public ReservationResult HandleItemOwner(
    WorldItemHandle handle, EntityUuid playerId, SessionEpoch epoch)
  {
    lock (_worldItems.CommitGate.SyncRoot)
    {
      if (!_worldItems.TryCapture(handle.SlotId, out var item) || item is null
        || item.Handle != handle
        || !_playerSlots.TryCapture(playerId, epoch, handle.WorldScope, out var owner)
        || owner is null)
      {
        return ReservationResult.Unavailable("The item or authenticated owner binding changed.");
      }
      return _reservations.TryReserveFrame(item, owner);
    }
  }

  /// <summary>Atomically clears and reselects packet39 ownership for its authenticated owner.</summary>
  public OwnerCommitResult HandleReleaseItemOwnership(
    WorldItemHandle handle,
    EntityUuid playerId,
    SessionEpoch epoch,
    ReleaseItemOwnershipPacket packet,
    WorldSpatialSnapshot? tiles = null,
    IWorldItemEmergencyStackingCapture? emergency = null)
  {
    ArgumentNullException.ThrowIfNull(packet);
    if (packet.ItemIndex != handle.SlotId
      || !playerId.IsValid
      || !epoch.IsValid)
    {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden, "The ownership release identity is invalid.");
    }

    if (_ownership is null || tiles is null)
    {
      return new OwnerCommitResult(
        OwnerCommitStatus.Unavailable,
        null,
        ProtocolErrorCode.NotReady,
        "The ownership coordinator and captured reselection tiles are unavailable.");
    }

    lock (_worldItems.CommitGate.SyncRoot)
    {
      if (!_playerSlots.TryCapture(playerId, epoch, handle.WorldScope, out var sender)
        || sender is null)
      {
        return new OwnerCommitResult(OwnerCommitStatus.Conflict, null,
          ProtocolErrorCode.StaleEpoch, "The acknowledging session binding is no longer active.");
      }
      OwnerCommitResult result = _ownership.PrepareAcknowledgement(
        handle, sender, tiles, emergency, out var transaction);
      return transaction is null ? result : transaction.Commit().Result;
    }
  }

  /// <summary>Resolves packet 31 coordinates against the authoritative chest index.</summary>
  public ChestAccessResult HandleChestOpen(
    WorldScopeId worldScope,
    EntityUuid playerId,
    SessionEpoch epoch,
    RequestChestOpenPacket packet,
    DateTimeOffset now)
  {
    ArgumentNullException.ThrowIfNull(packet);
    if (!_chests.TryFindByPosition(
      worldScope,
      new WorldTilePoint(packet.X, packet.Y),
      out ChestId chestId,
      out _))
    {
      return new ChestAccessResult(
        new OwnerCommitResult(OwnerCommitStatus.Unavailable, null, ProtocolErrorCode.NotReady, "No authoritative chest exists at the requested coordinate."),
        null,
        null);
    }

    return _chestAccess.TryOpen(chestId, playerId, epoch, worldScope, now);
  }

  /// <summary>Handles packet 32 through the active chest lease and sequence.</summary>
  public ChestAccessResult HandleChestItem(
    WorldScopeId worldScope,
    EntityUuid playerId,
    SessionEpoch epoch,
    PacketSequence sequence,
    SyncChestItemPacket packet,
    DateTimeOffset now)
  {
    ArgumentNullException.ThrowIfNull(packet);
    if (packet.ChestIndex < 0
      || packet.ChestIndex >= ContainerContentFacts.ChestIndexCount
      || packet.Stack < 0
      || packet.ItemType < 0)
    {
      return new ChestAccessResult(
        OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden, "The chest-item packet contains invalid values."),
        null,
        null);
    }

    InventoryItemState item = ToItem(packet.ItemType, packet.Stack, packet.Prefix, false);
    if (!_items.IsValid(item))
    {
      return new ChestAccessResult(
        OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden, "The chest-item packet contains an invalid item."),
        null,
        null);
    }

    return _chestAccess.TrySetSlot(
      new ChestId((ushort)packet.ChestIndex),
      playerId,
      epoch,
      worldScope,
      sequence,
      packet.Slot,
      item,
      now);
  }

  /// <summary>Handles packet 33 name updates under the same chest lease as packet 32.</summary>
  public ChestAccessResult HandlePlayerChestName(
    WorldScopeId worldScope,
    EntityUuid playerId,
    SessionEpoch epoch,
    PacketSequence sequence,
    SyncPlayerChestPacket packet,
    DateTimeOffset now)
  {
    ArgumentNullException.ThrowIfNull(packet);
    if (packet.ChestIndex < 0
      || packet.ChestIndex >= ContainerContentFacts.ChestIndexCount
      || packet.NameLengthIndicator is not (byte.MaxValue or >= 1 and <= ContainerContentFacts.MaximumChestNameLength)
      || packet.NameLengthIndicator == byte.MaxValue && packet.Name is not null
      || packet.NameLengthIndicator != byte.MaxValue && packet.Name is null)
    {
      return new ChestAccessResult(
        OwnerCommitResult.Rejected(ProtocolErrorCode.MalformedFrame, "The chest-name packet has an invalid name indicator or index."),
        null,
        null);
    }

    if (!_chests.TryCapture(new ChestId((ushort)packet.ChestIndex), out ChestSnapshot? chest)
      || chest is null
      || chest.WorldScope != worldScope
      || chest.Position != new WorldTilePoint(packet.ChestX, packet.ChestY))
    {
      return new ChestAccessResult(
        new OwnerCommitResult(OwnerCommitStatus.Stale, null, ProtocolErrorCode.StaleEntity, "The named chest coordinate is not current."),
        chest,
        null);
    }

    return _chestAccess.TrySetName(
      chest.Id,
      playerId,
      epoch,
      worldScope,
      sequence,
      packet.Name ?? string.Empty,
      now);
  }

  /// <summary>Returns the authoritative name for packet 69 without trusting client text.</summary>
  public P04InventoryCommitResult HandleChestNameRequest(
    WorldScopeId worldScope,
    ChestNameRequestPacket packet)
  {
    ArgumentNullException.ThrowIfNull(packet);
    WorldTilePoint position = new(packet.X, packet.Y);
    ChestSnapshot? chest;
    if (packet.ChestIndex == -1)
    {
      if (!_chests.TryFindByPosition(worldScope, position, out _, out chest)
        || chest is null)
      {
        return Rejected(ProtocolErrorCode.NotReady, "The requested chest does not exist.");
      }
    }
    else if (packet.ChestIndex < 0
      || packet.ChestIndex >= ContainerContentFacts.ChestIndexCount)
    {
      return Rejected(ProtocolErrorCode.MalformedFrame, "The chest-name request index is invalid.");
    }
    else if (!_chests.TryCapture(new ChestId((ushort)packet.ChestIndex), out chest)
      || chest is null)
    {
      return Rejected(ProtocolErrorCode.NotReady, "The requested chest does not exist.");
    }

    if (chest is null || chest.WorldScope != worldScope || chest.Position != position)
    {
      return Rejected(ProtocolErrorCode.StaleEntity, "The requested chest identity or coordinate is not current.");
    }

    return new P04InventoryCommitResult(OwnerCommitResult.Applied(chest.Revision), Chest: chest);
  }

  /// <summary>
  /// Handles the semantic create/destroy half of packet 34. P03 maps the verified wire action
  /// to this operation after its tile/container preflight; this class does not guess action values.
  /// </summary>
  public P04InventoryCommitResult HandleChestUpdate(
    ChestUpdateOperation operation,
    WorldScopeId worldScope,
    ChestUpdatesPacket packet,
    ChestGeneration generation,
    CommitVersion expectedRevision,
    ushort kindId,
    string name,
    IReadOnlyList<InventoryItemState> initialItems)
  {
    ArgumentNullException.ThrowIfNull(packet);
    ArgumentNullException.ThrowIfNull(name);
    ArgumentNullException.ThrowIfNull(initialItems);
    if (packet.ChestIndex < 0
      || packet.ChestIndex >= ContainerContentFacts.ChestIndexCount)
    {
      return Rejected(ProtocolErrorCode.Forbidden, "The chest update index is invalid.");
    }

    var id = new ChestId((ushort)packet.ChestIndex);
    if (operation == ChestUpdateOperation.Create)
    {
      OwnerCommitResult create = _chests.Create(
        id,
        worldScope,
        new WorldTilePoint(packet.X, packet.Y),
        kindId,
        name,
        initialItems,
        out ChestSnapshot? created);
      return new P04InventoryCommitResult(create, Chest: created);
    }

    if (operation == ChestUpdateOperation.Destroy)
    {
      OwnerCommitResult destroy = _chests.Destroy(id, worldScope, generation, expectedRevision);
      return new P04InventoryCommitResult(destroy);
    }

    return Rejected(ProtocolErrorCode.UnknownSubtype, "The chest update operation is not recognized.");
  }

  /// <summary>Produces the authoritative packet 88 projection after the owner supplies its values.</summary>
  public P04ProjectionResult<ItemTweakerPacket> CreateItemTweakerProjection(
    WorldItemSnapshot item,
    ItemTweakerPacket packet)
  {
    ArgumentNullException.ThrowIfNull(item);
    ArgumentNullException.ThrowIfNull(packet);
    if (packet.ItemIndex != item.Handle.SlotId || !HasFiniteTweakerValues(packet))
    {
      return P04ProjectionResult<ItemTweakerPacket>.Rejected(
        ProtocolErrorCode.Forbidden,
        "The item-tweaker projection is not bound to a valid authoritative item.");
    }

    return P04ProjectionResult<ItemTweakerPacket>.Applied(packet, item.Revision);
  }

  /// <summary>Handles packet 90 without writing the public world-item store.</summary>
  public P04InventoryCommitResult HandleInstancedItem(
    EntityUuid targetPlayer,
    WorldScopeId worldScope,
    CommitVersion? expectedRevision,
    InstancedItemPacket packet,
    SessionEpoch epoch = default)
  {
    ArgumentNullException.ThrowIfNull(packet);
    if (!epoch.IsValid)
    {
      return Rejected(ProtocolErrorCode.StaleEpoch,
        "The instanced-item projection requires a valid session epoch.");
    }

    if (packet.ItemIndex < 0 || packet.Stack <= 0 || packet.ItemType <= 0)
    {
      return Rejected(ProtocolErrorCode.Forbidden, "The instanced-item packet contains an invalid item.");
    }

    OwnerCommitResult commit = _instancedItems.Upsert(
      targetPlayer,
      worldScope,
      packet.ItemIndex,
      ToItem(packet.ItemType, packet.Stack, packet.Prefix, false),
      new Vector2(packet.PositionX, packet.PositionY),
      new Vector2(packet.VelocityX, packet.VelocityY),
      timeLeft: 0,
      packet.Shimmered ?? false,
      packet.EnemyGrabDelayTime ?? 0,
      expectedRevision,
      epoch,
      out InstancedItemSnapshot? snapshot);
    return new P04InventoryCommitResult(commit, InstancedItem: snapshot);
  }

  /// <summary>Commits packet 110 payment only after the caller's wiring preflight succeeds.</summary>
  public P04InventoryCommitResult HandleWiringPayment(
    EntityUuid authenticatedPlayer,
    CommitVersion expectedRevision,
    MassWireOperationPayPacket packet)
  {
    ArgumentNullException.ThrowIfNull(packet);
    if (packet.ItemType <= 0 || packet.Count <= 0)
    {
      return Rejected(ProtocolErrorCode.Forbidden, "The wiring payment packet contains an invalid item count.");
    }

    InventoryPaymentPreparation preparation = _payments.PreflightPayment(
      authenticatedPlayer,
      expectedRevision,
      packet.ItemType,
      packet.Count);
    if (!preparation.IsPrepared || preparation.Plan is null)
    {
      return new P04InventoryCommitResult(preparation.Result);
    }

    OwnerCommitResult commit = _payments.CommitPayment(
      preparation.Plan,
      out InventorySnapshot? snapshot);
    return new P04InventoryCommitResult(commit, snapshot);
  }

  public P04InventoryCommitResult HandleShimmer(
    WorldItemHandle handle,
    CommitVersion expectedRevision,
    SyncItemsWithShimmerPacket packet)
  {
    ArgumentNullException.ThrowIfNull(packet);
    if (packet.Item.ItemIndex != handle.SlotId
      || !float.IsFinite(packet.ShimmerTime)
      || packet.ShimmerTime < 0)
    {
      return Rejected(ProtocolErrorCode.Forbidden, "The shimmer packet contains an invalid item or time.");
    }

    return UpdateWorldItemProjection(
      handle,
      expectedRevision,
      packet.Item,
      packet.Shimmered,
      packet.ShimmerTime,
      null);
  }

  public P04InventoryCommitResult HandleEnemyPickupDelay(
    WorldItemHandle handle,
    CommitVersion expectedRevision,
    SyncItemCannotBeTakenByEnemiesPacket packet)
  {
    ArgumentNullException.ThrowIfNull(packet);
    if (packet.Item.ItemIndex != handle.SlotId)
    {
      return Rejected(ProtocolErrorCode.Forbidden, "The enemy-pickup packet is not bound to its item slot.");
    }

    return UpdateWorldItemProjection(
      handle,
      expectedRevision,
      packet.Item,
      null,
      null,
      packet.CannotBeTakenTimer);
  }

  public P04InventoryCommitResult HandleItemDespawn(
    WorldItemHandle handle,
    EntityUuid requester,
    SessionEpoch epoch,
    DateTimeOffset now,
    SyncItemDespawnPacket packet)
  {
    ArgumentNullException.ThrowIfNull(packet);
    if (packet.ItemIndex != handle.SlotId || !requester.IsValid || !epoch.IsValid)
    {
      return Rejected(ProtocolErrorCode.Forbidden, "The item-despawn identity is invalid.");
    }

    lock (_worldItems.CommitGate.SyncRoot)
    {
      if (!_worldItems.TryCapture(handle.SlotId, out WorldItemSnapshot? item)
        || item is null || item.Handle != handle)
      {
        return Rejected(ProtocolErrorCode.StaleEntity, "The world item handle is not current.");
      }

      IWorldItemPickupPreparation? player = _preparePickup?.Invoke(item, requester, epoch);
      if (_pickups is null || player is null)
      {
        return new P04InventoryCommitResult(new OwnerCommitResult(
          OwnerCommitStatus.Unavailable, null, ProtocolErrorCode.NotReady,
          "Player-owned pickup facts and effects have not been prepared."));
      }

      OwnerCommitResult preparation = _pickups.Prepare(
        handle, item.Revision, requester, epoch, now, player, out var transaction);
      if (transaction is null)
      {
        return new P04InventoryCommitResult(preparation);
      }

      WorldItemTransactionResult result = transaction.Commit();
      return new P04InventoryCommitResult(result.Result, result.Inventory,
        result.WorldItems.FirstOrDefault());
    }
  }

  public P04InventoryCommitResult HandleChestSize(
    WorldScopeId worldScope,
    ChestGeneration generation,
    CommitVersion expectedRevision,
    SyncChestSizePacket packet)
  {
    ArgumentNullException.ThrowIfNull(packet);
    if (packet.ChestIndex < 0
      || packet.ChestIndex >= ContainerContentFacts.ChestIndexCount
      || packet.Size <= 0)
    {
      return Rejected(ProtocolErrorCode.Forbidden, "The chest-size packet contains an invalid index or size.");
    }

    OwnerCommitResult commit = _chests.Resize(
      new ChestId((ushort)packet.ChestIndex),
      worldScope,
      generation,
      expectedRevision,
      packet.Size,
      _isAllowedChestSize,
      out ChestSnapshot? snapshot);
    return new P04InventoryCommitResult(commit, Chest: snapshot);
  }

  public P04ProjectionResult<ItemPositionPacket> CreateItemPositionProjection(WorldItemSnapshot item)
  {
    ArgumentNullException.ThrowIfNull(item);
    if (item.Handle.SlotId > short.MaxValue)
    {
      return P04ProjectionResult<ItemPositionPacket>.Rejected(
        ProtocolErrorCode.Forbidden,
        "The world item slot cannot be represented by Version4 packet 160.");
    }

    return P04ProjectionResult<ItemPositionPacket>.Applied(
      new ItemPositionPacket
      {
        ItemIndex = (short)item.Handle.SlotId,
        Position = new PacketVector2(item.Position.X, item.Position.Y)
      },
      item.Revision);
  }

  public P04ProjectionResult<ItemOwnerPacket> CreateItemOwnerProjection(
    WorldItemSnapshot item,
    DateTimeOffset now)
  {
    ArgumentNullException.ThrowIfNull(item);
    byte reservedForPlayer = byte.MaxValue;
    if (_reservations.TryGet(item.Handle, now, out WorldItemReservation? reservation)
      && reservation is not null)
    {
      if (reservation.WirePlayerSlot is not { } slot || slot == byte.MaxValue)
      {
        return P04ProjectionResult<ItemOwnerPacket>.Rejected(
          ProtocolErrorCode.NotReady, "The reservation has no captured authenticated wire slot.");
      }

      reservedForPlayer = slot;
    }

    if (item.Handle.SlotId > short.MaxValue)
    {
      return P04ProjectionResult<ItemOwnerPacket>.Rejected(
        ProtocolErrorCode.Forbidden,
        "The world item slot cannot be represented by Version4 packet 22.");
    }

    return P04ProjectionResult<ItemOwnerPacket>.Applied(
      new ItemOwnerPacket
      {
        ItemIndex = (short)item.Handle.SlotId,
        ReservedForPlayer = reservedForPlayer,
        PositionX = item.Position.X,
        PositionY = item.Position.Y
      },
      item.Revision);
  }

  public P04ProjectionResult<SyncItemPacket> CreateSyncItemProjection(WorldItemSnapshot item)
  {
    ArgumentNullException.ThrowIfNull(item);
    if (item.Handle.SlotId > short.MaxValue
      || item.Item.Stack > short.MaxValue
      || item.Item.Type > short.MaxValue
      || item.Item.Type < 0)
    {
      return P04ProjectionResult<SyncItemPacket>.Rejected(
        ProtocolErrorCode.Forbidden,
        "The world item cannot be represented by Version4 packet 21.");
    }

    return P04ProjectionResult<SyncItemPacket>.Applied(
      new SyncItemPacket
      {
        ItemIndex = (short)item.Handle.SlotId,
        PositionX = item.Position.X,
        PositionY = item.Position.Y,
        VelocityX = item.Velocity.X,
        VelocityY = item.Velocity.Y,
        Stack = (short)item.Item.Stack,
        Prefix = item.Item.Prefix,
        ItemType = (short)item.Item.Type,
        Shimmered = item.IsShimmering,
        ShimmerTime = item.ShimmerTime,
        EnemyGrabDelayTime = item.EnemyPickupDelay > byte.MaxValue
          ? byte.MaxValue
          : (byte)item.EnemyPickupDelay
      },
      item.Revision);
  }

  public P04ProjectionResult<InstancedItemPacket> CreateInstancedItemProjection(
    InstancedItemSnapshot item)
  {
    ArgumentNullException.ThrowIfNull(item);
    if (item.ItemIndex > short.MaxValue
      || item.Item.Stack > short.MaxValue
      || item.Item.Type > short.MaxValue)
    {
      return P04ProjectionResult<InstancedItemPacket>.Rejected(
        ProtocolErrorCode.Forbidden,
        "The instanced item cannot be represented by Version4 packet 90.");
    }

    return P04ProjectionResult<InstancedItemPacket>.Applied(
      new InstancedItemPacket
      {
        ItemIndex = (short)item.ItemIndex,
        PositionX = item.Position.X,
        PositionY = item.Position.Y,
        VelocityX = item.Velocity.X,
        VelocityY = item.Velocity.Y,
        Stack = (short)item.Item.Stack,
        Prefix = item.Item.Prefix,
        ItemType = (short)item.Item.Type,
        Shimmered = item.IsShimmering,
        ShimmerTime = 0,
        EnemyGrabDelayTime = item.EnemyPickupDelay > byte.MaxValue
          ? byte.MaxValue
          : (byte)item.EnemyPickupDelay
      },
      item.Revision);
  }

  private P04InventoryCommitResult UpdateWorldItemProjection(
    WorldItemHandle handle,
    CommitVersion expectedRevision,
    PacketItemSyncData packet,
    bool? isShimmering,
    float? shimmerTime,
    int? enemyPickupDelay)
  {
    if (!_worldItems.TryCapture(handle.SlotId, out WorldItemSnapshot? current) || current is null)
    {
      return Rejected(ProtocolErrorCode.NotReady, "The world item is not active.");
    }

    if (packet.ItemIndex != handle.SlotId
      || packet.ItemType <= 0
      || packet.Stack <= 0)
    {
      return Rejected(ProtocolErrorCode.Forbidden, "The item projection contains an invalid item body.");
    }

    float updatedShimmerTime = shimmerTime ?? current.ShimmerTime;
    if (!float.IsFinite(updatedShimmerTime) || updatedShimmerTime < 0)
    {
      return Rejected(ProtocolErrorCode.Forbidden, "The item projection shimmer time is invalid.");
    }

    var mutation = new WorldItemMutation(
      ToItem(packet.ItemType, packet.Stack, packet.Prefix, false),
      new Vector2(packet.PositionX, packet.PositionY),
      new Vector2(packet.VelocityX, packet.VelocityY),
      current.TimeLeft,
      isShimmering ?? current.IsShimmering,
      enemyPickupDelay ?? current.EnemyPickupDelay,
      updatedShimmerTime);
    OwnerCommitResult commit = _worldItems.Update(handle, expectedRevision, mutation, out WorldItemSnapshot? updated);
    return new P04InventoryCommitResult(commit, WorldItem: updated);
  }

  private static InventoryItemState ToItem(int type, int stack, byte prefix, bool favorited)
  {
    return type == 0 || stack == 0
      ? InventoryItemState.Empty
      : new InventoryItemState(type, stack, prefix, favorited);
  }

  private static bool HasFiniteTweakerValues(ItemTweakerPacket packet)
  {
    return (!packet.Knockback.HasValue || float.IsFinite(packet.Knockback.Value))
      && (!packet.ShootSpeed.HasValue || float.IsFinite(packet.ShootSpeed.Value))
      && (!packet.Extra?.Scale.HasValue ?? true || float.IsFinite(packet.Extra!.Scale!.Value));
  }

  private static P04InventoryCommitResult Rejected(ProtocolErrorCode code, string detail)
  {
    return new P04InventoryCommitResult(OwnerCommitResult.Rejected(code, detail));
  }
}

public enum ChestUpdateOperation
{
  Create,
  Destroy
}

public sealed record P04InventoryCommitResult(
  OwnerCommitResult Commit,
  InventorySnapshot? Inventory = null,
  WorldItemSnapshot? WorldItem = null,
  ChestSnapshot? Chest = null,
  InstancedItemSnapshot? InstancedItem = null);

public sealed record P04ProjectionResult<T>(OwnerCommitResult Commit, T? Projection)
  where T : class
{
  public static P04ProjectionResult<T> Applied(T projection, CommitVersion version) =>
    new(OwnerCommitResult.Applied(version), projection);

  public static P04ProjectionResult<T> Rejected(ProtocolErrorCode code, string detail) =>
    new(OwnerCommitResult.Rejected(code, detail), null);
}
