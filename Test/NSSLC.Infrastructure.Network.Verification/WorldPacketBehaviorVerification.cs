using System.Buffers.Binary;
using NSSLC.Infrastructure.Network;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;
using Terraria.NonAuthoritative.Persistence;
using Terraria.Relationships;
using Terraria.WorldStorage;
using Terraria.WorldGeneration.Actions;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Systems;

namespace NSSLC.NetworkVerification;

internal static class WorldPacketBehaviorVerification {
  private const int SectionWidth = 200;
  private const int SectionHeight = 150;

  private sealed class FixedFramingPort(
      WorldGenerationTileFramingAndDebugSystem.TileFrameRegion affectedRegion)
      : WorldGenerationTileFramingAndDebugSystem.IFramingPort {
    public WorldGenerationTileFramingAndDebugSystem.TileFrameRegion Frame(
        TilePosition target, bool frameNeighbors) {
      Verify.That(target == new TilePosition(199, 149) && frameNeighbors,
          "The framing command must pass its actual target and neighbor policy to the effect port.");
      return affectedRegion;
    }
  }

  private static void VerifyInterest(SectionInterestProjection? interest,
      EntityRuntimeId worldRuntimeId, (int X, int Y)[] expectedSections, string message) {
    var expected = expectedSections.Select(section =>
        new Terraria.Network.SectionCoordinate(section.X, section.Y)).ToHashSet();
    Verify.That(interest is not null && interest.WorldKey == worldRuntimeId.Value
        && interest.WorldGeneration == 0 && interest.Revision == expected.Count
        && interest.Sections.SetEquals(expected), message);
  }

  private static void VerifyFramePacket(OutboundDispatch dispatch,
      EntityRuntimeId worldRuntimeId, int sectionX, int sectionY) {
    Verify.That(dispatch.Kind == PacketDispatchKind.SectionSubscribers
        && dispatch.AllowedStages == NetworkSessionStage.Active
        && dispatch.WorldKey == worldRuntimeId.Value && dispatch.WorldGeneration == 0
        && dispatch.Section == new Terraria.Network.SectionCoordinate(sectionX, sectionY),
        "A frame event must target only the exact current-world section's active subscribers.");
    ProtocolProfile profile = SteamProtocolProfile.Create(
        GeneratedProfileVerification.CreateFacts(steamModules: true));
    byte[] frame = profile.Find(PacketDirection.ServerToClient, (byte)11).Encode(dispatch.Packet);
    Verify.That(frame.Length == 11 && frame[2] == 11
        && BinaryPrimitives.ReadInt16LittleEndian(frame.AsSpan(3)) == sectionX
        && BinaryPrimitives.ReadInt16LittleEndian(frame.AsSpan(5)) == sectionY
        && BinaryPrimitives.ReadInt16LittleEndian(frame.AsSpan(7)) == sectionX
        && BinaryPrimitives.ReadInt16LittleEndian(frame.AsSpan(9)) == sectionY,
        "Packet 11 wire endpoints must be inclusive section coordinates, not width and height.");
  }

