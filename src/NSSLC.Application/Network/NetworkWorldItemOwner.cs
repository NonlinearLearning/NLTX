using EntityEcs;
using EntityEcs.Components;
using Terraria.Items;
using Terraria.NonAuthoritative.Persistence;
using Terraria.Relationships;
using Terraria.WorldStorage;
using ItemEntityRef = Terraria.Relationships.ItemEntityRef;

namespace Terraria.Network;

/// <summary>
/// Owns the Steam world-item slot projection for one NetworkWorldOwner session, applies
/// authenticated packet 21 updates and packet 151 removals, and captures packet 22 ownership
/// state on its EntityRuntime thread.
/// </summary>
/// <remarks>
/// Host gameplay registers an already-created, authorized item before clients can address it.
/// The network command can update its motion, but cannot create an item or alter its type,
/// prefix, or quantity. Wire slot IDs have no generation, so each use resolves the current
/// WorldStorage generation and exact item reference again. Registration records the host's
/// reuse protection deadline, and the injected tick provider is read inside the owner callback.
/// </remarks>
public sealed class NetworkWorldItemOwner
{
  private const int MaximumSteamItemSlots = 400;
  private const byte KnownSteamItemSyncFlags = 0x0F;
  private const int SectionWidthInTiles = 200;
  private const int SectionHeightInTiles = 150;
  private const int TileSize = 16;
  private const float WorldItemWidth = 16.0f;
  private const float WorldItemHeight = 16.0f;

  private readonly NetworkWorldOwner _worldOwner;
  private readonly NetworkPlayerOwner _players;
  private readonly WorldItemMotionSystem _worldItemMotionSystem = new();
  private readonly Func<long> _currentTick;

  public NetworkWorldItemOwner(
    NetworkWorldOwner worldOwner,
    NetworkPlayerOwner players,
    Func<long> currentTick)
  {
    _worldOwner = worldOwner ?? throw new ArgumentNullException(nameof(worldOwner));
    _players = players ?? throw new ArgumentNullException(nameof(players));
    _currentTick = currentTick ?? throw new ArgumentNullException(nameof(currentTick));
  }

  /// <summary>
  /// Registers a world item created by trusted server gameplay and assigns its Steam slot.
  /// Re-registering the same entity returns its existing slot and generation.
  /// </summary>
  public ValueTask<NetworkWorldItemSlotRegistrationResult> RegisterWorldItemAsync(
    ItemEntityRef item,
    CancellationToken cancellationToken = default)
  {
    return RegisterWorldItemAsync(
      item,
      reuseBlockedUntilTick: 0,
      cancellationToken: cancellationToken);
  }

  /// <summary>
  /// Registers a trusted world item and records the host's current slot reuse protection
  /// deadline. The deadline is an authoritative world tick, not a packet supplied value.
  /// </summary>
  public ValueTask<NetworkWorldItemSlotRegistrationResult> RegisterWorldItemAsync(
    ItemEntityRef item,
    long reuseBlockedUntilTick,
    CancellationToken cancellationToken = default)
  {
    if (item.IsEmpty)
    {
      return ValueTask.FromResult(RejectedRegistration("EmptyWorldItem"));
    }

    if (reuseBlockedUntilTick < 0)
    {
      return ValueTask.FromResult(RejectedRegistration("InvalidSlotReuseDeadline"));
    }

    cancellationToken.ThrowIfCancellationRequested();
    return _worldOwner.InvokeAsync(
      session => RegisterWorldItemOnOwnerThread(session, item, reuseBlockedUntilTick),
      cancellationToken);
  }

  /// <summary>
  /// Captures active world items intersecting one section's legacy item-sync bounds.
  /// The caller routes the detached results only to the client whose section was activated.
  /// </summary>
  public ValueTask<IReadOnlyList<NetworkWorldItemPositionProjection>>
    CaptureSectionItemsAsync(
      int sectionX,
      int sectionY,
      CancellationToken cancellationToken = default)
  {
    if (sectionX < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(sectionX));
    }

