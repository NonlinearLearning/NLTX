using NSSLC.Infrastructure.Network;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.Relationships;
using Terraria.WorldStorage;

namespace NSSLC.NetworkVerification;

internal static class MiningPacketVerification {
  public static async Task RunAsync() {
    const int size = 20;
    var cells = new TileCellState[size * size];
    cells[5 * size + 5] = new TileCellState { TileHeader = 0x20, Type = 1 };
    cells[6 * size + 5] = new TileCellState { TileHeader = 0x20, Type = 2 };
    var controls = new PlayerControlsObservationStore();
    var breaks = new TileBreakObservationStore();
    EntityRuntimeId worldRuntimeId = new(Guid.NewGuid());
    var handler = new WorldSynchronizationPacketHandlers(
        new WorldDataPacket { MaxTilesX = size, MaxTilesY = size },
        new TileMapSnapshot(size, size, cells), new[] { false, false, true },
        controls, breaks, new WorldSynchronizationObservation(),
        context => context.WorldRuntimeId == worldRuntimeId);
    var context = new NetworkSessionContext(new ConnectionIdentity(Guid.NewGuid(), 1),
        "mining-test", NetworkSessionStage.Active, new SenderBinding(7, Guid.NewGuid()), false,
        worldRuntimeId);
    controls.Record(7, new PlayerControlsPacket { Position = new PacketVector2(80, 80) });
    TileSectionPacket initial = await ReadSectionAsync(handler, context);
    Verify.That(initial.Tiles[5 * size + 5].Active, "The fixture starts with an active block.");
    PacketHandlingResult pick = await handler.HandleAsync(context,
        new SyncTilePickingPacket { Player = 201, X = 5, Y = 5, TileType = 20 },
        CancellationToken.None);
    PacketHandlingResult sound = await new PlayerAdmissionPacketHandlers().HandleAsync(context,
        new ItemUseSoundPacket { Player = 201 }, CancellationToken.None);
    Verify.That(pick.Accepted && sound.Accepted && pick.Outbound.Count == 0
        && sound.Outbound.Count == 0 && breaks.Snapshot().Count == 0,
        "Picking and sound notices must not modify terrain or echo the claimed player slot.");
    PacketHandlingResult invalidPick = await handler.HandleAsync(context,
        new SyncTilePickingPacket { X = size, Y = 5 }, CancellationToken.None);
    Verify.That(!invalidPick.Accepted && invalidPick.RejectionCode == "TileOutsideWorld",
        "Picking notices must reject out-of-world coordinates before producing effects.");

    for (int hit = 0; hit < 8; hit++) {
      PacketHandlingResult result = await handler.HandleAsync(context,
          new TileManipulationPacket { X = 5, Y = 5, TileOrWallType = 1 },
          CancellationToken.None);
      Verify.That(result.Accepted && breaks.Snapshot().Count == 0,
          "Steam's fail=1 mining hits must keep the block intact and the connection admitted.");
    }
    TileSectionPacket afterHits = await ReadSectionAsync(handler, context);
    Verify.That(afterHits.Tiles[5 * size + 5].Active,
        "A section requested after partial hits must still contain the block.");

    PacketHandlingResult broken = await handler.HandleAsync(context,
        new TileManipulationPacket { X = 5, Y = 5 }, CancellationToken.None);
    Verify.That(broken.Accepted && breaks.Snapshot().Count == 1
        && breaks.Snapshot()[0].PlayerSlot == 7
        && broken.Outbound[0].Packet is AreaTileChangePacket changed && !changed.Tiles[0].Active,
        "The final hit must remove one block and report its authenticated actor.");
    TileSectionPacket afterBreak = await ReadSectionAsync(handler, context);
    Verify.That(!afterBreak.Tiles[5 * size + 5].Active,
        "The cached section must be invalidated when the block is actually broken.");
    PacketHandlingResult repeated = await handler.HandleAsync(context,
        new TileManipulationPacket { X = 5, Y = 5 }, CancellationToken.None);
    Verify.That(repeated.Accepted && breaks.Snapshot().Count == 1,
        "A repeated final hit must keep the connection open without recording a second break.");
    PacketHandlingResult framed = await handler.HandleAsync(context,
        new TileManipulationPacket { X = 6, Y = 5 }, CancellationToken.None);
    Verify.That(framed.Accepted && framed.Outbound[0].Packet is AreaTileChangePacket restored
        && restored.Tiles[0].Active && breaks.Snapshot().Count == 1,
        "Unsupported framed tiles must be restored without terminating a normal mining session.");
    PacketHandlingResult outside = await handler.HandleAsync(context,
        new TileManipulationPacket { X = -1, Y = 5 }, CancellationToken.None);
    Verify.That(!outside.Accepted && outside.RejectionCode == "TileOutsideWorld",
        "The mining change must retain world-bound validation.");
  }

  private static async Task<TileSectionPacket> ReadSectionAsync(
      WorldSynchronizationPacketHandlers handler, NetworkSessionContext context) {
    PacketHandlingResult section = await handler.HandleAsync(context,
        new RequestSectionPacket(), CancellationToken.None);
    return (TileSectionPacket)section.Outbound[1].Packet;
  }
}
