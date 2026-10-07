using EntityEcs;
using EntityEcs.Components;
using NSSLC.Infrastructure.Network;
using Terraria.Items;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.Network;
using Terraria.NonAuthoritative.Persistence;
using Terraria.Relationships;
using Terraria.WorldStorage;

namespace Terraria.Items.NetworkOwner.Verification;

internal static class NetworkWorldItemOwnerVerification
{
  private static int _nextReplicationId;

  public static async Task RunAsync()
  {
    var activeBindings = new HashSet<(ConnectionIdentity, SenderBinding)>();
    long currentTick = 10;
    CancellationTokenSource? cancelDuringItemCapture = null;
    var connection = new ConnectionIdentity(Guid.NewGuid(), 1);
    var binding = new SenderBinding(5, Guid.NewGuid());
    activeBindings.Add((connection, binding));

    await using var worldOwner = new NetworkWorldOwner(
      static () => new LoadedWorldSession(maximumWorldItemSlots: 400));
    await worldOwner.Ready.ConfigureAwait(false);
    EntityRuntimeId runtimeId = await worldOwner.InvokeAsync(
      static session => session.WorldRuntimeId).ConfigureAwait(false);
    var context = new NetworkSessionContext(
      connection,
      "steam-items-owner-verification",
      NetworkSessionStage.Active,
      binding,
      IsHost: false,
      runtimeId);
    var playerOwner = new NetworkPlayerOwner(
      worldOwner,
      current => activeBindings.Contains((current.Connection, current.Actor)));
    NetworkPlayerBindingResult playerResult = await playerOwner.EnsurePlayerAsync(context)
      .ConfigureAwait(false);
    Assert(playerResult.Succeeded && playerResult.Player.HasValue,
      "the authenticated player is bound to this world session");
    EntityReference playerReference = playerResult.Player!.Value.Reference;

    ItemEntityRef item = await worldOwner.InvokeAsync(
      session => CreateWorldItem(
        session,
        playerReference,
        noGrabUntilTick: 20)).ConfigureAwait(false);
    var owner = new NetworkWorldItemOwner(
      worldOwner,
      playerOwner,
      () =>
      {
        cancelDuringItemCapture?.Cancel();
        return System.Threading.Interlocked.Read(ref currentTick);
      });

    ItemEntityRef unrepresentableTypeItem = await worldOwner.InvokeAsync(
      session => CreateWorldItem(
        session,
        playerReference,
        itemType: short.MaxValue + 1)).ConfigureAwait(false);
    NetworkWorldItemSlotRegistrationResult unrepresentableTypeRegistration =
      await owner.RegisterWorldItemAsync(unrepresentableTypeItem).ConfigureAwait(false);
    Assert(!unrepresentableTypeRegistration.Succeeded &&
      unrepresentableTypeRegistration.RejectionCode == "InvalidWorldItemRegistration",
      "an item whose type cannot fit Steam's signed 16-bit field receives no network slot");

    ItemEntityRef unrepresentableStackItem = await worldOwner.InvokeAsync(
      session => CreateWorldItem(
        session,
        playerReference,
        quantity: short.MaxValue + 1)).ConfigureAwait(false);
    NetworkWorldItemSlotRegistrationResult unrepresentableStackRegistration =
      await owner.RegisterWorldItemAsync(unrepresentableStackItem).ConfigureAwait(false);
    Assert(!unrepresentableStackRegistration.Succeeded &&
      unrepresentableStackRegistration.RejectionCode == "InvalidWorldItemRegistration",
      "an item stack that cannot fit Steam's signed 16-bit field receives no network slot");

    ItemEntityRef nonFiniteItem = await worldOwner.InvokeAsync(
      session => CreateWorldItem(
        session,
        playerReference,
        positionX: float.NaN)).ConfigureAwait(false);
    NetworkWorldItemSlotRegistrationResult nonFiniteRegistration =
      await owner.RegisterWorldItemAsync(nonFiniteItem).ConfigureAwait(false);
    Assert(!nonFiniteRegistration.Succeeded &&
      nonFiniteRegistration.RejectionCode == "InvalidWorldItemRegistration",
      "an item with non-finite initial motion state receives no network slot");

    NetworkWorldItemSlotRegistrationResult registration = await owner.RegisterWorldItemAsync(
      item,
      reuseBlockedUntilTick: 10).ConfigureAwait(false);
    Assert(registration.Succeeded && registration.ItemIndex == 0 &&
      registration.Generation == 1,
      "the host registers an existing authoritative item into a generated world slot");

    NetworkWorldItemOwnershipProjection? ownershipProjection =
      await owner.CaptureOwnershipProjectionAsync(registration.ItemIndex).ConfigureAwait(false);
    Assert(ownershipProjection is NetworkWorldItemOwnershipProjection ownership &&
      ownership.ItemIndex == registration.ItemIndex &&
      ownership.ReservedForPlayer == context.Actor.PlayerSlot &&
      ownership.TimeToKeepReservation == 90 &&
      ownership.GrabDelayPlayer == byte.MaxValue && ownership.GrabDelayTime == 10 &&
      ownership.PositionX == 10.0f && ownership.PositionY == 15.0f,
      "packet 22 projection contains the active owner, remaining timers, and current item position");
    OutboundDispatch ownershipDispatch =
      SteamItemPacketCommandOwnerAdapter.ToSteamItemOwnerDispatch(
        ownershipProjection!.Value,
        connection);
    Assert(ownershipDispatch.Kind == PacketDispatchKind.Single &&
      ownershipDispatch.Targets.Count == 1 && ownershipDispatch.Targets[0] == connection &&
      ownershipDispatch.Packet is ItemOwnerPacket ownershipPacket &&
      ownershipPacket.ItemIndex == registration.ItemIndex &&
      ownershipPacket.ReservedForPlayer == context.Actor.PlayerSlot &&
      ownershipPacket.TimeToKeepReservation == 90 &&
      ownershipPacket.GrabDelayPlayer == byte.MaxValue &&
      ownershipPacket.GrabDelayTime == 10 &&
      ownershipPacket.PositionX == 10.0f && ownershipPacket.PositionY == 15.0f,
      "packet 22 adapter emits the complete ownership payload to its selected client");

    System.Threading.Interlocked.Exchange(ref currentTick, 100);
    NetworkWorldItemOwnershipProjection? expiredOwnershipProjection =
      await owner.CaptureOwnershipProjectionAsync(registration.ItemIndex).ConfigureAwait(false);
    Assert(expiredOwnershipProjection is NetworkWorldItemOwnershipProjection expiredOwnership &&
      expiredOwnership.ReservedForPlayer == byte.MaxValue &&
      expiredOwnership.TimeToKeepReservation == 0 &&
      expiredOwnership.GrabDelayPlayer == 0 && expiredOwnership.GrabDelayTime == 0,
      "packet 22 projection clears expired reservation and no-grab timers");
    System.Threading.Interlocked.Exchange(ref currentTick, 10);

    NetworkWorldItemSlotRegistrationResult repeatedRegistration =
      await owner.RegisterWorldItemAsync(item).ConfigureAwait(false);
    Assert(repeatedRegistration.Succeeded &&
      repeatedRegistration.ItemIndex == registration.ItemIndex &&
      repeatedRegistration.Generation == registration.Generation,
      "re-registering the same entity is idempotent");

    ItemEntityRef distantItem = await worldOwner.InvokeAsync(
      session => CreateWorldItem(
        session,
        playerReference,
        positionX: 5000.0f)).ConfigureAwait(false);
    NetworkWorldItemSlotRegistrationResult distantRegistration =
      await owner.RegisterWorldItemAsync(distantItem).ConfigureAwait(false);
    Assert(distantRegistration.Succeeded && distantRegistration.ItemIndex == 1,
      "a second active item receives its own Steam slot");
    NetworkWorldItemSlotRegistrationResult extendedDistantRegistration =
      await owner.RegisterWorldItemAsync(distantItem, reuseBlockedUntilTick: 20)
        .ConfigureAwait(false);
    Assert(extendedDistantRegistration.Succeeded &&
      extendedDistantRegistration.ItemIndex == distantRegistration.ItemIndex &&
      extendedDistantRegistration.Generation == distantRegistration.Generation,
      "re-registering an item extends its reuse deadline without changing its slot generation");
    PacketHandlingResult protectedDistantSync = await owner.ApplySyncItemAsync(
      context,
      new NetworkWorldItemSyncCommand(
        distantRegistration.ItemIndex,
        ItemType: 42,
        Stack: 5,
        Prefix: 3,
        StateFlags: 0,
        PositionX: 5001.0f,
        PositionY: 15.0f,
        VelocityX: 0.0f,
        VelocityY: 0.0f)).ConfigureAwait(false);
    Assert(!protectedDistantSync.Accepted &&
      protectedDistantSync.RejectionCode == "WorldItemSlotReuseProtected",
      "an extended authoritative reuse deadline blocks packet 21 updates");

    ItemEntityRef instancedItem = await worldOwner.InvokeAsync(
      session => CreateWorldItem(
        session,
        playerReference,
        positionX: 100.0f,
        isInstanced: true)).ConfigureAwait(false);
    NetworkWorldItemSlotRegistrationResult instancedRegistration =
      await owner.RegisterWorldItemAsync(instancedItem).ConfigureAwait(false);
    Assert(instancedRegistration.Succeeded && instancedRegistration.ItemIndex == 2,
      "an instanced item remains addressable in the host's slot projection");

    IReadOnlyList<NetworkWorldItemPositionProjection> firstSectionItems =
      await owner.CaptureSectionItemsAsync(0, 0).ConfigureAwait(false);
    Assert(firstSectionItems.Count == 1 &&
      firstSectionItems[0].ItemIndex == registration.ItemIndex &&
      firstSectionItems[0].PositionX == 10.0f &&
      firstSectionItems[0].PositionY == 15.0f,
      "packet 160 section capture includes the active item inside the legacy section bounds");

    IReadOnlyList<NetworkWorldItemPositionProjection> secondSectionItems =
      await owner.CaptureSectionItemsAsync(1, 0).ConfigureAwait(false);
    Assert(secondSectionItems.Count == 1 &&
      secondSectionItems[0].ItemIndex == distantRegistration.ItemIndex &&
      secondSectionItems[0].PositionX == 5000.0f,
      "packet 160 section capture excludes items outside that section's expanded bounds");
    OutboundDispatch positionDispatch =
      SteamItemPacketCommandOwnerAdapter.ToSteamItemPositionDispatch(
        secondSectionItems[0],
        connection);
    Assert(positionDispatch.Kind == PacketDispatchKind.Single &&
      positionDispatch.Targets.Count == 1 && positionDispatch.Targets[0] == connection &&
      positionDispatch.Packet is ItemPositionPacket positionPacket &&
      positionPacket.ItemIndex == distantRegistration.ItemIndex &&
      positionPacket.Position == new PacketVector2(5000.0f, 15.0f),
      "packet 160 projection is encoded for exactly the activated-section client");

    ItemEntityRef movementItem = await worldOwner.InvokeAsync(
      session => CreateWorldItem(
        session,
        playerReference,
        positionX: 8000.0f,
        positionY: 6000.0f)).ConfigureAwait(false);
    NetworkWorldItemSlotRegistrationResult movementRegistration =
      await owner.RegisterWorldItemAsync(movementItem).ConfigureAwait(false);
    Assert(movementRegistration.Succeeded && movementRegistration.ItemIndex == 3,
      "a world item outside the initial spawn sections receives a registered network slot");
    ItemEntityRef extraSpawnItem = await worldOwner.InvokeAsync(
      session => CreateWorldItem(
        session,
        playerReference,
        positionX: 1000.0f,
        positionY: 5000.0f)).ConfigureAwait(false);
    NetworkWorldItemSlotRegistrationResult extraSpawnRegistration =
      await owner.RegisterWorldItemAsync(extraSpawnItem).ConfigureAwait(false);
    Assert(extraSpawnRegistration.Succeeded && extraSpawnRegistration.ItemIndex == 4,
      "a world item outside initial and movement sections receives its own network slot");

    const int sectionWorldWidth = 600;
    const int sectionWorldHeight = 450;
    var sectionWorldPacket = new WorldDataPacket
    {
      MaxTilesX = sectionWorldWidth,
      MaxTilesY = sectionWorldHeight,
      SpawnTileX = 10,
      SpawnTileY = 10,
      WorldName = "item section verification",
      WorldGuid = Guid.NewGuid().ToByteArray()
    };
    var sectionHandlers = new WorldSynchronizationPacketHandlers(
      sectionWorldPacket,
      new TileMapSnapshot(
        sectionWorldWidth,
        sectionWorldHeight,
        new TileCellState[sectionWorldWidth * sectionWorldHeight]),
      new bool[ushort.MaxValue + 1],
      new PlayerControlsObservationStore(),
      new TileBreakObservationStore(),
      new WorldSynchronizationObservation(),
      current => activeBindings.Contains((current.Connection, current.Actor)),
      () => runtimeId,
      owner);
    NetworkSessionContext initialSectionContext = context with
    {
      Stage = NetworkSessionStage.AwaitSectionRequest
    };
    PacketHandlingResult initialSectionTransfer = await sectionHandlers.HandleAsync(
      initialSectionContext,
      new SpawnTileDataPacket { X = -1, Y = -1 },
      CancellationToken.None).ConfigureAwait(false);
    ItemPositionPacket[] initialItemPositions = initialSectionTransfer.Outbound
      .Select(dispatch => dispatch.Packet)
      .OfType<ItemPositionPacket>()
      .ToArray();
    Assert(initialSectionTransfer.Accepted && initialItemPositions.Length == 2 &&
      initialSectionTransfer.Outbound.Where(dispatch =>
        dispatch.Packet is ItemPositionPacket).All(dispatch =>
          dispatch.AllowedStages == NetworkSessionStage.Synchronizing &&
          dispatch.Kind == PacketDispatchKind.Single &&
          dispatch.Targets.Count == 1 && dispatch.Targets[0] == connection) &&
      initialItemPositions.Any(packet => packet.ItemIndex == registration.ItemIndex &&
        packet.Position == new PacketVector2(10.0f, 15.0f)) &&
      initialItemPositions.Any(packet => packet.ItemIndex == distantRegistration.ItemIndex &&
        packet.Position == new PacketVector2(5000.0f, 15.0f)),
      "initial section transfer emits registered packet-160 item positions to its joining client");

    PacketHandlingResult sectionTransfer = await sectionHandlers.HandleAsync(
      context,
      new RequestSectionPacket { SectionX = 1, SectionY = 0 },
      CancellationToken.None).ConfigureAwait(false);
    OutboundDispatch sectionPositionDispatch = sectionTransfer.Outbound.SingleOrDefault(
      dispatch => dispatch.Packet is ItemPositionPacket) ??
      throw new InvalidOperationException(
        "The activated section did not produce a packet-160 position dispatch.");
    Assert(sectionTransfer.Accepted &&
      sectionPositionDispatch.AllowedStages == NetworkSessionStage.Active &&
      sectionPositionDispatch.Kind == PacketDispatchKind.Single &&
      sectionPositionDispatch.Targets.Count == 1 &&
      sectionPositionDispatch.Targets[0] == connection &&
      sectionPositionDispatch.Packet is ItemPositionPacket sectionPositionPacket &&
      sectionPositionPacket.ItemIndex == distantRegistration.ItemIndex &&
      sectionPositionPacket.Position == new PacketVector2(5000.0f, 15.0f),
      "an active section request captures registered items and targets packet 160 only to its requester");

    using var extraSpawnCaptureCancellation = new CancellationTokenSource();
    cancelDuringItemCapture = extraSpawnCaptureCancellation;
    bool extraSpawnCaptureCanceled = false;
    try {
      await sectionHandlers.CreateExtraSpawnSectionDispatchesAsync(
        context,
        tileX: 10,
        tileY: 300,
        cancellationToken: extraSpawnCaptureCancellation.Token).ConfigureAwait(false);
    } catch (OperationCanceledException) when (
        extraSpawnCaptureCancellation.IsCancellationRequested) {
      extraSpawnCaptureCanceled = true;
    } finally {
      cancelDuringItemCapture = null;
    }
    Assert(extraSpawnCaptureCanceled,
      "packet-157 section capture propagates cancellation before committing transfer state");

    IReadOnlyList<OutboundDispatch> extraSpawnSectionTransfer =
      await sectionHandlers.CreateExtraSpawnSectionDispatchesAsync(
        context,
        tileX: 10,
        tileY: 300,
        cancellationToken: CancellationToken.None).ConfigureAwait(false);
    OutboundDispatch extraSpawnPositionDispatch = extraSpawnSectionTransfer
      .SingleOrDefault(dispatch => dispatch.Packet is ItemPositionPacket) ??
      throw new InvalidOperationException(
        "The team extra-spawn section did not produce a packet-160 position dispatch.");
    Assert(extraSpawnPositionDispatch.AllowedStages == NetworkSessionStage.Active &&
      extraSpawnPositionDispatch.Kind == PacketDispatchKind.Single &&
      extraSpawnPositionDispatch.Targets.Count == 1 &&
      extraSpawnPositionDispatch.Targets[0] == connection &&
      extraSpawnPositionDispatch.Packet is ItemPositionPacket extraSpawnPositionPacket &&
      extraSpawnPositionPacket.ItemIndex == extraSpawnRegistration.ItemIndex &&
      extraSpawnPositionPacket.Position == new PacketVector2(1000.0f, 5000.0f),
      "packet-157 extra team-spawn section activation targets its registered packet-160 item to the player");
    IReadOnlyList<OutboundDispatch> repeatedExtraSpawnSectionTransfer =
      await sectionHandlers.CreateExtraSpawnSectionDispatchesAsync(
        context,
        tileX: 10,
        tileY: 300,
        cancellationToken: CancellationToken.None).ConfigureAwait(false);
    Assert(repeatedExtraSpawnSectionTransfer.Count == 0,
      "repeated packet-157 activation does not resend the section or its packet-160 items");

    PacketHandlingResult movementTransfer = await sectionHandlers.HandleAsync(
      context,
      new PlayerControlsPacket { Position = new PacketVector2(8000.0f, 6000.0f) },
      CancellationToken.None).ConfigureAwait(false);
    OutboundDispatch movementPositionDispatch = movementTransfer.Outbound.SingleOrDefault(
      dispatch => dispatch.Packet is ItemPositionPacket itemPosition &&
        itemPosition.ItemIndex == movementRegistration.ItemIndex) ??
      throw new InvalidOperationException(
        "Movement into a new section did not produce the registered packet-160 item position.");
    Assert(movementTransfer.Accepted &&
      movementPositionDispatch.AllowedStages == NetworkSessionStage.Active &&
      movementPositionDispatch.Kind == PacketDispatchKind.Single &&
      movementPositionDispatch.Targets.Count == 1 &&
      movementPositionDispatch.Targets[0] == connection &&
      movementPositionDispatch.Packet is ItemPositionPacket movementPositionPacket &&
      movementPositionPacket.Position == new PacketVector2(8000.0f, 6000.0f),
      "movement section activation sends packet 160 for its registered item only to that active client");

    var adapter = new SteamItemPacketCommandOwnerAdapter(owner);
    var packet = new SyncItemPacket
    {
      ItemIndex = registration.ItemIndex,
      ItemType = 42,
      Stack = 5,
      Prefix = 3,
      PositionX = 20.0f,
      PositionY = 30.0f,
      VelocityX = 4.0f,
      VelocityY = -2.0f
    };
    PacketHandlingResult applied = await adapter.ApplySyncItemAsync(
      context,
      packet,
      CancellationToken.None).ConfigureAwait(false);
    Assert(applied.Accepted && applied.Outbound.Count == 1 &&
      applied.Outbound[0].Kind == PacketDispatchKind.AllActiveExceptSender &&
      applied.Outbound[0].Packet is SyncItemPacket,
      "packet 21 commits through the Items motion system and relays to other active sessions");
    SyncItemPacket relay = (SyncItemPacket)applied.Outbound[0].Packet;
    Assert(relay.ItemIndex == registration.ItemIndex && relay.ItemType == 42 &&
      relay.Stack == 5 && relay.Prefix == 3 && relay.PositionX == 20.0f &&
      relay.PositionY == 30.0f && relay.VelocityX == 4.0f && relay.VelocityY == -2.0f,
      "the relay contains the committed item state");

    PacketHandlingResult replay = await adapter.ApplySyncItemAsync(
      context,
      packet,
      CancellationToken.None).ConfigureAwait(false);
    Assert(replay.Accepted && replay.Outbound.Count == 0,
      "an exact replay is an accepted no-op and is not rebroadcast");

    PacketHandlingResult forgedType = await adapter.ApplySyncItemAsync(
      context,
      new SyncItemPacket
      {
        ItemIndex = registration.ItemIndex,
        ItemType = 43,
        Stack = 5,
        Prefix = 3,
        PositionX = 80.0f,
        PositionY = 90.0f,
        VelocityX = 0.0f,
        VelocityY = 0.0f
      },
      CancellationToken.None).ConfigureAwait(false);
    Assert(!forgedType.Accepted && forgedType.RejectionCode == "UnauthorizedWorldItemMutation" &&
      forgedType.Outbound.Count == 0,
      "a packet cannot change the registered item type");

    PacketHandlingResult unauthenticatedCreate = await adapter.ApplySyncItemAsync(
      context,
      new SyncItemPacket
      {
        ItemIndex = 400,
        ItemType = 42,
        Stack = 5,
        Prefix = 3,
        PositionX = 80.0f,
        PositionY = 90.0f
      },
      CancellationToken.None).ConfigureAwait(false);
    Assert(!unauthenticatedCreate.Accepted &&
      unauthenticatedCreate.RejectionCode == "UnauthenticatedWorldItemCreation" &&
      unauthenticatedCreate.Outbound.Count == 0,
      "packet 21 sentinel creation is rejected without server provenance");

    var extendedPacket = new SyncItemPacket
    {
      ItemIndex = registration.ItemIndex,
      ItemType = 42,
      Stack = 5,
      Prefix = 3,
      StateFlags = 0x0C,
      PositionX = 20.0f,
      PositionY = 30.0f,
      VelocityX = 4.0f,
      VelocityY = -2.0f,
      Shimmered = true,
      ShimmerTime = 0.5f,
      EnemyGrabDelayTime = 12
    };
    PacketHandlingResult extensionApplied = await adapter.ApplySyncItemAsync(
      context,
      extendedPacket,
      CancellationToken.None).ConfigureAwait(false);
    Assert(extensionApplied.Accepted && extensionApplied.Outbound.Count == 1 &&
      extensionApplied.Outbound[0].Packet is SyncItemPacket extensionRelay &&
      extensionRelay.StateFlags == 0x0C && extensionRelay.Shimmered == true &&
      extensionRelay.ShimmerTime == 0.5f && extensionRelay.EnemyGrabDelayTime == 12,
      "packet 21 commits and relays shimmer and enemy-grab-delay extensions");
    WorldItemState extendedState = await worldOwner.InvokeAsync(
      session => CaptureWorldItem(session, item)).ConfigureAwait(false);
    Assert(extendedState.IsShimmered && extendedState.ShimmerTime == 0.5f &&
      extendedState.EnemyGrabDelayTime == 12 && extendedState.Revision == 5,
      "the network extension state and motion share one authoritative revision");

    var ownerHintPacket = new SyncItemPacket
    {
      ItemIndex = registration.ItemIndex,
      ItemType = 42,
      Stack = 5,
      Prefix = 3,
      StateFlags = 0x0F,
      PositionX = 20.0f,
      PositionY = 30.0f,
      VelocityX = 4.0f,
      VelocityY = -2.0f,
      Shimmered = true,
      ShimmerTime = 0.5f,
      EnemyGrabDelayTime = 12
    };
    PacketHandlingResult ownerHint = await adapter.ApplySyncItemAsync(
      context,
      ownerHintPacket,
      CancellationToken.None).ConfigureAwait(false);
    Assert(ownerHint.Accepted && ownerHint.Outbound.Count == 0,
      "packet 21 ignores spawn-ownership hint bits for an already-registered slot");

    PacketHandlingResult mismatchedExtension = await adapter.ApplySyncItemAsync(
      context,
      new SyncItemPacket
      {
        ItemIndex = registration.ItemIndex,
        ItemType = 42,
        Stack = 5,
        Prefix = 3,
        StateFlags = 0x04,
        PositionX = 20.0f,
        PositionY = 30.0f,
        VelocityX = 4.0f,
        VelocityY = -2.0f
      },
      CancellationToken.None).ConfigureAwait(false);
    Assert(!mismatchedExtension.Accepted &&
      mismatchedExtension.RejectionCode == "InvalidWorldItemExtensions" &&
      mismatchedExtension.Outbound.Count == 0,
      "extension flags must match their payload fields");

    PacketHandlingResult unsupportedFlags = await adapter.ApplySyncItemAsync(
      context,
      new SyncItemPacket
      {
        ItemIndex = registration.ItemIndex,
        ItemType = 42,
        Stack = 5,
        Prefix = 3,
        StateFlags = 0x80,
        PositionX = 20.0f,
        PositionY = 30.0f,
        VelocityX = 4.0f,
        VelocityY = -2.0f
      },
      CancellationToken.None).ConfigureAwait(false);
    Assert(!unsupportedFlags.Accepted &&
      unsupportedFlags.RejectionCode == "UnsupportedWorldItemStateFlags" &&
      unsupportedFlags.Outbound.Count == 0,
      "packet 21 rejects unknown state flags");

    PacketHandlingResult resetExtensions = await adapter.ApplySyncItemAsync(
      context,
      new SyncItemPacket
      {
        ItemIndex = registration.ItemIndex,
        ItemType = 42,
        Stack = 5,
        Prefix = 3,
        PositionX = 20.0f,
        PositionY = 30.0f,
        VelocityX = 4.0f,
        VelocityY = -2.0f
      },
      CancellationToken.None).ConfigureAwait(false);
    Assert(resetExtensions.Accepted && resetExtensions.Outbound.Count == 1 &&
      resetExtensions.Outbound[0].Packet is SyncItemPacket resetRelay &&
      resetRelay.StateFlags == 0 && resetRelay.Shimmered is null &&
      resetRelay.ShimmerTime is null && resetRelay.EnemyGrabDelayTime is null,
      "omitted Steam extensions reset to defaults and are projected without extension flags");

    var staleContext = new NetworkSessionContext(
      new ConnectionIdentity(Guid.NewGuid(), 1),
      context.ProfileKey,
      context.Stage,
      new SenderBinding(5, Guid.NewGuid()),
      IsHost: false,
      runtimeId);
    PacketHandlingResult staleSender = await adapter.ApplySyncItemAsync(
      staleContext,
      packet,
      CancellationToken.None).ConfigureAwait(false);
    Assert(!staleSender.Accepted && staleSender.RejectionCode == "StaleItemSenderBinding" &&
      staleSender.Outbound.Count == 0,
      "a stale connection and binding cannot mutate or project item state");

    WorldItemState state = await worldOwner.InvokeAsync(
      session => CaptureWorldItem(session, item)).ConfigureAwait(false);
    Assert(state.PositionX == 20.0f && state.PositionY == 30.0f &&
      state.VelocityX == 4.0f && state.VelocityY == -2.0f && state.Revision == 6 &&
      !state.IsShimmered && state.ShimmerTime == 0.0f &&
      state.EnemyGrabDelayTime == 0,
      "rejected updates leave state unchanged and absent extensions reset to defaults");

    var otherConnection = new ConnectionIdentity(Guid.NewGuid(), 1);
    var otherBinding = new SenderBinding(6, Guid.NewGuid());
    activeBindings.Add((otherConnection, otherBinding));
    var otherContext = new NetworkSessionContext(
      otherConnection,
      context.ProfileKey,
      context.Stage,
      otherBinding,
      IsHost: false,
      runtimeId);
    NetworkPlayerBindingResult otherPlayerResult = await playerOwner.EnsurePlayerAsync(otherContext)
      .ConfigureAwait(false);
    Assert(otherPlayerResult.Succeeded,
      "a second authenticated player can reach item ownership checks");
    PacketHandlingResult otherPlayerDespawn = await adapter.ApplyDespawnItemAsync(
      otherContext,
      new SyncItemDespawnPacket { ItemIndex = registration.ItemIndex },
      CancellationToken.None).ConfigureAwait(false);
    bool itemStillResolvesAfterUnauthorizedDespawn = await worldOwner.InvokeAsync(
      session => session.EntityRuntime.TryResolve(item.Reference, out _)).ConfigureAwait(false);
    int occupiedSlotsAfterUnauthorizedDespawn = await worldOwner.InvokeAsync(
      static session => session.Storage.WorldItems.ActiveCount).ConfigureAwait(false);
    Assert(!otherPlayerDespawn.Accepted &&
      otherPlayerDespawn.RejectionCode == "WorldItemReservationOwnerMismatch" &&
      otherPlayerDespawn.Outbound.Count == 0 &&
      itemStillResolvesAfterUnauthorizedDespawn &&
      occupiedSlotsAfterUnauthorizedDespawn == 3,
      "packet 151 from another authenticated player leaves the item and slot unchanged");

    PacketHandlingResult outOfRangeDespawn = await adapter.ApplyDespawnItemAsync(
      context,
      new SyncItemDespawnPacket { ItemIndex = 400 },
      CancellationToken.None).ConfigureAwait(false);
    Assert(!outOfRangeDespawn.Accepted &&
      outOfRangeDespawn.RejectionCode == "InvalidWorldItemSlot" &&
      outOfRangeDespawn.Outbound.Count == 0,
      "packet 151 rejects the packet-21 creation sentinel as an item slot");

    PacketHandlingResult despawned = await adapter.ApplyDespawnItemAsync(
      context,
      new SyncItemDespawnPacket { ItemIndex = registration.ItemIndex },
      CancellationToken.None).ConfigureAwait(false);
    Assert(despawned.Accepted && despawned.Outbound.Count == 1 &&
      despawned.Outbound[0].Kind == PacketDispatchKind.AllActiveExceptSender &&
      despawned.Outbound[0].Packet is SyncItemDespawnPacket despawnRelay &&
      despawnRelay.ItemIndex == registration.ItemIndex,
      "packet 151 removes the owned item and relays only the committed slot removal");
    bool removedReferenceResolves = await worldOwner.InvokeAsync(
      session => session.EntityRuntime.TryResolve(item.Reference, out _)).ConfigureAwait(false);
    int occupiedSlotsAfterDespawn = await worldOwner.InvokeAsync(
      static session => session.Storage.WorldItems.ActiveCount).ConfigureAwait(false);
    Assert(!removedReferenceResolves && occupiedSlotsAfterDespawn == 2,
      "packet 151 removes the ECS entity and releases its generated slot");

    PacketHandlingResult repeatedDespawn = await adapter.ApplyDespawnItemAsync(
      context,
      new SyncItemDespawnPacket { ItemIndex = registration.ItemIndex },
      CancellationToken.None).ConfigureAwait(false);
    Assert(!repeatedDespawn.Accepted && repeatedDespawn.Outbound.Count == 0,
      "a repeated packet 151 cannot relay a second removal for an empty slot");

    ItemEntityRef nextItem = await worldOwner.InvokeAsync(
      session => CreateWorldItem(session, playerReference)).ConfigureAwait(false);
    NetworkWorldItemSlotRegistrationResult nextRegistration =
      await owner.RegisterWorldItemAsync(nextItem, reuseBlockedUntilTick: 20)
        .ConfigureAwait(false);
    Assert(nextRegistration.Succeeded && nextRegistration.ItemIndex == registration.ItemIndex &&
      nextRegistration.Generation == registration.Generation + 1,
      "a released network slot is reused only with a new generation");

    PacketHandlingResult protectedSync = await adapter.ApplySyncItemAsync(
      context,
      new SyncItemPacket
      {
        ItemIndex = nextRegistration.ItemIndex,
        ItemType = 42,
        Stack = 5,
        Prefix = 3,
        PositionX = 24.0f,
        PositionY = 31.0f,
        VelocityX = 0.0f,
        VelocityY = 0.0f
      },
      CancellationToken.None).ConfigureAwait(false);
    Assert(!protectedSync.Accepted &&
      protectedSync.RejectionCode == "WorldItemSlotReuseProtected" &&
      protectedSync.Outbound.Count == 0,
      "packet 21 cannot update a slot while its host reuse deadline is active");

    PacketHandlingResult protectedDespawn = await adapter.ApplyDespawnItemAsync(
      context,
      new SyncItemDespawnPacket { ItemIndex = nextRegistration.ItemIndex },
      CancellationToken.None).ConfigureAwait(false);
    Assert(!protectedDespawn.Accepted &&
      protectedDespawn.RejectionCode == "WorldItemSlotReuseProtected" &&
      protectedDespawn.Outbound.Count == 0,
      "packet 151 cannot clear a slot while the host reuse deadline is active");

    System.Threading.Interlocked.Exchange(ref currentTick, 20);
    PacketHandlingResult expiredProtectionDespawn = await adapter.ApplyDespawnItemAsync(
      context,
      new SyncItemDespawnPacket { ItemIndex = nextRegistration.ItemIndex },
      CancellationToken.None).ConfigureAwait(false);
    Assert(expiredProtectionDespawn.Accepted && expiredProtectionDespawn.Outbound.Count == 1,
      "packet 151 is admitted once the authoritative reuse deadline expires");

  }