  public static async Task RunAsync() {
    const int worldWidth = 750;
    const int worldHeight = 500;
    var tiles = new TileCellState[worldWidth * worldHeight];
    tiles[0 * worldHeight + 0] = new TileCellState {
      FrameX = -3,
      FrameY = 19,
      Header = 0x87,
      Header3 = 0xE0,
      LiquidAmount = 128,
      LiquidType = 3,
      TileHeader = 0x8FE5,
      Type = 123,
      Wall = 321
    };
    tiles[1 * worldHeight] = new TileCellState {
      FrameX = -16,
      FrameY = 32,
      TileHeader = 0x20,
      Type = 77
    };
    tiles[0 * worldHeight + 1] = new TileCellState {
      LiquidAmount = byte.MaxValue,
      LiquidType = 0,
      TileHeader = 0x20,
      Type = 88
    };
    tiles[1 * worldHeight + 1] = new TileCellState {
      FrameX = 44,
      FrameY = 55,
      TileHeader = 0x3020,
      Type = 77
    };
    var tileSnapshot = new TileMapSnapshot(worldWidth, worldHeight, tiles);
    var world = new WorldDataPacket {
      WorldId = 83,
      MaxTilesX = worldWidth,
      MaxTilesY = worldHeight,
      SpawnTileX = 10,
      SpawnTileY = 10,
      WorldName = "world packet fixture",
      WorldGuid = Guid.NewGuid().ToByteArray(),
      WorldGeneratorVersion = 1234
    };
    var controls = new PlayerControlsObservationStore();
    var observation = new WorldSynchronizationObservation();
    var frameImportant = new bool[65536];
    frameImportant[77] = true;
    frameImportant[123] = true;
    EntityRuntimeId worldRuntimeId = new(Guid.NewGuid());
    EntityRuntimeId currentWorldRuntimeId = worldRuntimeId;
    var activeSenders = new Dictionary<ConnectionIdentity, SenderBinding>();
    bool IsCurrentSender(NetworkSessionContext context) {
      return context.WorldRuntimeId == worldRuntimeId
          && activeSenders.TryGetValue(context.Connection, out SenderBinding? sender)
          && sender == context.Actor;
    }
    var handler = new WorldSynchronizationPacketHandlers(world, tileSnapshot,
        frameImportant, controls, new TileBreakObservationStore(),
        observation, IsCurrentSender, () => currentWorldRuntimeId);

    ConnectionIdentity connection = new(Guid.NewGuid(), 1);
    SenderBinding sender = new(0, Guid.NewGuid());
    activeSenders.Add(connection, sender);
    NetworkSessionContext context = CreateContext(connection, sender, worldRuntimeId,
        NetworkSessionStage.AwaitSectionRequest);
    await VerifyWorldDataGuardAsync(context, IsCurrentSender);
    PacketHandlingResult initial = await handler.HandleAsync(context,
        new SpawnTileDataPacket { X = -1, Y = -1 }, CancellationToken.None);
    (int X, int Y)[] defaultSections = [(0, 0), (0, 1), (1, 0), (1, 1), (2, 0), (2, 1)];
    VerifySpawnDispatch(initial, context, world, defaultSections,
        "The default spawn must send packet 7, progress, each clipped default section and packet 49.");
    VerifyInterest(initial.Interest, worldRuntimeId, defaultSections,
        "Packet 8 must publish the sections recorded by its connection transfer history.");
    ProtocolFacts steamFacts = GeneratedProfileVerification.CreateFacts(steamModules: true);
    VerifyProductionDirections(steamFacts);
    VerifyTileProjection(initial.Outbound[2].Packet, steamFacts);

    PacketHandlingResult selected = await handler.HandleAsync(context,
        new SpawnTileDataPacket { X = 600, Y = 300 }, CancellationToken.None);
    (int X, int Y)[] selectedOnly = [
      (1, 2), (1, 3), (2, 2), (2, 3), (3, 1), (3, 2), (3, 3)
    ];
    VerifySpawnDispatch(selected, context, world, selectedOnly,
        "Selected spawn uses the inclusive end, deduplicates overlap and clips right/bottom edges.");
    VerifyInterest(selected.Interest, worldRuntimeId,
        defaultSections.Concat(selectedOnly).Distinct().ToArray(),
        "Selected spawn must extend the same session interest with newly transferred sections.");
    TileSectionPacket bottomRight = selected.Outbound.Select(dispatch => dispatch.Packet)
        .OfType<TileSectionPacket>().Single(packet => packet.StartX == 600
            && packet.StartY == 450);
    Verify.That(bottomRight.Width == 150 && bottomRight.Height == 50
        && bottomRight.Tiles.Length == 150 * 50,
        "The selected bottom-right packet 10 must carry the exact partial section dimensions.");

    PacketHandlingResult repeated = await handler.HandleAsync(context,
        new SpawnTileDataPacket { X = 600, Y = 300 }, CancellationToken.None);
    VerifySpawnDispatch(repeated, context, world, [],
        "Repeated packet 8 on the same connection must not retransmit loaded tile sections.");
    VerifyInterest(repeated.Interest, worldRuntimeId,
        defaultSections.Concat(selectedOnly).Distinct().ToArray(),
        "A repeated packet 8 must keep the current transfer history as its interest.");
    PacketHandlingResult inclusiveUpperBound = await handler.HandleAsync(context,
        new SpawnTileDataPacket { X = worldWidth - 10, Y = worldHeight - 10 },
        CancellationToken.None);
    VerifySpawnDispatch(inclusiveUpperBound, context, world, [],
        "The valid selected-spawn upper bound maxTiles-10 is inclusive.");

    NetworkSessionContext activeContext = context with { Stage = NetworkSessionStage.Active };
    PacketHandlingResult requested = await handler.HandleAsync(activeContext,
        new RequestSectionPacket { SectionX = 3, SectionY = 0 }, CancellationToken.None);
    Verify.That(requested.Accepted && requested.Outbound.Count == 2,
        "A valid explicit section request must return its status and tile section.");
    VerifyInterest(requested.Interest, worldRuntimeId,
        defaultSections.Concat(selectedOnly).Append((3, 0)).Distinct().ToArray(),
        "An explicit packet-10 section transfer must update session interest.");

    PacketHandlingResult moved = await handler.HandleAsync(activeContext,
        new PlayerControlsPacket { Position = new(168, 4808) }, CancellationToken.None);
    Verify.That(moved.Accepted && moved.Outbound.OfType<OutboundDispatch>().Count() > 0,
        "Movement into an untransferred area must transfer nearby sections.");
    VerifyInterest(moved.Interest, worldRuntimeId,
        defaultSections.Concat(selectedOnly).Append((3, 0)).Append((0, 2)).Append((0, 3))
            .Distinct().ToArray(),
        "Movement-driven tile transfers must join the same section interest projection.");

    WorldGenerationTileFramingAndDebugSystem.TileFrameRegion actualFrameRegion =
        new(199, 149, 200, 150);
    WorldGenerationTileFramingAndDebugSystem.Result frameEvent =
        WorldGenerationTileFramingAndDebugSystem.Execute(
            WorldGenerationTileFramingAndDebugActionsCommand.SetFrames(
                new TilePosition(199, 149), frameNeighbors: true),
            new FixedFramingPort(actualFrameRegion));
    Verify.That(frameEvent.Accepted && frameEvent.FramesApplied
        && frameEvent.AffectedRegion == actualFrameRegion,
        "The real framing action result must expose its effect port's actual inclusive tile bounds.");
    TileFrameRepairDispatchResult singleSectionRepair = handler.CreateFrameRepairDispatches(
        worldRuntimeId, 0, 0, 0, 0);
    Verify.That(singleSectionRepair.Accepted && singleSectionRepair.Outbound.Count == 1,
        "The zero endpoint region must create one legal packet 11 repair dispatch.");
    VerifyFramePacket(singleSectionRepair.Outbound[0], worldRuntimeId, 0, 0);
    TileFrameRepairDispatchResult crossSectionRepair = handler.CreateFrameRepairDispatches(
        worldRuntimeId, frameEvent.AffectedRegion!.Value.StartX,
        frameEvent.AffectedRegion.Value.StartY,
        frameEvent.AffectedRegion.Value.EndXInclusive,
        frameEvent.AffectedRegion.Value.EndYInclusive);
    Verify.That(crossSectionRepair.Accepted && crossSectionRepair.Outbound.Count == 4,
        "An actual frame event crossing both section edges must project one packet 11 per affected section.");
    (int X, int Y)[] framedSections = [(0, 0), (0, 1), (1, 0), (1, 1)];
    for (int index = 0; index < framedSections.Length; index++) {
      VerifyFramePacket(crossSectionRepair.Outbound[index], worldRuntimeId,
          framedSections[index].X, framedSections[index].Y);
    }
    Verify.That(crossSectionRepair.Outbound.All(dispatch =>
        dispatch.Kind == PacketDispatchKind.SectionSubscribers
        && dispatch.AllowedStages == NetworkSessionStage.Active)
        && tileSnapshot.GetTile(199, 149).Type == 0,
        "Frame repair must use active section interest and must not mutate authoritative tile state.");
    TileFrameRepairDispatchResult clippedRepair = handler.CreateFrameRepairDispatches(
        worldRuntimeId, -20, -20, 0, 0);
    Verify.That(clippedRepair.Accepted && clippedRepair.Outbound.Count == 1,
        "A partially out-of-world frame repair must clip to its intersecting section.");
    VerifyFramePacket(clippedRepair.Outbound[0], worldRuntimeId, 0, 0);
    TileFrameRepairDispatchResult outsideRepair = handler.CreateFrameRepairDispatches(
        worldRuntimeId, worldWidth, worldHeight, worldWidth + 10, worldHeight + 10);
    Verify.That(outsideRepair.Accepted && outsideRepair.Outbound.Count == 0,
        "A frame repair fully outside the world must produce no packet.");
    TileFrameRepairDispatchResult invertedRepair = handler.CreateFrameRepairDispatches(
        worldRuntimeId, 1, 0, 0, 0);
    Verify.That(!invertedRepair.Accepted
        && invertedRepair.RejectionCode == "InvalidFrameRepairBounds"
        && invertedRepair.Outbound.Count == 0,
        "An inverted repair range must be rejected without an outbound packet.");
    currentWorldRuntimeId = new EntityRuntimeId(Guid.NewGuid());
    TileFrameRepairDispatchResult staleRepair = handler.CreateFrameRepairDispatches(
        worldRuntimeId, 0, 0, 0, 0);
    Verify.That(!staleRepair.Accepted && staleRepair.RejectionCode == "StaleWorldRuntime"
        && staleRepair.Outbound.Count == 0,
        "A repair event from an old world runtime must not produce packet 11.");
    currentWorldRuntimeId = worldRuntimeId;

    foreach (NetworkSessionContext rejectedContext in new[] {
        context with { Stage = NetworkSessionStage.Active },
        context with { WorldRuntimeId = new EntityRuntimeId(Guid.NewGuid()) },
        context with { Actor = new SenderBinding(1, Guid.NewGuid()) },
        context with { Connection = new ConnectionIdentity(Guid.NewGuid(), 1) }
      }) {
      PacketHandlingResult rejected = await handler.HandleAsync(rejectedContext,
          new SpawnTileDataPacket { X = -1, Y = -1 }, CancellationToken.None);
      Verify.That(!rejected.Accepted && rejected.Outbound.Count == 0,
          "A stale sender, stale world token or disallowed stage must produce no packet 7/9/10/49.");
    }
    foreach (SpawnTileDataPacket invalidRequest in new[] {
        new SpawnTileDataPacket { X = -1, Y = 10 },
        new SpawnTileDataPacket { X = worldWidth - 9, Y = 10 },
        new SpawnTileDataPacket { X = 10, Y = worldHeight - 9 },
        new SpawnTileDataPacket { X = -1, Y = -1, Team = 6 }
      }) {
      PacketHandlingResult rejected = await handler.HandleAsync(context, invalidRequest,
          CancellationToken.None);
      Verify.That(!rejected.Accepted && rejected.Outbound.Count == 0,
          "Invalid spawn coordinates and team values must be rejected before outbound projection.");
    }

    activeSenders.Remove(connection);
    ConnectionIdentity reusedConnection = connection with { Epoch = 2 };
    SenderBinding reusedSender = new(0, Guid.NewGuid());
    activeSenders.Add(reusedConnection, reusedSender);
    NetworkSessionContext reusedContext = CreateContext(reusedConnection, reusedSender,
        worldRuntimeId, NetworkSessionStage.AwaitSectionRequest);
    PacketHandlingResult reusedSlot = await handler.HandleAsync(reusedContext,
        new SpawnTileDataPacket { X = -1, Y = -1 }, CancellationToken.None);
    VerifySpawnDispatch(reusedSlot, reusedContext, world, defaultSections,
        "A new connection epoch reusing a player slot must receive its own initial section set.");
    Verify.That(observation.Sections.Count == 22
        && observation.Sections.Count(section => section.Reason == "spawn") == 19
        && observation.Sections.Where(section => section.Reason == "request")
            .Select(section => (section.SectionX, section.SectionY)).SequenceEqual([(3, 0)])
        && observation.Sections.Where(section => section.Reason == "movement")
            .Select(section => (section.SectionX, section.SectionY))
            .SequenceEqual([(0, 2), (0, 3)]),
        "Section observations must account for initial transfers, the explicit request and movement.");
    Verify.That(tileSnapshot.GetTile(0, 0).Type == 123
        && tileSnapshot.GetTile(1, 0).FrameX == -16
        && tileSnapshot.GetTile(0, 1).LiquidAmount == byte.MaxValue,
        "Packet 8/10 projection must not mutate authoritative tile snapshot state.");
  }

