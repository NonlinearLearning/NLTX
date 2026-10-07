using System.Numerics;
using EntityEcs;
using EntityEcs.Components;
using NSSLC.Infrastructure.Network;
using Terraria.Content;
using Terraria.Items;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NonAuthoritative.Persistence;
using Terraria.Player;
using Terraria.Player.Environment;
using Terraria.Relationships;

namespace NSSLC.NetworkVerification;

/// <summary>
/// Root acceptance for the first production player lifecycle slice plus the formal
/// packet-36 and packet-41 Player integrations. This calls the production handlers
/// directly and checks formal owner state plus projected outbound packets.
/// </summary>
internal static class PlayerLifecycleHandlerAcceptanceVerification
{
  public static async Task RunAsync()
  {
    var current = new Dictionary<ConnectionIdentity, SenderBinding>();
    var faelingSpawns = new RecordingFaelingSpawnPort();
    await using var worldOwner = new NetworkWorldOwner(() => new LoadedWorldSession());
    await worldOwner.Ready.WaitAsync(TimeSpan.FromSeconds(5));
    EntityRuntimeId worldRuntimeId = await worldOwner.InvokeAsync(
      session => session.EntityRuntime.RuntimeId);
    var playerOwner = new NetworkPlayerOwner(
      worldOwner,
      context => current.TryGetValue(context.Connection, out SenderBinding? binding) &&
        binding == context.Actor,
      new ItemDefinitionCatalog([CreateChannelledItemDefinition()]),
      faelingSpawnPort: faelingSpawns);
    var handlers = new PlayerLifecyclePacketHandlers(playerOwner, 4200, 1200);
    var stateHandlers = new PlayerStatePacketHandlers(playerOwner);

    ConnectionIdentity connection = new(Guid.NewGuid(), 1);
    var binding = new SenderBinding(7, Guid.NewGuid());
    current[connection] = binding;
    NetworkSessionContext upload = Context(
      connection, binding, worldRuntimeId, NetworkSessionStage.AwaitPlayerData);

    PacketHandlingResult identity = await handlers.HandleAsync(upload,
      new SyncPlayerPacket {
        Player = 200,
        Name = "  Alice  ",
        VoiceVariant = 0,
        Hair = 250
      }, CancellationToken.None);
    Verify.That(identity.Accepted && identity.Outbound.Count == 1 &&
      identity.Outbound[0].Packet is SyncPlayerPacket projectedIdentity &&
      projectedIdentity.Player == 7 && projectedIdentity.Name == "Alice" &&
      projectedIdentity.VoiceVariant == 1 && projectedIdentity.Hair == 0,
      "Packet 4 must apply identity to the authenticated player and project the bound slot.");

    ConnectionIdentity duplicateConnection = new(Guid.NewGuid(), 1);
    var duplicateBinding = new SenderBinding(8, Guid.NewGuid());
    current[duplicateConnection] = duplicateBinding;
    NetworkSessionContext duplicateContext = Context(
      duplicateConnection, duplicateBinding, worldRuntimeId, NetworkSessionStage.AwaitPlayerData);
    PacketHandlingResult duplicateIdentity = await handlers.HandleAsync(duplicateContext,
      new SyncPlayerPacket { Player = 7, Name = "Alice" }, CancellationToken.None);
    NetworkPlayerBindingResult afterRejectedIdentity = await playerOwner.EnsurePlayerAsync(
      duplicateContext);
    Verify.That(!duplicateIdentity.Accepted &&
      duplicateIdentity.RejectionCode == "InvalidPlayerState" &&
      afterRejectedIdentity.Status == NetworkPlayerBindingStatus.Applied,
      "A duplicate character name must reject packet 4 and roll back its newly created player entity.");

    NetworkSessionContext synchronizing = Context(
      connection, binding, worldRuntimeId, NetworkSessionStage.Synchronizing);
    PacketHandlingResult spawn = await handlers.HandleAsync(synchronizing,
      new PlayerSpawnPacket {
        Player = 7,
        SpawnX = 120,
        SpawnY = 240,
        RespawnTimer = 0,
        PveDeaths = 1,
        PvpDeaths = 2,
        Team = 3,
        SpawnContext = 0
      }, CancellationToken.None);
    Verify.That(spawn.Accepted && spawn.NextStage == NetworkSessionStage.Active &&
      spawn.Outbound.Count == 3 &&
      spawn.Outbound.Any(static dispatch =>
        dispatch.Packet is FinishedConnectingToServerPacket &&
        dispatch.Kind == PacketDispatchKind.Single) &&
      spawn.Outbound.Any(static dispatch =>
        dispatch.Packet is PlayerSpawnPacket projected &&
        projected.Player == 7 &&
        dispatch.Kind == PacketDispatchKind.AllActiveExceptSender) &&
      spawn.Outbound.Any(static dispatch =>
        dispatch.Packet is PlayerActivePacket activeProjection &&
        activeProjection.Player == 7 && activeProjection.ActiveState == 1 &&
        dispatch.Kind == PacketDispatchKind.AllActiveExceptSender),
      "Packet 12 must commit spawn state, advance to Active, and project the authenticated player's active state.");

    NetworkSessionContext active = Context(
      connection, binding, worldRuntimeId, NetworkSessionStage.Active);
    NetworkPlayerSnapshot? afterIdentity = await playerOwner.CapturePlayerAsync(active);
    Verify.That(afterIdentity is { } identitySnapshot &&
      identitySnapshot.CharacterName == "Alice",
      "Packet 4 must commit the formal character identity.");

    PacketHandlingResult controls = await handlers.HandleAsync(active,
      new PlayerControlsPacket {
        Player = 200,
        ControlFlags = 3,
        MovementFlags = 5,
        SelectedItem = 4,
        Position = new PacketVector2(128, 256),
        Velocity = new PacketVector2(1, -2)
      }, CancellationToken.None);
    Verify.That(controls.Accepted && controls.Outbound.Count == 1 &&
      controls.Outbound[0].Packet is PlayerControlsPacket projectedControls &&
      projectedControls.Player == 7 && projectedControls.Position.X == 128 &&
      projectedControls.Position.Y == 256,
      "Packet 13 must commit movement and project the authenticated player slot.");

    NetworkPlayerSnapshot? afterControls = await playerOwner.CapturePlayerAsync(active);
    Verify.That(afterControls is { } movementSnapshot &&
      movementSnapshot.Position == new Vector2(128, 256) &&
      movementSnapshot.SelectedInventorySlot == 4,
      "Packet 13 must update the formal position and selected inventory slot.");

    EntityReference playerReference = (await playerOwner.ResolveAuthenticatedPlayerAsync(active)).Reference;
    await worldOwner.InvokeAsync<object?>(session => {
      if (!session.EntityRuntime.TryResolve(playerReference, out RuntimeEntityHandle playerHandle)) {
        throw new InvalidOperationException("The authenticated Player entity could not be resolved.");
      }

      RuntimeEntityHandle itemHandle = session.EntityRuntime.CreateEntity();
      if (!session.EntityRuntime.TryAttach(itemHandle, new ItemDefinitionComponent(9001)) ||
          !session.EntityRuntime.TryPublishEntity(itemHandle) ||
          !session.EntityRuntime.TryGetReference(
            itemHandle, EntityReferenceScope.Item, out EntityReference itemReference) ||
          !session.EntityRuntime.TryEdit<PlayerInventorySlotsComponent>(
            playerHandle,
            (ref PlayerInventorySlotsComponent inventory) =>
              inventory.MainInventorySlots[4] = ItemEntityRef.FromReference(itemReference))) {
        throw new InvalidOperationException("The channelled selected item fixture could not be committed.");
      }

      return null;
    });

    PacketHandlingResult animation = await stateHandlers.HandleAsync(active,
      new ItemRotationAndAnimationPacket {
        Player = 200,
        ItemRotation = 0.5f,
        ItemAnimation = 12
      }, CancellationToken.None);
    byte animationChannel = await worldOwner.InvokeAsync(session => {
      if (!session.EntityRuntime.TryResolve(playerReference, out RuntimeEntityHandle playerHandle) ||
          !session.EntityRuntime.TryCapture(
            playerHandle,
            static (PlayerNetworkStateComponent network) => network.ItemAnimationChannel,
            out byte channel)) {
        throw new InvalidOperationException("The Player animation state could not be captured.");
      }

      return channel;
    });
    Verify.That(animation.Accepted && animation.Outbound.Count == 1 &&
      animation.Outbound[0].Packet is ItemRotationAndAnimationPacket projectedAnimation &&
      projectedAnimation.Player == 7 && animationChannel == 1,
      "Packet 41 must resolve the selected formal item definition and commit its channel state.");

    PacketHandlingResult zoneFalse = await stateHandlers.HandleAsync(active,
      new SyncPlayerZonePacket { Player = 200, Zone5 = 0, TownNpcCount = 2 },
      CancellationToken.None);
    PacketHandlingResult zoneTrue = await stateHandlers.HandleAsync(active,
      new SyncPlayerZonePacket { Player = 200, Zone5 = 1, TownNpcCount = 2 },
      CancellationToken.None);
    PacketHandlingResult zoneTrueAgain = await stateHandlers.HandleAsync(active,
      new SyncPlayerZonePacket { Player = 200, Zone5 = 1, TownNpcCount = 2 },
      CancellationToken.None);
    Verify.That(zoneFalse.Accepted && zoneTrue.Accepted && zoneTrueAgain.Accepted &&
      faelingSpawns.Count == 1,
      "Packet 36 must invoke the formal shimmer transition only on a false-to-true edge.");

    NetworkPlayerSnapshot? beforeActiveUpload = await playerOwner.CapturePlayerAsync(active);
    PacketHandlingResult activeUpload = await handlers.HandleAsync(active,
      new PlayerActivePacket { Player = 8, ActiveState = 0 }, CancellationToken.None);
    NetworkPlayerSnapshot? afterActiveUpload = await playerOwner.CapturePlayerAsync(active);
    Verify.That(activeUpload.Accepted && activeUpload.Outbound.Count == 0 &&
      beforeActiveUpload is { } beforeActive && afterActiveUpload is { } afterActive &&
      beforeActive.Active == afterActive.Active &&
      beforeActive.ConnectionState == afterActive.ConnectionState,
      "Client packet 14 must not change the sender or a claimed target's active state.");

    PacketHandlingResult life = await handlers.HandleAsync(active,
      new PlayerLifeManaPacket { Player = 200, Life = 80, MaximumLife = 100 },
      CancellationToken.None);
    Verify.That(life.Accepted && life.Outbound.Count == 1 &&
      life.Outbound[0].Packet is PlayerLifeManaPacket projectedLife &&
      projectedLife.Player == 7 && projectedLife.Life == 80 &&
      projectedLife.MaximumLife == 100,
      "Packet 16 must commit life and relay the authenticated slot.");

    PacketHandlingResult death = await handlers.HandleAsync(active,
      new PlayerLifeManaPacket { Player = 200, Life = 0, MaximumLife = 100 },
      CancellationToken.None);
    Verify.That(death.Accepted && death.Outbound.Count == 2 &&
      death.Outbound[0].Packet is PlayerLifeManaPacket projectedDeathLife &&
      projectedDeathLife.Player == 7 && projectedDeathLife.Life == 0 &&
      death.Outbound[1].Packet is DeadPlayerPacket projectedDeathEffect &&
      projectedDeathEffect.Player == 7,
      "Packet 16 death commits must project packet 135 to other active peers.");

    PacketHandlingResult mana = await handlers.HandleAsync(active,
      new Unknown42Packet { Player = 200, Mana = 40, MaximumMana = 60 },
      CancellationToken.None);
    Verify.That(mana.Accepted && mana.Outbound.Count == 0,
      "Packet 42 must commit mana without inventing an unsolicited relay.");

    PacketHandlingResult equipmentMana = await handlers.HandleAsync(upload,
      new Unknown42Packet { Player = 200, Mana = 220, MaximumMana = 200 },
      CancellationToken.None);
    Verify.That(equipmentMana.Accepted && equipmentMana.Outbound.Count == 0,
      "Login packet 42 carries base maximum mana and must allow equipment-boosted current mana.");
    NetworkPlayerSnapshot? afterEquipmentMana = await playerOwner.CapturePlayerAsync(active);
    Verify.That(afterEquipmentMana is { StatMana: 220, StatManaMax: 200 },
      "Packet 42 must preserve current mana separately from the transmitted base maximum.");
    foreach ((short currentMana, short baseMaximum) in new[] {
      ((short)-1, (short)200), ((short)220, (short)-1)
    }) {
      PacketHandlingResult invalidMana = await handlers.HandleAsync(upload,
        new Unknown42Packet { Player = 200, Mana = currentMana, MaximumMana = baseMaximum },
        CancellationToken.None);
      NetworkPlayerSnapshot? afterInvalidMana = await playerOwner.CapturePlayerAsync(active);
      Verify.That(!invalidMana.Accepted && invalidMana.RejectionCode == "InvalidPlayerState" &&
        afterInvalidMana is { StatMana: 220, StatManaMax: 200 },
        "Negative packet-42 mana values must reject without changing the accepted player state.");
    }

    PacketHandlingResult buffs = await handlers.HandleAsync(active,
      new PlayerBuffsPacket { Player = 200, BuffTypes = new ushort[] { 5, 10 } },
      CancellationToken.None);
    Verify.That(buffs.Accepted && buffs.Outbound.Count == 1 &&
      buffs.Outbound[0].Packet is PlayerBuffsPacket projectedBuffs &&
      projectedBuffs.Player == 7 && projectedBuffs.BuffTypes.SequenceEqual(new ushort[] { 5, 10 }),
      "Packet 50 must commit buffs and relay the authenticated slot.");

    current.Remove(connection);
    PacketHandlingResult staleControls = await handlers.HandleAsync(active,
      new PlayerControlsPacket {
        Player = 7,
        Position = new PacketVector2(160, 256)
      }, CancellationToken.None);
    Verify.That(!staleControls.Accepted && staleControls.RejectionCode == "RejectedSenderBinding",
      "A stale sender binding must not mutate the player or produce an outbound projection.");
  }

