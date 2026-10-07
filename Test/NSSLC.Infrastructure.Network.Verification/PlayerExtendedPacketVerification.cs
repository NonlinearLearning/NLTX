using System.Numerics;
using EntityEcs;
using NSSLC.Infrastructure.Network;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NonAuthoritative.Persistence;
using Terraria.Player;
using Terraria.Relationships;

namespace NSSLC.NetworkVerification;

internal static class PlayerExtendedPacketVerification
{
  public static async Task RunAsync()
  {
    var current = new Dictionary<ConnectionIdentity, SenderBinding>();
    await using var world = new NetworkWorldOwner(() => new LoadedWorldSession());
    await world.Ready.WaitAsync(TimeSpan.FromSeconds(5));
    EntityRuntimeId runtimeId = await world.InvokeAsync(
      session => session.EntityRuntime.RuntimeId);
    var owner = new NetworkPlayerOwner(
      world,
      context => current.TryGetValue(context.Connection, out SenderBinding? binding) &&
        binding == context.Actor);
    ConnectionIdentity connection = new(Guid.NewGuid(), 1);
    SenderBinding binding = new(2, Guid.NewGuid());
    current[connection] = binding;
    var context = new NetworkSessionContext(
      connection, "verification", NetworkSessionStage.Active, binding,
      IsHost: false, runtimeId);
    NetworkPlayerBindingResult firstPlayer = await owner.EnsurePlayerAsync(context);
    Verify.That(firstPlayer.Succeeded,
      "The extended packet fixture requires an authenticated player.");
    EntityReference firstReference = firstPlayer.Player!.Value.Reference;

    ConnectionIdentity targetConnection = new(Guid.NewGuid(), 2);
    SenderBinding targetBinding = new(7, Guid.NewGuid());
    current[targetConnection] = targetBinding;
    var targetContext = new NetworkSessionContext(
      targetConnection, "verification", NetworkSessionStage.Active, targetBinding,
      IsHost: false, runtimeId);
    Verify.That((await owner.EnsurePlayerAsync(targetContext)).Succeeded,
      "The spectating fixture requires an active target player.");

    var handlers = new PlayerExtendedPacketHandlers(owner);
    PacketHandlingResult loadout = await handlers.HandleAsync(
      context,
      new SyncLoadoutPacket { Player = 99, LoadoutIndex = 1, AccessoryVisibilityMask = 3 },
      CancellationToken.None);
    Verify.That(loadout.Accepted && loadout.Outbound.Count == 1 &&
      loadout.Outbound[0].Packet is SyncLoadoutPacket projectedLoadout &&
      projectedLoadout.Player == 2 && projectedLoadout.LoadoutIndex == 1 &&
      projectedLoadout.AccessoryVisibilityMask == 3,
      "Packet 147 must apply the authenticated loadout and project its visibility mask.");
    (int currentLoadoutIndex, ushort visibilityMask) = await world.InvokeAsync(session =>
      session.EntityRuntime.TryResolve(firstReference, out RuntimeEntityHandle firstHandle) &&
      session.EntityRuntime.TryCapture(
        firstHandle,
        static (PlayerLoadoutStateComponent loadouts) => loadouts.CurrentLoadoutIndex,
        out int loadoutIndex) &&
      session.EntityRuntime.TryCapture(
        firstHandle,
        static (PlayerAppearanceSelectionComponent appearance) =>
          (ushort)((appearance.HiddenVisibleAccessories[0] ? 1 : 0) |
            (appearance.HiddenVisibleAccessories[1] ? 2 : 0)),
        out ushort mask)
        ? (loadoutIndex, mask)
        : (-1, (ushort)0));
    Verify.That(currentLoadoutIndex == 1 && visibilityMask == 3,
      "Packet 147 must commit loadout and accessory visibility on the shared Player entity.");

    PacketHandlingResult spectate = await handlers.HandleAsync(
      context,
      new SpectatePlayerPacket { Player = 99, TargetPlayer = 7 },
      CancellationToken.None);
    Verify.That(spectate.Accepted && spectate.Outbound.Count == 1 &&
      ((SpectatePlayerPacket)spectate.Outbound[0].Packet).Player == 2 &&
      ((SpectatePlayerPacket)spectate.Outbound[0].Packet).TargetPlayer == 7,
      "Packet 150 must update the authenticated observer lifecycle and relay a valid target.");
    PacketHandlingResult invalidSpectate = await handlers.HandleAsync(
      context,
      new SpectatePlayerPacket { Player = 99, TargetPlayer = 9 },
      CancellationToken.None);
    Verify.That(invalidSpectate.Accepted && invalidSpectate.Outbound.Count == 0,
      "Packet 150 must resolve an unresolved target through the formal fallback selector without broadcasting a no-op.");
    int currentSpectatingTarget = await world.InvokeAsync(session =>
      session.EntityRuntime.TryResolve(firstReference, out RuntimeEntityHandle firstHandle) &&
      session.EntityRuntime.TryCapture(firstHandle,
        static (PlayerLifecycleComponent lifecycle) => lifecycle.SpectatingTargetSlot?.Value ?? -1,
        out int targetSlot)
        ? targetSlot
        : -2);
    Verify.That(currentSpectatingTarget == 7,
      "A rejected packet 150 fallback must not partially mutate spectator state.");

    PacketHandlingResult team = await handlers.HandleAsync(
      context,
      new TeamChangeFromUIPacket { Player = 99, Team = 4 },
      CancellationToken.None);
    Verify.That(team.Accepted && ((TeamChangePacket)team.Outbound[0].Packet).Player == 2,
      "Packet 157 must use the authenticated sender slot and emit packet 45.");

    bool pendingTeleportSet = await world.InvokeAsync(session =>
      session.EntityRuntime.TryResolve(firstReference, out RuntimeEntityHandle firstHandle) &&
      session.EntityRuntime.TryEdit<PlayerNetworkStateComponent>(
        firstHandle,
        (ref PlayerNetworkStateComponent network) =>
          PlayerNetworkStateSystem.BeginUnacknowledgedTeleport(
            network, new Vector2(320, 640))));
    Verify.That(pendingTeleportSet, "Packet 96 fixture must arrange a pending portal destination.");
    PacketHandlingResult portal = await handlers.HandleAsync(
      context,
      new TeleportPlayerThroughPortalPacket {
        Player = 99,
        PortalColorIndex = 2,
        PositionX = 320,
        PositionY = 640,
        VelocityX = 1,
        VelocityY = -1
      },
      CancellationToken.None);
    Verify.That(portal.Accepted && portal.Outbound.Count == 1 &&
      ((TeleportPlayerThroughPortalPacket)portal.Outbound[0].Packet).Player == 2,
      "Packet 96 must commit the matching pending portal position and relay the paired color.");
    NetworkPlayerSnapshot? afterPortal = await owner.CapturePlayerAsync(context);
    Verify.That(afterPortal is NetworkPlayerSnapshot moved &&
      moved.Position == new Vector2(320, 640) && !moved.HasPendingTeleport,
      "Packet 96 must commit movement and acknowledge only the matching pending teleport.");

    PacketHandlingResult manaEffect = await handlers.HandleAsync(
      context,
      new ManaEffectPacket { Player = 99, ManaEffect = 14 },
      CancellationToken.None);
    Verify.That(manaEffect.Accepted && manaEffect.Outbound.Count == 1 &&
      ((ManaEffectPacket)manaEffect.Outbound[0].Packet).Player == 2 &&
      manaEffect.Outbound[0].Kind == PacketDispatchKind.AllActiveExceptSender,
      "Packet 43 must relay its visual effect from the authenticated slot without changing mana.");

    PacketHandlingResult miscSound = await handlers.HandleAsync(
      context,
      new MiscDataSyncPacket { Player = 99, Action = 2 },
      CancellationToken.None);
    Verify.That(miscSound.Accepted && miscSound.Outbound.Count == 1 &&
      ((MiscDataSyncPacket)miscSound.Outbound[0].Packet).Player == 2,
      "Packet 51 action 2 must relay the authenticated sender's sound notice.");
    PacketHandlingResult miscBoss = await handlers.HandleAsync(
      context,
      new MiscDataSyncPacket { Player = 99, Action = 1 },
      CancellationToken.None);
    Verify.That(!miscBoss.Accepted &&
      miscBoss.RejectionCode == "SkeletronSpawnOwnerUnavailable",
      "Packet 51 action 1 must not silently claim a boss spawn without the NPC owner.");

    PacketHandlingResult pickTile = await handlers.HandleAsync(
      context,
      new SyncTilePickingPacket { Player = 99, X = 3, Y = 4, TileType = 5 },
      CancellationToken.None);
    Verify.That(pickTile.Accepted && pickTile.Outbound.Count == 1 &&
      ((SyncTilePickingPacket)pickTile.Outbound[0].Packet).Player == 2 &&
      pickTile.Outbound[0].Kind == PacketDispatchKind.AllActiveExceptSender,
      "Packet 125 must relay visual pick damage without changing tile state.");

    PacketHandlingResult nebula = await handlers.HandleAsync(
      context,
      new NebulaLevelupRequestPacket {
        Player = 99, ItemType = 55, Position = new(10, 20)
      },
      CancellationToken.None);
    Verify.That(nebula.Accepted && nebula.Outbound[0].Kind == PacketDispatchKind.AllActive,
      "Packet 102 must preserve the server branch's sender-inclusive relay.");

    PacketHandlingResult rejectedTeleport = await handlers.HandleAsync(
      context,
      new RequestTeleportationByServerPacket { TeleportationKind = 0 },
      CancellationToken.None);
    Verify.That(!rejectedTeleport.Accepted &&
      rejectedTeleport.RejectionCode == "TeleportDestinationUnavailable",
      "Packet 73 must wait for a validated World teleport destination.");
  }
}