  private static NetworkSessionContext CreateContext(ConnectionIdentity connection,
      SenderBinding sender, EntityRuntimeId worldRuntimeId, NetworkSessionStage stage) {
    return new NetworkSessionContext(connection, "world-test", stage, sender, false,
        worldRuntimeId);
  }

  private static async Task VerifyWorldDataGuardAsync(NetworkSessionContext context,
      Func<NetworkSessionContext, bool> isCurrentSender) {
    var handler = new WorldDataPacketHandler(
        new WorldPersistenceDocument(319, Array.Empty<WorldPersistenceSection>()),
        isCurrentSender);
    NetworkSessionContext[] invalidContexts = [
      context with { Stage = NetworkSessionStage.Active },
      context with { WorldRuntimeId = null },
      context with { WorldRuntimeId = new EntityRuntimeId(Guid.NewGuid()) },
      context with { Actor = new SenderBinding((byte)(context.Actor.PlayerSlot + 1), Guid.NewGuid()) },
      context with { Connection = new ConnectionIdentity(Guid.NewGuid(), 1) }
    ];
    foreach (NetworkSessionContext invalidContext in invalidContexts) {
      PacketHandlingResult rejected = await handler.HandleAsync(invalidContext,
          new RequestWorldDataPacket(), CancellationToken.None);
      Verify.That(!rejected.Accepted && rejected.Outbound.Count == 0
          && rejected.NextStage is null,
          "Packet 6 must reject invalid stage, stale world runtime and stale sender before projection.");
    }
  }