    if (sectionY < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(sectionY));
    }

    long left = (long)sectionX * SectionWidthInTiles * TileSize - TileSize;
    long top = (long)sectionY * SectionHeightInTiles * TileSize - TileSize;
    long right = left + SectionWidthInTiles * TileSize + TileSize * 2;
    long bottom = top + SectionHeightInTiles * TileSize + TileSize * 2;
    cancellationToken.ThrowIfCancellationRequested();
    return _worldOwner.InvokeAsync(
      session => CaptureSectionItemsOnOwnerThread(session, left, top, right, bottom),
      cancellationToken);
  }

  /// <summary>Captures the detached Steam packet-22 ownership state for one registered item.</summary>
  public ValueTask<NetworkWorldItemOwnershipProjection?> CaptureOwnershipProjectionAsync(
    int itemIndex,
    CancellationToken cancellationToken = default)
  {
    if (itemIndex < 0 || itemIndex >= MaximumSteamItemSlots)
    {
      return ValueTask.FromResult<NetworkWorldItemOwnershipProjection?>(null);
    }

    cancellationToken.ThrowIfCancellationRequested();
    return _worldOwner.InvokeAsync(
      session => CaptureOwnershipProjectionOnOwnerThread(session, itemIndex),
      cancellationToken);
  }

  /// <summary>
  /// Captures packet-22 state while the caller is already inside the world-owner callback.
  /// </summary>
  public NetworkWorldItemOwnershipProjection? CaptureOwnershipProjectionOnOwnerThread(
    LoadedWorldSession session,
    int itemIndex)
  {
    ArgumentNullException.ThrowIfNull(session);
    if (itemIndex < 0 || itemIndex >= MaximumSteamItemSlots)
    {
      return null;
    }

    long currentTick = _currentTick();
    if (currentTick < 0)
    {
      throw new InvalidOperationException(
        "The authoritative world tick cannot be negative when capturing item ownership.");
    }

    EntitySlotStore<WorldEntityState, WorldItemSlot> slots = session.Storage.WorldItems;
    if (!slots.TryGetOccupiedAt(
          itemIndex,
          out WorldItemSlot slot,
          out uint generation,
          out WorldEntityState? slotState) ||
      slotState is not NetworkWorldItemSlotState worldItemState ||
      !slots.TryGet(slot, generation, out WorldEntityState? currentState) ||
      !ReferenceEquals(slotState, currentState) ||
      !TryCaptureWorldItem(
        session,
        worldItemState.Item,
        out _,
        out WorldItemSnapshot snapshot) ||
      !IsUsableWorldItem(snapshot, currentTick))
    {
      return null;
    }

    byte reservedForPlayer = byte.MaxValue;
    int timeToKeepReservation = 0;
    if (snapshot.Reservation.HasReservationAt(currentTick) &&
      snapshot.Reservation.ReservedFor is RuntimeEntityId reservedFor &&
      _players.TryGetPlayerSlotOnOwnerThread(session, reservedFor, out byte ownerSlot))
    {
      reservedForPlayer = ownerSlot;
      timeToKeepReservation = ToRemainingTimer(
        snapshot.Reservation.ReservationExpiresAt,
        currentTick);
    }

    byte grabDelayPlayer = 0;
    int grabDelayTime = 0;
    if (snapshot.Reservation.NoGrabUntilTick > currentTick)
    {
      grabDelayPlayer = byte.MaxValue;
      grabDelayTime = ToRemainingTimer(
        snapshot.Reservation.NoGrabUntilTick,
        currentTick);
    }

    return new NetworkWorldItemOwnershipProjection(
      checked((short)itemIndex),
      reservedForPlayer,
      timeToKeepReservation,
      grabDelayPlayer,
      grabDelayTime,
      snapshot.Position.X,
      snapshot.Position.Y);
  }

  /// <summary>
  /// Registers an item while an existing NetworkWorldOwner callback is already running.
  /// The caller must pass the same session currently owned by that callback.
  /// </summary>
  public NetworkWorldItemSlotRegistrationResult RegisterWorldItemOnOwnerThread(
    LoadedWorldSession session,
    ItemEntityRef item)
  {
    return RegisterWorldItemOnOwnerThread(session, item, reuseBlockedUntilTick: 0);
  }

  /// <summary>
  /// Registers an item during an existing owner callback with the host's reuse deadline.
  /// </summary>
  public NetworkWorldItemSlotRegistrationResult RegisterWorldItemOnOwnerThread(
    LoadedWorldSession session,
    ItemEntityRef item,
    long reuseBlockedUntilTick)
  {
    ArgumentNullException.ThrowIfNull(session);
    if (item.IsEmpty)
    {
      return RejectedRegistration("EmptyWorldItem");
    }

    if (reuseBlockedUntilTick < 0)
    {
      return RejectedRegistration("InvalidSlotReuseDeadline");
    }

    return RegisterWorldItemOnOwnerThreadCore(session, item, reuseBlockedUntilTick);
  }

  /// <summary>
  /// Commits a sender-owned world-item motion update and returns any relay projection.
  /// </summary>
  public ValueTask<PacketHandlingResult> ApplySyncItemAsync(
    NetworkSessionContext context,
    NetworkWorldItemSyncCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(context);
    cancellationToken.ThrowIfCancellationRequested();
    return _worldOwner.InvokeAsync(
      session => ApplySyncItemOnOwnerThread(session, context, command),
      cancellationToken);
  }

  /// <summary>Applies packet 151 for an authenticated reservation owner.</summary>
  public ValueTask<PacketHandlingResult> ApplyDespawnItemAsync(
    NetworkSessionContext context,
    NetworkWorldItemDespawnCommand command,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(context);
    cancellationToken.ThrowIfCancellationRequested();
    return _worldOwner.InvokeAsync(
      session => ApplyDespawnItemOnOwnerThread(session, context, command),
      cancellationToken);
  }

  private NetworkWorldItemSlotRegistrationResult RegisterWorldItemOnOwnerThreadCore(
    LoadedWorldSession session,
    ItemEntityRef item,
    long reuseBlockedUntilTick)
  {
    long currentTick = _currentTick();
    if (currentTick < 0 || reuseBlockedUntilTick < 0 ||
      item.Reference.Scope != EntityReferenceScope.Item ||
      item.Reference.RuntimeId != session.EntityRuntime.RuntimeId ||
      !TryCaptureWorldItem(session, item, out _, out WorldItemSnapshot snapshot) ||
      !IsUsableWorldItem(snapshot, currentTick))
    {
      return RejectedRegistration("InvalidWorldItemRegistration");
    }

    EntitySlotStore<WorldEntityState, WorldItemSlot> slots = session.Storage.WorldItems;
    for (int index = 0; index < MaximumSteamItemSlots; index++)
    {
      if (!slots.TryGetOccupiedAt(
            index,
            out WorldItemSlot existingSlot,
            out uint existingGeneration,
            out WorldEntityState? existingState))
      {
        continue;
      }

      if (existingState is NetworkWorldItemSlotState registered && registered.Item == item)
      {
        registered.ExtendReuseBlockedUntil(reuseBlockedUntilTick);
        return new NetworkWorldItemSlotRegistrationResult(
          true,
          checked((short)existingSlot.Value),
          existingGeneration,
          null);
      }
    }

    for (int index = 0; index < MaximumSteamItemSlots; index++)
    {
      var slot = new WorldItemSlot(index);
      var state = new NetworkWorldItemSlotState(item, reuseBlockedUntilTick);
      if (slots.TryAllocateAt(slot, state, out uint generation))
      {
        return new NetworkWorldItemSlotRegistrationResult(
          true,
          checked((short)index),
          generation,
          null);
      }
    }

    return RejectedRegistration("WorldItemSlotCapacityExceeded");
  }

  private IReadOnlyList<NetworkWorldItemPositionProjection> CaptureSectionItemsOnOwnerThread(
    LoadedWorldSession session,
    long left,
    long top,
    long right,
    long bottom)
  {
    long currentTick = _currentTick();
    if (currentTick < 0)
    {
      throw new InvalidOperationException(
        "The authoritative world tick cannot be negative when capturing item sections.");
    }

    EntitySlotStore<WorldEntityState, WorldItemSlot> slots = session.Storage.WorldItems;
    var projections = new List<NetworkWorldItemPositionProjection>();
    for (int index = 0; index < MaximumSteamItemSlots; index++)
    {
      if (!slots.TryGetOccupiedAt(
            index,
            out WorldItemSlot slot,
            out uint generation,
            out WorldEntityState? slotState) ||
        slotState is not NetworkWorldItemSlotState worldItemState ||
        !slots.TryGet(slot, generation, out WorldEntityState? currentState) ||
        !ReferenceEquals(slotState, currentState) ||
        !TryCaptureWorldItem(
          session,
          worldItemState.Item,
          out _,
          out WorldItemSnapshot snapshot) ||
        !IsUsableWorldItem(snapshot, currentTick) ||
        snapshot.WorldItem.IsInstanced)
      {
        continue;
      }

      double centerX = snapshot.Position.X + WorldItemWidth / 2.0;
      double centerY = snapshot.Position.Y + WorldItemHeight / 2.0;
      if (centerX < left || centerX >= right || centerY < top || centerY >= bottom)
      {
        continue;
      }

      projections.Add(new NetworkWorldItemPositionProjection(
        checked((short)index),
        snapshot.Position.X,
        snapshot.Position.Y));
    }

    return Array.AsReadOnly(projections.ToArray());
  }

  private PacketHandlingResult ApplyDespawnItemOnOwnerThread(
    LoadedWorldSession session,
    NetworkSessionContext context,
    NetworkWorldItemDespawnCommand command)
  {
    if (context.Stage != NetworkSessionStage.Active ||
      context.Actor.GameSessionKey == Guid.Empty ||
      context.Actor.PlayerSlot == byte.MaxValue ||
      context.WorldRuntimeId is not { } expectedRuntimeId ||
      expectedRuntimeId != session.EntityRuntime.RuntimeId ||
      !_players.TryResolveOnOwnerThread(session, context, out EntityReference playerReference))
    {
      return Reject("StaleItemSenderBinding");
    }

    if (command.ItemIndex < 0 || command.ItemIndex >= MaximumSteamItemSlots)
    {
      return Reject("InvalidWorldItemSlot");
    }

    long currentTick = _currentTick();
    if (currentTick < 0)
    {
      return Reject("InvalidWorldTick");
    }

    EntitySlotStore<WorldEntityState, WorldItemSlot> slots = session.Storage.WorldItems;
    if (!slots.TryGetOccupiedAt(
          command.ItemIndex,
          out WorldItemSlot slot,
          out uint generation,
          out WorldEntityState? slotState) ||
      slotState is not NetworkWorldItemSlotState worldItemState ||
      !slots.TryGet(slot, generation, out WorldEntityState? currentState) ||
      !ReferenceEquals(slotState, currentState) ||
      !TryCaptureWorldItem(
        session,
        worldItemState.Item,
        out RuntimeEntityHandle itemHandle,
        out WorldItemSnapshot snapshot) ||
      !IsUsableWorldItem(snapshot, currentTick))
    {
      return Reject("StaleWorldItemSlot");
    }

    if (currentTick < worldItemState.ReuseBlockedUntilTick)
    {
      return Reject("WorldItemSlotReuseProtected");
    }

    RuntimeEntityId playerEntity = RuntimeEntityId.FromEntityReference(playerReference);
    if (snapshot.Reservation.ReservedFor != playerEntity)
    {
      return Reject("WorldItemReservationOwnerMismatch");
    }

    if (!session.EntityRuntime.TryBeginTermination(itemHandle))
    {
      return Reject("WorldItemRemovalConflict");
    }

    if (!session.EntityRuntime.TryRemoveEntity(itemHandle))
    {
      throw new InvalidOperationException(
        "An authorized world item could not be removed after beginning termination.");
    }

    if (!slots.TryRelease(slot, generation, out WorldEntityState? releasedState) ||
      !ReferenceEquals(worldItemState, releasedState))
    {
      throw new InvalidOperationException(
        "The world item entity was removed but its generated network slot could not be released.");
    }

    var projection = new NetworkWorldItemDespawnProjection(checked((short)slot.Value));
    return new PacketHandlingResult(
      accepted: true,
      [new OutboundDispatch(projection, PacketDispatchKind.AllActiveExceptSender)]);
  }

  private PacketHandlingResult ApplySyncItemOnOwnerThread(
    LoadedWorldSession session,
    NetworkSessionContext context,
    NetworkWorldItemSyncCommand command)
  {
    if (context.Stage != NetworkSessionStage.Active ||
      context.Actor.GameSessionKey == Guid.Empty ||
      context.Actor.PlayerSlot == byte.MaxValue ||
      context.WorldRuntimeId is not { } expectedRuntimeId ||
      expectedRuntimeId != session.EntityRuntime.RuntimeId ||
      !_players.TryResolveOnOwnerThread(session, context, out EntityReference playerReference))
    {
      return Reject("StaleItemSenderBinding");
    }

    if ((command.StateFlags & ~KnownSteamItemSyncFlags) != 0)
    {
      return Reject("UnsupportedWorldItemStateFlags");
    }

    bool hasShimmerExtension = (command.StateFlags & (1 << 2)) != 0;
    bool hasEnemyGrabDelayExtension = (command.StateFlags & (1 << 3)) != 0;
    if (hasShimmerExtension != command.Shimmered.HasValue ||
      hasShimmerExtension != command.ShimmerTime.HasValue ||
      hasEnemyGrabDelayExtension != command.EnemyGrabDelayTime.HasValue)
    {
      return Reject("InvalidWorldItemExtensions");
    }

    float shimmerTime = command.ShimmerTime ?? 0.0f;
    if (!float.IsFinite(shimmerTime) || shimmerTime < 0.0f)
    {
      return Reject("InvalidWorldItemState");
    }

    if (command.ItemIndex == MaximumSteamItemSlots)
    {
      return Reject("UnauthenticatedWorldItemCreation");
    }

    if (command.ItemIndex < 0 || command.ItemIndex >= MaximumSteamItemSlots ||
      command.ItemType <= 0 || command.Stack <= 0 ||
      !float.IsFinite(command.PositionX) || !float.IsFinite(command.PositionY) ||
      !float.IsFinite(command.VelocityX) || !float.IsFinite(command.VelocityY))
    {
      return Reject("InvalidWorldItemState");
    }

    long currentTick = _currentTick();
    if (currentTick < 0)
    {
      return Reject("InvalidWorldTick");
    }

    EntitySlotStore<WorldEntityState, WorldItemSlot> slots = session.Storage.WorldItems;
    if (!slots.TryGetOccupiedAt(
          command.ItemIndex,
          out WorldItemSlot slot,
          out uint generation,
          out WorldEntityState? slotState) ||
      slotState is not NetworkWorldItemSlotState worldItemState ||
      !slots.TryGet(slot, generation, out WorldEntityState? currentState) ||
      !ReferenceEquals(slotState, currentState) ||
      !TryCaptureWorldItem(
        session,
        worldItemState.Item,
        out RuntimeEntityHandle itemHandle,
        out WorldItemSnapshot snapshot) ||
      !IsUsableWorldItem(snapshot, currentTick))
    {
      return Reject("StaleWorldItemSlot");
    }

    if (currentTick < worldItemState.ReuseBlockedUntilTick)
    {
      return Reject("WorldItemSlotReuseProtected");
    }

    RuntimeEntityId playerEntity = RuntimeEntityId.FromEntityReference(playerReference);
    if (!snapshot.Reservation.HasReservationAt(currentTick) ||
      snapshot.Reservation.ReservedFor != playerEntity)
    {
      return Reject("WorldItemReservationOwnerMismatch");
    }

    if (command.ItemType != snapshot.Instance.DefinitionRef.ContentId.TypeId ||
      command.Stack != snapshot.Stack.Quantity ||
      command.Prefix != snapshot.Instance.PrefixId)
    {
      return Reject("UnauthorizedWorldItemMutation");
    }

    RuntimeEntityId itemEntity = RuntimeEntityId.FromItemReference(snapshot.Item);
    var stateCommand = new WorldItemSynchronizedStateCommand(
      itemEntity,
      snapshot.ReplicationId,
      currentTick,
      snapshot.WorldItem.Revision,
      new WorldPosition(command.PositionX, command.PositionY),
      new WorldVector(command.VelocityX, command.VelocityY),
      command.Shimmered ?? false,
      shimmerTime,
      command.EnemyGrabDelayTime ?? 0);
    WorldItemMotionResult stateResult = default;
    bool reservationOwnerMismatch = false;
    bool edited = session.EntityRuntime.TryEditComponents<
        WorldItemStateComponent,
        LocationComponent,
        VelocityComponent,
        WorldItemComponent,
        WorldItemReservationComponent>(
        itemHandle,
        (ref WorldItemStateComponent worldItemState,
          ref LocationComponent position,
          ref VelocityComponent velocity,
          ref WorldItemComponent worldItem,
          ref WorldItemReservationComponent reservation) =>
        {
          if (!reservation.HasReservationAt(currentTick) ||
            reservation.ReservedFor != playerEntity)
          {
            reservationOwnerMismatch = true;
            return;
          }

          if (reservation.ReservationRevision != snapshot.Reservation.ReservationRevision ||
            reservation.ReservationId != snapshot.Reservation.ReservationId ||
            reservation.ReservationExpiresAt != snapshot.Reservation.ReservationExpiresAt)
          {
            return;
          }

          stateResult = _worldItemMotionSystem.ApplySynchronizedState(
            stateCommand,
            itemEntity,
            worldItemState.ReplicationId,
            worldItem,
            new WorldPosition(position.X, position.Y),
            new WorldVector(velocity.X, velocity.Y));
          if (!stateResult.Applied || !stateResult.Changed)
          {
            return;
          }

          position.X = stateCommand.Position.X;
          position.Y = stateCommand.Position.Y;
          velocity.X = stateCommand.Velocity.X;
          velocity.Y = stateCommand.Velocity.Y;
        });
    if (reservationOwnerMismatch)
    {
      return Reject("WorldItemReservationOwnerMismatch");
    }

    if (!edited || !stateResult.Applied)
    {
      return Reject("WorldItemUpdateConflict");
    }

    if (!stateResult.Changed)
    {
      return new PacketHandlingResult(accepted: true);
    }

    var projection = new NetworkWorldItemSyncProjection(
      checked((short)slot.Value),
      command.ItemType,
      command.Stack,
      command.Prefix,
      command.PositionX,
      command.PositionY,
      command.VelocityX,
      command.VelocityY,
      stateCommand.IsShimmered,
      stateCommand.ShimmerTime,
      stateCommand.EnemyGrabDelayTime);
    return new PacketHandlingResult(
      accepted: true,
      [new OutboundDispatch(projection, PacketDispatchKind.AllActiveExceptSender)]);
  }

  private static bool TryCaptureWorldItem(
    LoadedWorldSession session,
    ItemEntityRef item,
    out RuntimeEntityHandle handle,
    out WorldItemSnapshot snapshot)
  {
    if (item.Reference.Scope != EntityReferenceScope.Item ||
      item.Reference.RuntimeId != session.EntityRuntime.RuntimeId ||
      !session.EntityRuntime.TryResolve(item.Reference, out handle) ||
      !session.EntityRuntime.TryCapture(
        handle,
        static (ItemInstanceComponent component) =>
          new ItemInstanceSnapshot(
            component.DefinitionRef,
            component.PersistentInstanceId,
            component.PrefixId),
        out ItemInstanceSnapshot instance) ||
      !session.EntityRuntime.TryCapture(
        handle,
        static (ItemStackComponent component) => component,
        out ItemStackComponent stack) ||
      !session.EntityRuntime.TryCapture(
        handle,
        static (LocationComponent component) => new WorldPosition(component.X, component.Y),
        out WorldPosition position) ||
      !session.EntityRuntime.TryCapture(
        handle,
        static (VelocityComponent component) => new WorldVector(component.X, component.Y),
        out WorldVector velocity) ||
      !session.EntityRuntime.TryCapture(
        handle,
        static (WorldItemComponent component) =>
          new WorldItemLifecycleSnapshot(
            component.SpawnedAtTick,
            component.DespawnAtTick,
            component.IsInstanced,
            component.Revision,
            component.ShimmerTime),
        out WorldItemLifecycleSnapshot worldItem) ||
      !session.EntityRuntime.TryCapture(
        handle,
        static (WorldItemStateComponent component) => component.ReplicationId,
        out ReplicationId replicationId) ||
      !session.EntityRuntime.TryCapture(
        handle,
        static (WorldItemReservationComponent component) =>
          new WorldItemReservationSnapshot(
            component.ReservationId,
            component.ReservedFor,
            component.ReservationExpiresAt,
            component.NoGrabUntilTick,
            component.ReservationRevision),
        out WorldItemReservationSnapshot reservation))
    {
      handle = default;
      snapshot = default;
      return false;
    }

    snapshot = new WorldItemSnapshot(
      item,
      instance,
      stack,
      position,
      velocity,
      worldItem,
      replicationId,
      reservation);
    return true;
  }

  private static bool IsUsableWorldItem(WorldItemSnapshot snapshot, long currentTick)
  {
    return snapshot.Instance.DefinitionRef.IsKnown &&
      snapshot.Instance.DefinitionRef.ContentId.TypeId is > 0 and <= short.MaxValue &&
      snapshot.Instance.PersistentInstanceId.IsAssigned &&
      snapshot.Instance.PrefixId is >= 0 and <= byte.MaxValue &&
      snapshot.Stack.IsValid &&
      snapshot.Stack.Quantity > 0 &&
      snapshot.Stack.Quantity <= short.MaxValue &&
      snapshot.Stack.MaximumQuantity > 0 &&
      float.IsFinite(snapshot.Position.X) &&
      float.IsFinite(snapshot.Position.Y) &&
      float.IsFinite(snapshot.Velocity.X) &&
      float.IsFinite(snapshot.Velocity.Y) &&
      snapshot.WorldItem.SpawnedAtTick >= 0 &&
      snapshot.WorldItem.SpawnedAtTick <= currentTick &&
      snapshot.WorldItem.Revision >= 0 &&
      float.IsFinite(snapshot.WorldItem.ShimmerTime) &&
      snapshot.WorldItem.ShimmerTime >= 0.0f &&
      !snapshot.WorldItem.IsExpiredAt(currentTick) &&
      snapshot.ReplicationId.IsAssigned &&
      snapshot.Reservation.ReservationRevision >= 0;
  }

  private static int ToRemainingTimer(long? deadline, long currentTick)
  {
    if (!deadline.HasValue || deadline.Value <= currentTick)
    {
      return 0;
    }

    return (int)Math.Min(deadline.Value - currentTick, int.MaxValue);
  }

  private static int ToRemainingTimer(long deadline, long currentTick)
  {
    return (int)Math.Min(deadline - currentTick, int.MaxValue);
  }

  private static NetworkWorldItemSlotRegistrationResult RejectedRegistration(string code) =>
    new(false, -1, 0, code);

  private static PacketHandlingResult Reject(string code) =>
    new(accepted: false, rejectionCode: code);

  private sealed class NetworkWorldItemSlotState : WorldEntityState
  {
    public NetworkWorldItemSlotState(ItemEntityRef item, long reuseBlockedUntilTick)
    {
      Item = item;
      ReuseBlockedUntilTick = reuseBlockedUntilTick;
    }

    public ItemEntityRef Item { get; }
    public long ReuseBlockedUntilTick { get; private set; }

    public void ExtendReuseBlockedUntil(long deadline)
    {
      ReuseBlockedUntilTick = Math.Max(ReuseBlockedUntilTick, deadline);
    }
  }

  private readonly record struct ItemInstanceSnapshot(
    ItemDefinitionRef DefinitionRef,
    PersistentItemId PersistentInstanceId,
    int PrefixId);

  private readonly record struct WorldItemLifecycleSnapshot(
    long SpawnedAtTick,
    long? DespawnAtTick,
    bool IsInstanced,
    long Revision,
    float ShimmerTime)
  {
    public bool IsExpiredAt(long currentTick) =>
      DespawnAtTick.HasValue && currentTick >= DespawnAtTick.Value;
  }

  private readonly record struct WorldItemReservationSnapshot(
    ReservationId? ReservationId,
    RuntimeEntityId? ReservedFor,
    long? ReservationExpiresAt,
    long NoGrabUntilTick,
    long ReservationRevision)
  {
    public bool HasReservationAt(long currentTick) =>
      ReservationId is { IsAssigned: true } &&
      ReservedFor.HasValue &&
      ReservationExpiresAt.HasValue &&
      currentTick < ReservationExpiresAt.Value;
  }

  private readonly record struct WorldItemSnapshot(
    ItemEntityRef Item,
    ItemInstanceSnapshot Instance,
    ItemStackComponent Stack,
    WorldPosition Position,
    WorldVector Velocity,
    WorldItemLifecycleSnapshot WorldItem,
    ReplicationId ReplicationId,
    WorldItemReservationSnapshot Reservation);
}
