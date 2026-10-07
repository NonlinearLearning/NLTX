using NSSLC.Infrastructure.Network;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.Relationships;
using Terraria.WorldStorage;

namespace NSSLC.NetworkVerification;

internal static class TilePlacementPacketVerification {
  public static async Task RunAsync() {
    const int size = 20;
    var cells = new TileCellState[size * size];
    cells[5 * size + 5] = new TileCellState {
      Wall = 2, Header = 0x83, TileHeader = 0x8380,
      LiquidAmount = 100, LiquidType = 1, Header3 = 0x40
    };
    var controls = new PlayerControlsObservationStore();
    var breaks = new TileBreakObservationStore();
    EntityRuntimeId worldRuntimeId = new(Guid.NewGuid());
    var handler = new WorldSynchronizationPacketHandlers(
        new WorldDataPacket { MaxTilesX = size, MaxTilesY = size },
        new TileMapSnapshot(size, size, cells), new[] { false, false, true },
        controls, breaks, new WorldSynchronizationObservation(),
        context => context.WorldRuntimeId == worldRuntimeId);
    var context = new NetworkSessionContext(new ConnectionIdentity(Guid.NewGuid(), 1),
        "placement-test", NetworkSessionStage.Active, new SenderBinding(7, Guid.NewGuid()), false,
        worldRuntimeId);
    controls.Record(7, new PlayerControlsPacket { Position = new PacketVector2(80, 80) });
    TileSectionPacket initial = await ReadSectionAsync(handler, context);
    Verify.That(!initial.Tiles[5 * size + 5].Active,
        "The placement fixture must start empty and populate the section cache.");

    var request = new TileManipulationPacket { Action = 1, X = 5, Y = 5, TileOrWallType = 1 };
    PacketHandlingResult placed = await handler.HandleAsync(context, request, CancellationToken.None);
    Verify.That(placed.Accepted && placed.Outbound.Count == 1
        && placed.Outbound[0].Packet is AreaTileChangePacket update
        && update.Tiles[0].Active && update.Tiles[0].TileType == 1,
        "An ordinary packet-17 placement must remain admitted and return the placed tile.");
    TileSectionPacket after = await ReadSectionAsync(handler, context);
    Packet10Tile tile = after.Tiles[5 * size + 5];
    Verify.That(tile.Active && tile.Type == 1 && tile.Wall == 2 && tile.WallColor == 3
        && tile.Wire && tile.Wire2 && tile.Wire3 && tile.Wire4
        && tile.LiquidAmount == 100 && tile.LiquidType == Packet10LiquidType.Lava
        && tile.InvisibleWall && tile.FullbrightWall && !tile.HalfBrick && tile.Slope == 0,
        "Placement must invalidate the cached section and preserve wall, wire and liquid state.");
    Verify.That(breaks.Snapshot().Count == 0,
        "Placement must not be recorded as a tile break.");

    request.TileOrWallType = 0;
    await handler.HandleAsync(context, request, CancellationToken.None);
    Verify.That((await ReadSectionAsync(handler, context)).Tiles[5 * size + 5].Type == 1,
        "A placement on an occupied cell must restore its existing state, not overwrite it.");
    request.X = 6;
    request.TileOrWallType = 2;
    PacketHandlingResult framed = await handler.HandleAsync(context, request, CancellationToken.None);
    Verify.That(framed.Accepted && framed.Outbound[0].Packet is AreaTileChangePacket restored
        && !restored.Tiles[0].Active,
        "Unsupported framed placement must correct the tile without closing the connection.");
    request.TileOrWallType = 3;
    PacketHandlingResult invalidType = await handler.HandleAsync(context, request,
        CancellationToken.None);
    Verify.That(!invalidType.Accepted && invalidType.RejectionCode == "InvalidPlacementTileType",
        "Unknown tile IDs must remain invalid.");
    request.TileOrWallType = 1;
    request.X = -1;
    PacketHandlingResult outside = await handler.HandleAsync(context, request, CancellationToken.None);
    Verify.That(!outside.Accepted && outside.RejectionCode == "TileOutsideWorld",
        "Placement must validate world bounds before reading tile state.");
    request.X = 19;
    request.Y = 19;
    PacketHandlingResult far = await handler.HandleAsync(context, request, CancellationToken.None);
    Verify.That(!far.Accepted && far.RejectionCode == "TileOutOfReach",
        "Placement must keep the existing reach bound.");

    await VerifyGatewayAsync(handler, controls, worldRuntimeId);
  }

  private static async Task VerifyGatewayAsync(WorldSynchronizationPacketHandlers handler,
      PlayerControlsObservationStore controls, EntityRuntimeId worldRuntimeId) {
    ProtocolProfile profile = SteamProtocolProfile.Create(
        GeneratedProfileVerification.CreateFacts(steamModules: true));
    await using var gateway = new PacketGateway(profile, new RecordingAuthority(),
        new PacketGatewayOptions { IgnoreClientVersion = true, UseSteamModuleIds = true,
          EnablePing = true }, worldRuntimeIdProvider: () => worldRuntimeId);
    GatewayVerification.RegisterProgression(gateway);
    gateway.Register<TileManipulationPacket>(new PacketPolicy(17, NetworkSessionStage.Active),
        handler);
    await using var peer = new GatewayPeer(gateway, profile);
    await peer.JoinAsync();
    controls.Record(0, new PlayerControlsPacket { Position = new PacketVector2(80, 80) });
    int before = peer.Transport.FrameCount;
    // Steam packet 17: action 1, x 7, y 5, tile type 1, style 0.
    peer.ReceiveRaw(Convert.FromHexString("0B00110107000500010000"));
    await Verify.EventuallyAsync(() => peer.Transport.FrameCount > before || peer.Run.IsCompleted,
        "The placement fixture must produce a correction or a terminal rejection.");
    Verify.That(!peer.Run.IsCompleted && peer.Session.Stage == NetworkSessionStage.Active,
        "Placing a block must not terminate an Active Steam session.");
    var changed = (AreaTileChangePacket)profile.Find(PacketDirection.ServerToClient, (byte)20)
        .Decode(peer.Transport.GetFrame(before).AsMemory(3));
    Verify.That(changed.StartX == 7 && changed.StartY == 5 && changed.Tiles[0].Active
        && changed.Tiles[0].TileType == 1,
        "The gateway must return the actual committed tile after decoding Steam's placement.");
    peer.ReceiveRaw(new byte[] { 3, 0, 154 });
    await Verify.EventuallyAsync(() => peer.Transport.FrameCount > before + 1,
        "The connection must remain responsive to latency Ping after placement.");
  }

  private static async Task<TileSectionPacket> ReadSectionAsync(
      WorldSynchronizationPacketHandlers handler, NetworkSessionContext context) {
    PacketHandlingResult section = await handler.HandleAsync(context,
        new RequestSectionPacket(), CancellationToken.None);
    return (TileSectionPacket)section.Outbound[1].Packet;
  }
}