  private static void VerifySpawnDispatch(PacketHandlingResult result,
      NetworkSessionContext context, WorldDataPacket world,
      IReadOnlyList<(int X, int Y)> expectedSections, string message) {
    Verify.That(result.Accepted && result.NextStage == NetworkSessionStage.Synchronizing
        && result.Outbound.Count == expectedSections.Count + 3
        && ReferenceEquals(result.Outbound[0].Packet, world)
        && result.Outbound[1].Packet is StatusTextSizePacket progress
        && progress.Value == expectedSections.Count
        && result.Outbound[^1].Packet is InitialSpawnPacket,
        message);
    TileSectionPacket[] sections = result.Outbound.Select(dispatch => dispatch.Packet)
        .OfType<TileSectionPacket>().ToArray();
    Verify.That(sections.Select(section => (section.StartX / SectionWidth,
            section.StartY / SectionHeight)).SequenceEqual(expectedSections),
        "Packet 10 section coordinates must match the ordered expected section set.");
    Verify.That(result.Outbound.All(dispatch => dispatch.Kind == PacketDispatchKind.Single
        && dispatch.Targets.SequenceEqual([context.Connection])
        && dispatch.AllowedStages == NetworkSessionStage.Synchronizing),
        "The 7/9/10/49 join projection must target only the authenticated syncing connection.");
  }