  private static NetworkSessionContext Context(
    ConnectionIdentity connection,
    SenderBinding binding,
    EntityRuntimeId worldRuntimeId,
    NetworkSessionStage stage)
  {
    return new NetworkSessionContext(
      connection,
      "acceptance",
      stage,
      binding,
      IsHost: false,
      WorldRuntimeId: worldRuntimeId);
  }

  private static ItemDefinition CreateChannelledItemDefinition()
  {
    return new ItemDefinition(
      new ItemIdentityDefinition(9001, "verification.channelled-item"),
      new ItemUseDefinition(0, 5, true, 30, 30, true, false, false, false, false, null, 0),
      new ItemStackDefinition(1, true, false, 1),
      new ItemCombatDefinition(0, 0, 0, 0, 0, null, 0, null, null, false, false, false, false, false),
      new ItemPlacementDefinition(null, null, 0, false),
      new ItemEffectDefinition(0, 0, 0, 0, 0, null, 0, false, false, false),
      new ItemEquipmentDefinition(false, null, null, null),
      new ItemEconomyDefinition(0, null),
      new ItemCapabilitiesDefinition(false, false, false));
  }

  private sealed class RecordingFaelingSpawnPort : IPlayerFaelingSpawnPort
  {
    public int Count { get; private set; }

    public void SpawnFaelings()
    {
      Count++;
    }
  }
}