  private static ItemEntityRef CreateWorldItem(
    LoadedWorldSession session,
    EntityReference ownerReference,
    int itemType = 42,
    int quantity = 5,
    float positionX = 10.0f,
    float positionY = 15.0f,
    bool isInstanced = false,
    long noGrabUntilTick = 0)
  {
    EntityRuntime runtime = session.EntityRuntime;
    RuntimeEntityHandle handle = runtime.CreateEntity();
    bool attached = runtime.TryAttach(
        handle,
        new ItemInstanceComponent(
          new ItemDefinitionRef(new ExternalContentId("verification", itemType), 1),
          new PersistentItemId(Guid.NewGuid()),
          prefixId: 3)) &&
      runtime.TryAttach(handle, new ItemStackComponent(quantity, Math.Max(quantity, 20))) &&
      runtime.TryAttach(handle, new LocationComponent(positionX, positionY)) &&
      runtime.TryAttach(handle, new VelocityComponent(1.0f, 2.0f)) &&
      runtime.TryAttach(
        handle,
        new WorldItemComponent(0, 1000, isInstanced: isInstanced, revision: 3)) &&
      runtime.TryAttach(
        handle,
        new WorldItemStateComponent(
          new ReplicationId(System.Threading.Interlocked.Increment(ref _nextReplicationId)))) &&
      runtime.TryAttach(
        handle,
        new WorldItemReservationComponent(
          new ReservationId(Guid.NewGuid()),
          RuntimeEntityId.FromEntityReference(ownerReference),
          reservationExpiresAt: 100,
          noGrabUntilTick: noGrabUntilTick,
          reservationRevision: 4));
    if (!attached || !runtime.TryPublishEntity(handle) ||
      !runtime.TryGetReference(handle, EntityReferenceScope.Item, out EntityReference reference))
    {
      throw new InvalidOperationException("The verification world item could not be created.");
    }

    return ItemEntityRef.FromReference(reference);
  }