  private static void VerifyTileProjection(object packet, ProtocolFacts facts) {
    if (packet is not TileSectionPacket section) {
      throw new InvalidOperationException("Expected a packet-10 section.");
    }
    Verify.That(section.StartX == 0 && section.StartY == 0
        && section.Width == SectionWidth && section.Height == SectionHeight,
        "The first packet 10 must cover the expected full section.");
    ProtocolProfile profile = SteamProtocolProfile.Create(facts);
    PacketBinding binding = profile.Find(PacketDirection.ServerToClient,
        typeof(TileSectionPacket));
    byte[] frame = binding.Encode(packet);
    Verify.That(frame[2] == 10,
        "The handler's packet-10 projection must encode using the server-to-client packet ID.");
    var decoded = (TileSectionPacket)binding.Decode(frame.AsMemory(3));
    Verify.That(decoded.Tiles.Length == SectionWidth * SectionHeight,
        "The compressed packet 10 must expand to exactly its declared tile area.");
    Verify.That(section.Tiles[0].FrameX == -3 && section.Tiles[0].FrameY == 19
        && section.Tiles[1].FrameX == -16 && section.Tiles[1].FrameY == 32,
        "The handler projection must preserve saved tile frame coordinates before wire rules apply.");
    Packet10Tile tile = decoded.Tiles[0];
    Verify.That(tile.Active && tile.Type == 123 && tile.FrameX == 0 && tile.FrameY == 0
        && tile.TileColor == 5 && tile.Wall == 321 && tile.WallColor == 7
        && tile.LiquidAmount == 128 && tile.LiquidType == Packet10LiquidType.Shimmer
        && tile.Wire && tile.Wire2 && tile.Wire3 && tile.Wire4
        && tile.HalfBrick && tile.Slope == 0 && tile.Actuator && tile.Inactive
        && tile.InvisibleBlock && tile.InvisibleWall && tile.FullbrightBlock
        && tile.FullbrightWall,
        "Packet 10 must preserve tile, wall, liquid, wire, frame and visual flags.");
    Verify.That(decoded.Tiles[1].Type == 77 && decoded.Tiles[1].FrameX == 0
        && decoded.Tiles[1].FrameY == 0
        && decoded.Tiles[SectionWidth].Type == 88
        && decoded.Tiles[SectionWidth].LiquidType == Packet10LiquidType.Water
        && decoded.Tiles[SectionWidth + 1].Type == 77
        && decoded.Tiles[SectionWidth + 1].Slope == 3
        && !decoded.Tiles[SectionWidth + 1].HalfBrick,
        "The wire projection must obey its non-important frame facts and encode slope separately from half-brick.");
    VerifyFrameImportantWire(section);
  }

