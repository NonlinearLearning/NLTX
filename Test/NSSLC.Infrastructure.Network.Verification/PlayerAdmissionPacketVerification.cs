using NSSLC.Infrastructure.Network;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace NSSLC.NetworkVerification;

internal static class PlayerAdmissionPacketVerification {
  public static async Task RunAsync() {
    var handler = new PlayerAdmissionPacketHandlers();
    var context = new NetworkSessionContext(
        new ConnectionIdentity(Guid.NewGuid(), 1),
        "test-profile",
        NetworkSessionStage.AwaitPlayerData,
        new SenderBinding(7, Guid.NewGuid()),
        false);

    PacketHandlingResult playerResult = await handler.HandleAsync(context, new SyncPlayerPacket {
      Player = 201,
      Name = "Headless",
      VoicePitchOffset = 0.25f
    }, CancellationToken.None);
    Verify.That(playerResult.Accepted && playerResult.Outbound.Count == 1
        && playerResult.Outbound[0].Packet is SyncPlayerPacket projected
        && projected.Player == 7 && projected.Name == "Headless",
        "Player admission must replace the declared player slot with the bound actor.");

    PacketHandlingResult equipmentResult = await handler.HandleAsync(context,
        new SyncEquipmentPacket { Player = 201, Slot = 600, Stack = 1, ItemType = 1 },
        CancellationToken.None);
    Verify.That(equipmentResult.Accepted,
        "A bounded equipment upload must be accepted during player admission.");

    PacketHandlingResult invalidEquipment = await handler.HandleAsync(context,
        new SyncEquipmentPacket { Slot = -1, Stack = 1, ItemType = 1 },
        CancellationToken.None);
    Verify.That(!invalidEquipment.Accepted && invalidEquipment.RejectionCode == "InvalidEquipmentSlot",
        "Negative inventory slots must be rejected before an owner sees them.");

    PacketHandlingResult highestVanillaEquipment = await handler.HandleAsync(context,
        new SyncEquipmentPacket { Slot = 989, Stack = 1, ItemType = 1 }, CancellationToken.None);
    Verify.That(highestVanillaEquipment.Accepted,
        "The highest slot emitted by the current Terraria item-slot table must be accepted.");

    PacketHandlingResult outOfRangeEquipment = await handler.HandleAsync(context,
        new SyncEquipmentPacket { Slot = 990, Stack = 1, ItemType = 1 }, CancellationToken.None);
    Verify.That(!outOfRangeEquipment.Accepted
        && outOfRangeEquipment.RejectionCode == "InvalidEquipmentSlot",
        "Slots beyond the current Terraria item-slot table must be rejected.");

    PacketHandlingResult invalidMarker = await handler.HandleAsync(context,
        new ClientSyncedInventoryPacket { Bytes = new byte[] { 1 } }, CancellationToken.None);
    Verify.That(!invalidMarker.Accepted
        && invalidMarker.RejectionCode == "InventorySyncMarkerMustBeEmpty",
        "Packet 138 must remain an empty completion marker.");

    PacketHandlingResult loadoutResult = await handler.HandleAsync(context,
        new SyncLoadoutPacket { Player = 201, LoadoutIndex = 2, AccessoryVisibilityMask = 3 },
        CancellationToken.None);
    Verify.That(loadoutResult.Accepted && loadoutResult.Outbound[0].Packet is SyncLoadoutPacket loadout
        && loadout.Player == 7 && loadout.LoadoutIndex == 2,
        "Loadout admission must project the bound player slot.");

    PacketHandlingResult invalidLoadout = await handler.HandleAsync(context,
        new SyncLoadoutPacket { LoadoutIndex = 3 }, CancellationToken.None);
    Verify.That(!invalidLoadout.Accepted && invalidLoadout.RejectionCode == "InvalidLoadoutIndex",
        "Loadout indices outside the three vanilla loadouts must be rejected.");

    for (byte team = 0; team <= 5; team++) {
      PacketHandlingResult teamResult = await handler.HandleAsync(context,
          new TeamChangePacket { Player = 201, Team = team }, CancellationToken.None);
      Verify.That(teamResult.Accepted && teamResult.Outbound.Count == 1
          && teamResult.Outbound[0].Packet is TeamChangePacket projectedTeam
          && projectedTeam.Player == 7 && projectedTeam.Team == team,
          "All six vanilla teams must project the bound sender instead of a claimed slot.");
    }
    PacketHandlingResult invalidTeam = await handler.HandleAsync(context,
        new TeamChangePacket { Player = 201, Team = 6 }, CancellationToken.None);
    Verify.That(!invalidTeam.Accepted && invalidTeam.RejectionCode == "InvalidPlayerTeam"
        && invalidTeam.Outbound.Count == 0,
        "A team outside the vanilla range must be rejected without sending a projection.");

    PacketHandlingResult zoneResult = await handler.HandleAsync(context,
        new SyncPlayerZonePacket { Player = 201, Zone1 = 1, Zone5 = 16, TownNpcCount = 3 },
        CancellationToken.None);
    Verify.That(zoneResult.Accepted
        && zoneResult.Outbound[0].Packet is SyncPlayerZonePacket projectedZone
        && projectedZone.Player == 7 && projectedZone.Zone1 == 1
        && projectedZone.Zone5 == 16 && projectedZone.TownNpcCount == 3,
        "Zone reports must preserve their flags and project the authenticated player slot.");

    foreach (short npc in new short[] { -1, 0, 199 }) {
      PacketHandlingResult talkResult = await handler.HandleAsync(context,
          new SyncTalkNPCPacket { Player = 201, NpcIndex = npc }, CancellationToken.None);
      Verify.That(talkResult.Accepted
          && talkResult.Outbound[0].Packet is SyncTalkNPCPacket projectedTalk
          && projectedTalk.Player == 7 && projectedTalk.NpcIndex == npc,
          "NPC talk reports must bind the sender, including the no-conversation sentinel.");
    }
    foreach (short npc in new short[] { -2, 200 }) {
      PacketHandlingResult talkResult = await handler.HandleAsync(context,
          new SyncTalkNPCPacket { NpcIndex = npc }, CancellationToken.None);
      Verify.That(!talkResult.Accepted && talkResult.RejectionCode == "InvalidTalkNpcIndex",
          "NPC talk reports must reject indices outside the vanilla slot table.");
    }
    foreach ((float rotation, short animation) in new[] {
        (-MathF.PI, (short)0), (0.25f, (short)30), (MathF.PI, short.MaxValue) }) {
      PacketHandlingResult animationResult = await handler.HandleAsync(context,
          new ItemRotationAndAnimationPacket {
            Player = 201, ItemRotation = rotation, ItemAnimation = animation
          }, CancellationToken.None);
      Verify.That(animationResult.Accepted && animationResult.Outbound.Count == 0,
          "Valid weapon animations must be admitted without a self echo or gameplay effects.");
    }
    foreach ((float rotation, short animation) in new[] {
        (float.NaN, (short)1), (float.PositiveInfinity, (short)1), (0f, (short)-1) }) {
      PacketHandlingResult animationResult = await handler.HandleAsync(context,
          new ItemRotationAndAnimationPacket {
            ItemRotation = rotation, ItemAnimation = animation
          }, CancellationToken.None);
      Verify.That(!animationResult.Accepted
          && animationResult.RejectionCode == "InvalidItemAnimation",
          "Weapon animation notices must reject non-finite rotations and negative timers.");
    }
  }
}