  private static WorldItemState CaptureWorldItem(
    LoadedWorldSession session,
    ItemEntityRef item)
  {
    if (!session.EntityRuntime.TryResolve(item.Reference, out RuntimeEntityHandle handle) ||
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
        static (WorldItemComponent component) => component.Revision,
        out long revision) ||
      !session.EntityRuntime.TryCapture(
        handle,
        static (WorldItemComponent component) => component.IsShimmered,
        out bool isShimmered) ||
      !session.EntityRuntime.TryCapture(
        handle,
        static (WorldItemComponent component) => component.ShimmerTime,
        out float shimmerTime) ||
      !session.EntityRuntime.TryCapture(
        handle,
        static (WorldItemComponent component) => component.EnemyGrabDelayTime,
        out byte enemyGrabDelayTime))
    {
      throw new InvalidOperationException("The verification world item could not be captured.");
    }

    return new WorldItemState(
      position.X,
      position.Y,
      velocity.X,
      velocity.Y,
      revision,
      isShimmered,
      shimmerTime,
      enemyGrabDelayTime);
  }

  private static void Assert(bool condition, string description)
  {
    if (!condition)
    {
      throw new InvalidOperationException(description);
    }
  }

  private readonly record struct WorldItemState(
    float PositionX,
    float PositionY,
    float VelocityX,
    float VelocityY,
    long Revision,
    bool IsShimmered,
    float ShimmerTime,
    byte EnemyGrabDelayTime);
}