  private static void VerifyFrameImportantWire(TileSectionPacket section) {
    var frameImportant = new bool[65536];
    frameImportant[77] = true;
    frameImportant[123] = true;
    using var stream = new MemoryStream();
    var writer = new PacketWireWriter(stream);
    PacketTileEntityCodecs codecs = PacketTileEntityCodecsV4.Create();
    TileSectionPacket.WriteCompressedBody(writer, section.StartX, section.StartY,
        section.Width, section.Height, section.Tiles, section.Chests, section.Signs,
        section.TileEntities, frameImportant, Enumerable.Repeat(true, 65536).ToArray(), codecs);
    var decoded = TileSectionPacket.ReadCompressedBody(new PacketWireReader(stream.ToArray()),
        frameImportant, codecs);
    Verify.That(decoded.Tiles[0].FrameX == -3 && decoded.Tiles[0].FrameY == 19
        && decoded.Tiles[1].FrameX == -16 && decoded.Tiles[1].FrameY == 32
        && decoded.Tiles[SectionWidth + 1].FrameX == 44
        && decoded.Tiles[SectionWidth + 1].FrameY == 55,
        "Packet 10 codec must retain frame-important frames when encoder and decoder use matching facts.");
  }

  private static void VerifyProductionDirections(ProtocolFacts facts) {
    ProtocolProfile raw = SteamProtocolProfile.Create(facts);
    ProtocolProfile production = ServerProtocolProfile.Create(raw);
    foreach (byte messageId in new byte[] { 7, 10, 11, 18, 57, 146 }) {
      _ = raw.Find(PacketDirection.ServerToClient, messageId);
      Verify.Throws<PacketProtocolException>(
          () => production.Find(PacketDirection.ClientToServer, messageId));
    }
    _ = raw.Find(PacketDirection.ClientToServer, 8);
    Verify.Throws<PacketProtocolException>(
        () => production.Find(PacketDirection.ServerToClient, 8));
    _ = raw.Find(PacketDirection.ClientToServer, 65);
    _ = raw.Find(PacketDirection.ServerToClient, 65);
    _ = production.Find(PacketDirection.ClientToServer, 65);
    _ = production.Find(PacketDirection.ServerToClient, 65);
  }
}
