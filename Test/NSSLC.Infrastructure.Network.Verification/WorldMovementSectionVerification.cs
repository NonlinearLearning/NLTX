using NSSLC.Infrastructure.Network;
using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.Relationships;
using Terraria.WorldStorage;

namespace NSSLC.NetworkVerification;

internal static class WorldMovementSectionVerification {
  public static async Task RunAsync() {
    const int width = 1250;
    const int height = 760;
    var world = new WorldDataPacket {
      MaxTilesX = width, MaxTilesY = height, SpawnTileX = 610, SpawnTileY = 310
    };
    var controls = new PlayerControlsObservationStore();
    var observation = new WorldSynchronizationObservation();
    EntityRuntimeId worldRuntimeId = new(Guid.NewGuid());
    var handler = new WorldSynchronizationPacketHandlers(world,
        new TileMapSnapshot(width, height, new TileCellState[width * height]),
        new bool[1], controls, new TileBreakObservationStore(), observation,
        context => context.WorldRuntimeId == worldRuntimeId);
    var context = new NetworkSessionContext(new ConnectionIdentity(Guid.NewGuid(), 1),
        "movement-test", NetworkSessionStage.Active, new SenderBinding(7, Guid.NewGuid()), false,
        worldRuntimeId);
    PacketHandlingResult initial = await handler.HandleAsync(context with {
      Stage = NetworkSessionStage.AwaitSectionRequest
    },
        new SpawnTileDataPacket { X = -1, Y = -1 }, CancellationToken.None);
    var received = Sections(initial);
    Verify.That(initial.Accepted && received.Count == 15,
        "The initial 5 by 3 section transfer must seed the connection's transfer history.");

    PacketHandlingResult atSpawn = await MoveAsync(handler, context, 610, 310);
    Verify.That(atSpawn.Accepted && atSpawn.Outbound.Count == 0,
        "Movement inside the initial section neighborhood must not resend loaded terrain.");

    foreach ((int tileX, int tileY) in new[] { (1150, 675), (1, 1), (1249, 759), (610, 310) }) {
      HashSet<(int X, int Y)> expected = Neighborhood(tileX, tileY, width, height);
      expected.ExceptWith(received);
      PacketHandlingResult movement = await MoveAsync(handler, context, tileX, tileY);
      VerifyMovement(movement, context, expected);
      received.UnionWith(expected);
      PacketHandlingResult duplicate = await MoveAsync(handler, context, tileX, tileY);
      Verify.That(duplicate.Accepted && duplicate.Outbound.Count == 0,
          "Repeated controls and revisits must not duplicate terrain transfers.");
    }
    Verify.That(controls.TryGetLastPosition(7, out PacketVector2 boundPosition)
        && boundPosition.X == 610 * 16f && !controls.TryGetLastPosition(201, out _),
        "Movement and section selection must use the authenticated actor, not the claimed slot.");
    Verify.That(observation.Sections.Count(section => section.Reason == "movement")
        == received.Count - 15,
        "Movement observations must count exactly the newly selected sections.");

    PacketHandlingResult explicitRequest = await handler.HandleAsync(context,
        new RequestSectionPacket { SectionX = 6, SectionY = 5 }, CancellationToken.None);
    Verify.That(explicitRequest.Accepted && explicitRequest.Outbound.Count == 2
        && explicitRequest.Outbound[1].Packet is TileSectionPacket {
          StartX: 1200, StartY: 750, Width: 50, Height: 10 },
        "An explicit refresh must still resend a loaded section and clip partial world edges.");
    PacketHandlingResult newExplicitSection = await handler.HandleAsync(context,
        new RequestSectionPacket { SectionX = 0, SectionY = 4 }, CancellationToken.None);
    Verify.That(newExplicitSection.Accepted, "A new explicitly requested section must transfer.");
    received.Add((0, 4));
    HashSet<(int X, int Y)> afterExplicit = Neighborhood(50, 650, width, height);
    afterExplicit.ExceptWith(received);
    VerifyMovement(await MoveAsync(handler, context, 50, 650), context, afterExplicit);

    NetworkSessionContext otherPlayer = context with {
      Connection = new ConnectionIdentity(Guid.NewGuid(), 1),
      Actor = new SenderBinding(8, context.Actor.GameSessionKey)
    };
    VerifyMovement(await MoveAsync(handler, otherPlayer, 1150, 675), otherPlayer,
        Neighborhood(1150, 675, width, height));
    NetworkSessionContext reconnect = context with {
      Connection = context.Connection with { Epoch = 2 }
    };
    VerifyMovement(await MoveAsync(handler, reconnect, 1150, 675), reconnect,
        Neighborhood(1150, 675, width, height));

    NetworkSessionContext invalidClient = context with {
      Connection = new ConnectionIdentity(Guid.NewGuid(), 1),
      Actor = new SenderBinding(9, context.Actor.GameSessionKey)
    };
    foreach (PacketVector2 position in new[] {
        new PacketVector2(float.NaN, 16), new PacketVector2(-1, 16),
        new PacketVector2(width * 16, 16) }) {
      PacketHandlingResult invalid = await handler.HandleAsync(invalidClient,
          new PlayerControlsPacket { Position = position }, CancellationToken.None);
      Verify.That(!invalid.Accepted && invalid.Outbound.Count == 0
          && invalid.RejectionCode == "InvalidPlayerControls",
          "Invalid movement must not select sections or record transfer history.");
    }
    VerifyMovement(await MoveAsync(handler, invalidClient, 610, 310), invalidClient,
        Neighborhood(610, 310, width, height));

  }

  private static ValueTask<PacketHandlingResult> MoveAsync(
      WorldSynchronizationPacketHandlers handler, NetworkSessionContext context, int x, int y) {
    return handler.HandleAsync(context, new PlayerControlsPacket {
      Player = 201, Position = new PacketVector2(x * 16f, y * 16f)
    }, CancellationToken.None);
  }

  private static HashSet<(int X, int Y)> Neighborhood(int tileX, int tileY, int width, int height) {
    var sections = new HashSet<(int X, int Y)>();
    for (int x = Math.Max(0, tileX / 200 - 1);
        x <= Math.Min((width - 1) / 200, tileX / 200 + 1); x++) {
      for (int y = Math.Max(0, tileY / 150 - 1);
          y <= Math.Min((height - 1) / 150, tileY / 150 + 1); y++) {
        sections.Add((x, y));
      }
    }
    return sections;
  }

  private static HashSet<(int X, int Y)> Sections(PacketHandlingResult result) {
    return result.Outbound.Select(dispatch => dispatch.Packet).OfType<TileSectionPacket>()
        .Select(packet => (packet.StartX / 200, packet.StartY / 150)).ToHashSet();
  }

  private static void VerifyMovement(PacketHandlingResult result, NetworkSessionContext context,
      HashSet<(int X, int Y)> expected) {
    Verify.That(result.Accepted && Sections(result).SetEquals(expected)
        && result.Outbound.Count == (expected.Count == 0 ? 0 : expected.Count + 1),
        "Movement must transfer exactly the missing in-bounds 3 by 3 neighborhood.");
    if (expected.Count > 0) {
      Verify.That(result.Outbound[0].Packet is StatusTextSizePacket status
          && status.Value == expected.Count,
          "The progress announcement must precede terrain and match the missing section count.");
    }
    Verify.That(result.Outbound.All(dispatch => dispatch.Kind == PacketDispatchKind.Single
        && dispatch.Targets.SequenceEqual(new[] { context.Connection })
        && dispatch.AllowedStages == NetworkSessionStage.Active),
        "Movement terrain must target only the current Active connection.");
  }
}
