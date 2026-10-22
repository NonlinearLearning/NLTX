using System.Net;
using System.Numerics;
using System.Net.Sockets;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Mechanisms;
using MinimalImplementation.Server.Protocol;
using MinimalImplementation.Server.World;
using Terraria.NetWork.Prototype.PacketDesignCompiler;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

if (!TryReadArguments(args, out string scenario, out string outputRoot) ||
    !string.Equals(scenario, "SC071", StringComparison.Ordinal))
{
  Console.Error.WriteLine(
    "Usage: --scenario SC071 --output-root <Build/diagnostics/MinimalImplementation/P06/SC071>");
  return 2;
}

Directory.CreateDirectory(outputRoot);
string resultPath = Path.Combine(outputRoot, "SC071-result.md");
try
{
  string result = await RunSc071Async();
  File.WriteAllText(resultPath, result);
  Console.WriteLine("SC071 PASS");
  Console.WriteLine($"Evidence: {resultPath}");
  return 0;
}
catch (Exception exception)
{
  string failure = $"# SC071 result{Environment.NewLine}{Environment.NewLine}" +
    "Status: FAIL" + Environment.NewLine + Environment.NewLine +
    $"Failure: {exception.GetType().Name}: {exception.Message}{Environment.NewLine}";
  File.WriteAllText(resultPath, failure);
  Console.Error.WriteLine(failure);
  return 1;
}

static async Task<string> RunSc071Async()
{
  WorldFacts facts = WorldFacts.CreateVersion4();
  Version4ProtocolFactsInput protocolFacts = CreateProtocolFacts(facts);
  Version4ProtocolFactsFactory.Create(protocolFacts);
  var wireProfile = new Version4WireProfile(protocolFacts);
  var factsCatalog = MechanismFactsCatalog.CreateVersion4();

  var worldOwner = new WorldOwner(facts);
  WorldPublicationResult publication = worldOwner.CommitPreparedWorld(
    worldOwner.PrepareWorld(CreateFixture(facts)));
  Require(publication.Published && publication.Version is not null,
    "SC071 fixture publication failed.");
  CommitVersion publishedVersion = publication.Version
    ?? throw new InvalidOperationException("SC071 fixture publication had no version.");
  WorldSnapshot before = worldOwner.CaptureSnapshot();
  Require(before.Version == publishedVersion,
    "SC071 fixture version did not match the publication result.");
  Require(before.GetTile(new TileCoordinate(40, 40)).Type == 136 &&
    before.GetTile(new TileCoordinate(40, 40)).FrameY == 0,
    "SC071 fixture switch was not initialized at frameY 0.");
  Require(before.GetTile(new TileCoordinate(44, 40)).Type == 421 &&
    !before.GetTile(new TileCoordinate(44, 40)).Actuator,
    "SC071 fixture mechanism was not initialized as actuator-free type 421.");
  for (int x = 40; x <= 44; x++)
  {
    Require((before.GetTile(new TileCoordinate(x, 40)).Wires & TileWireMask.Red) != 0,
      $"SC071 red wire is not continuous at ({x},40).");
  }

  IReadOnlyList<string> deviceFootprintEvidence = VerifyDeviceFootprintPackets(
    facts,
    factsCatalog,
    wireProfile);
  IReadOnlyList<string> animatedDeviceEvidence = VerifyAnimatedDevicePackets(
    facts, factsCatalog, wireProfile);
  IReadOnlyList<string> singleTileDeviceEvidence = VerifySingleTileDevicePackets(
    facts, factsCatalog, wireProfile);
  string pixelBoxEvidence = VerifyPixelBoxPackets(facts, factsCatalog, wireProfile);
  string timerEvidence = VerifyTimerTransactions(facts, factsCatalog, wireProfile);
  string leashTickAtomicityEvidence = VerifyLeashedSectionTickAtomicity(factsCatalog, wireProfile);
  string emptyRecipientRemovalEvidence = VerifyEmptyRecipientRemovals(
    facts,
    factsCatalog,
    wireProfile);

  SessionEpoch sessionA = SessionEpoch.Create(7001);
  SessionEpoch sessionB = SessionEpoch.Create(7002);
  var request = new HitSwitchPacket { X = 40, Y = 40 };
  byte[] inboundPayload = wireProfile.Encode(
    WireDirection.ClientToServer,
    request).Payload.ToArray();
  byte[] inboundFrame = PacketFrameCodec.Encode(HitSwitchPacketHandler.MessageId, inboundPayload);

  var worldPort = new MechanismWorldPort(worldOwner);
  var propagation = new WiringPropagationService(factsCatalog);
  var producer = new MechanismPacketProducer(wireProfile, facts);
  var handler = new HitSwitchPacketHandler(worldPort, propagation, producer);
  var dispatchSource = new TaskCompletionSource<HitSwitchDispatchResult>(
    TaskCreationOptions.RunContinuationsAsynchronously);
  Version4PacketConnection? serverA = null;
  Version4PacketConnection? serverB = null;
  var sendReceipts = new List<SendReceipt>();
  var options = new PacketTcpConnectionOptions
  {
    ReceiveBufferBytes = 32,
    MaximumReceiveBufferedBytes = 64 * 1024,
    MaximumPendingSendBytes = 64 * 1024,
    MaximumPendingSendFrames = 8,
    ReceiveNoProgressDeadline = TimeSpan.FromSeconds(2),
    SendDeadline = TimeSpan.FromSeconds(2)
  };
  using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));
  var listener = new TcpListener(
    IPAddress.Loopback,
    0);
  listener.Start();
  int port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
  Task<Socket> acceptedA = listener
    .AcceptSocketAsync(cancellation.Token)
    .AsTask();
  using var clientA = new Socket(
    AddressFamily.InterNetwork,
    SocketType.Stream,
    ProtocolType.Tcp);
  await clientA.ConnectAsync(
    IPAddress.Loopback,
    port,
    cancellation.Token).ConfigureAwait(false);
  Socket serverSocketA = await acceptedA.ConfigureAwait(false);

  Task<Socket> acceptedB = listener
    .AcceptSocketAsync(cancellation.Token)
    .AsTask();
  using var clientB = new Socket(
    AddressFamily.InterNetwork,
    SocketType.Stream,
    ProtocolType.Tcp);
  await clientB.ConnectAsync(
    IPAddress.Loopback,
    port,
    cancellation.Token).ConfigureAwait(false);
  Socket serverSocketB = await acceptedB.ConfigureAwait(false);
  listener.Stop();

  static InboundPacketEnvelope CreateActiveEnvelope(
    InboundRawFrame frame,
    ulong worldEpoch)
  {
    return new InboundPacketEnvelope(
      frame.Epoch,
      frame.Sequence,
      frame.ReceivedAt,
      frame.Frame.MessageId,
      frame.Frame.ModuleId,
      frame.Frame.FrameBytes,
      stage: "Active",
      worldScope: checked((long)worldEpoch),
      entityGeneration: null,
      expectedRevision: null);
  }

  async ValueTask HandleAAsync(
    InboundVersion4Packet inbound,
    CancellationToken dispatchCancellation)
  {
    if (inbound.Handling != Version4PacketHandling.Typed ||
        inbound.DecodedPacket is not HitSwitchPacket decodedRequest)
    {
      throw new InvalidOperationException(
        $"SC071 TCP server received an unexpected packet-59 dispatch: {inbound.Reason}");
    }

    HitSwitchDispatchResult result = handler.Handle(
      inbound.Envelope,
      authenticatedPlayerSlot: 0,
      decodedRequest,
      [sessionA, sessionB]);
    if (serverA is null || serverB is null)
    {
      throw new InvalidOperationException("SC071 TCP server connections were not ready.");
    }

    foreach (OutboundPacketIntent intent in result.OutboundIntents)
    {
      sendReceipts.Add(await serverA.SendAsync(intent, dispatchCancellation)
        .ConfigureAwait(false));
      sendReceipts.Add(await serverB.SendAsync(intent, dispatchCancellation)
        .ConfigureAwait(false));
    }
    dispatchSource.TrySetResult(result);
  }

  static ValueTask HandleBAsync(
    InboundVersion4Packet inbound,
    CancellationToken dispatchCancellation)
  {
    _ = dispatchCancellation;
    throw new InvalidOperationException(
      $"SC071 observer B received an unexpected inbound packet: {inbound.Envelope.MessageId}.");
  }

  serverA = new Version4PacketConnection(
    serverSocketA,
    sessionA,
    wireProfile,
    frame => CreateActiveEnvelope(frame, before.WorldEpoch),
    HandleAAsync,
    options: options);
  serverB = new Version4PacketConnection(
    serverSocketB,
    sessionB,
    wireProfile,
    frame => CreateActiveEnvelope(frame, before.WorldEpoch),
    HandleBAsync,
    options: options);
  Task serverRunA = serverA.RunAsync(cancellation.Token);
  Task serverRunB = serverB.RunAsync(cancellation.Token);
  Task<IReadOnlyList<RawPacketFrame>> clientAFrames = ReceiveFramesAsync(
    clientA,
    expectedCount: 2,
    cancellation.Token);
  Task<IReadOnlyList<RawPacketFrame>> clientBFrames = ReceiveFramesAsync(
    clientB,
    expectedCount: 3,
    cancellation.Token);
  await SendAllAsync(clientA, inboundFrame, cancellation.Token).ConfigureAwait(false);
  HitSwitchDispatchResult dispatch = await dispatchSource.Task
    .WaitAsync(cancellation.Token)
    .ConfigureAwait(false);
  IReadOnlyList<RawPacketFrame> aFrames = await clientAFrames.ConfigureAwait(false);
  IReadOnlyList<RawPacketFrame> bFrames = await clientBFrames.ConfigureAwait(false);
  clientA.Shutdown(SocketShutdown.Send);
  clientB.Shutdown(SocketShutdown.Send);
  await Task.WhenAll(serverRunA, serverRunB).ConfigureAwait(false);

  Require(dispatch.Status == HitSwitchStatus.Applied,
    $"SC071 packet 59 was not applied: {dispatch.Rejection}");
  CommitVersion commitVersion = dispatch.CommitVersion
    ?? throw new InvalidOperationException("SC071 did not return a committed version.");
  Require(dispatch.VisitedTiles.Count == 5,
    "SC071 did not traverse exactly the five red-wire fixture tiles.");
  Require(dispatch.OutboundIntents.Count == 3 &&
    dispatch.OutboundIntents.Select(intent => intent.MessageId)
      .SequenceEqual(new byte[] { 59, 20, 20 }),
    "SC071 expected packet 59, switch packet 20, then device packet 20.");

  WorldSnapshot after = worldOwner.CaptureSnapshot();
  WorldTile afterSwitch = after.GetTile(new TileCoordinate(40, 40));
  WorldTile afterMechanism = after.GetTile(new TileCoordinate(44, 40));
  Require(afterSwitch.FrameY == 18,
    $"SC071 switch frameY was {afterSwitch.FrameY}, expected 18.");
  Require(afterMechanism.Type == 422 && !afterMechanism.Actuator,
    "SC071 mechanism did not change from type 421 to actuator-free type 422.");
  Require(after.Version == commitVersion,
    "SC071 P03 snapshot version did not match the mechanism commit.");

  OutboundPacketIntent intent59 = dispatch.OutboundIntents.Single(
    intent => intent.MessageId == HitSwitchPacketHandler.MessageId);
  OutboundPacketIntent[] intents20 = dispatch.OutboundIntents
    .Where(intent => intent.MessageId == 20).ToArray();
  Require(intent59.TargetEpochs.Contains(sessionA) &&
    intent59.TargetEpochs.Contains(sessionB) && intent59.IgnoreEpoch == sessionA,
    "SC071 packet 59 intent did not retain both targets with the sender exclusion.");
  Require(intents20.All(intent => intent.TargetEpochs.Count == 2 &&
    intent.TargetEpochs.Contains(sessionA) && intent.TargetEpochs.Contains(sessionB) &&
    intent.IgnoreEpoch is null && intent.CommitVersion == commitVersion),
    "SC071 packet 20 intent did not target both active sessions.");
  Require(sendReceipts.Count == 6 &&
    sendReceipts[0].Certainty == TransportSendCertainty.NotSubmitted &&
    sendReceipts.Skip(1).All(receipt => receipt.Certainty == TransportSendCertainty.LocallySent),
    "SC071 transport receipts did not match sender exclusion and two-session delivery.");

  IReadOnlyList<RawPacketFrame> a59 = aFrames
    .Where(frame => frame.MessageId == HitSwitchPacketHandler.MessageId)
    .ToArray();
  IReadOnlyList<RawPacketFrame> b59 = bFrames
    .Where(frame => frame.MessageId == HitSwitchPacketHandler.MessageId)
    .ToArray();
  Require(a59.Count == 0, "SC071 sender A received a packet 59 echo.");
  Require(b59.Count == 1, "SC071 observer B did not receive exactly one packet 59.");
  Require(b59[0].Payload.Span.SequenceEqual(inboundPayload),
    "SC071 packet 59 payload changed during the server broadcast.");

  IReadOnlyList<RawPacketFrame> a20 = aFrames
    .Where(frame => frame.MessageId == 20)
    .ToArray();
  IReadOnlyList<RawPacketFrame> b20 = bFrames
    .Where(frame => frame.MessageId == 20)
    .ToArray();
  Require(a20.Count == 2 && b20.Count == 2 &&
    aFrames.Select(frame => frame.MessageId).SequenceEqual(new byte[] { 20, 20 }) &&
    bFrames.Select(frame => frame.MessageId).SequenceEqual(new byte[] { 59, 20, 20 }),
    "SC071 switch/device packets did not reach both sessions in order.");
  for (int index = 0; index < 2; index++)
  {
    Require(a20[index].Payload.Span.SequenceEqual(b20[index].Payload.Span) &&
      a20[index].Payload.Span.SequenceEqual(intents20[index].Payload.Span),
      "SC071 packet 20 payload changed between intent, sender and observer.");
    var decoded20 = (AreaTileChangePacket)wireProfile.Decode(
      WireDirection.ServerToClient, 20, a20[index].Payload);
    var coordinate = new TileCoordinate(index == 0 ? 40 : 44, 40);
    Require(decoded20.StartX == coordinate.X && decoded20.StartY == coordinate.Y &&
      decoded20.Width == 1 && decoded20.Height == 1 && decoded20.ChangeType == 0 &&
      decoded20.Tiles.Length == 1,
      "SC071 packet 20 did not preserve separate 1x1 switch and device refreshes.");
    AssertDecodedTileMatches(facts, decoded20.Tiles[0], after.GetTile(coordinate), coordinate);
  }

  string inboundFrameHex = Hex(inboundFrame);
  string packet59FrameHex = Hex(b59[0].FrameBytes.Span);
  string packet20FrameHex = string.Join("; ", a20.Select(frame => Hex(frame.FrameBytes.Span)));
  string deviceFootprintLines = string.Join(
    Environment.NewLine,
    deviceFootprintEvidence.Concat(animatedDeviceEvidence).Concat(singleTileDeviceEvidence)
      .Select(item => $"- {item}"));
  string generatedPath = Path.GetFullPath(
    Path.Combine("Build", "generated", "MinimalImplementation.Protocol", "Debug", "net10.0"));
  return $@"# SC071 result

Status: PASS

Input:

- P03 fixture commit version: `{before.Version.Value}`; world epoch: `{before.WorldEpoch}`.
- Active sessions: A=`{sessionA.Value}`, B=`{sessionB.Value}`.
- A sent exactly one packet 59 with payload `Int16(40), Int16(40)`.
- Inbound packet 59 frame bytes: `{inboundFrameHex}`.
- Fixture: `(40,40)` type `136`, `frameY=0`; `(44,40)` type `421`, `actuator=false`; red wire was present at every x from 40 through 44.

Committed P03 state:

- Mechanism commit version: `{commitVersion.Value}`.
- Switch `(40,40)`: type `{afterSwitch.Type}`, frameY `{afterSwitch.FrameY}`.
- Mechanism `(44,40)`: type `{afterMechanism.Type}`, actuator `{afterMechanism.Actuator}`.
- Traversed red-wire tiles: `{string.Join(", ", dispatch.VisitedTiles.Select(coordinate => coordinate.ToString()))}`.

Protocol evidence:

- Transport: two loopback TCP sockets used production `Version4PacketConnection`; A's frame was assembled from the socket and typed-decoded at the `Active` stage before the P06 handler ran.
- Send receipts in intent order (59 to A/B, switch 20 to A/B, device 20 to A/B): `NotSubmitted`, `LocallySent`, `LocallySent`, `LocallySent`, `LocallySent`, `LocallySent`.
- Packet 59: B received one payload `{Hex(b59[0].Payload.Span)}`; A received zero packet 59 echoes; effective target session was `{sessionB.Value}` and the intent ignored `{sessionA.Value}`.
- Packet 59 frame bytes delivered to B: `{packet59FrameHex}`.
- Packet 20: A and B each received the same two ordered payloads; intent target sessions were `{sessionA.Value},{sessionB.Value}`.
- Packet 20 frame bytes: `{packet20FrameHex}`.
- Decoded packet 20 rectangles: switch `(40,40)` size `1x1` with frameY `18`, then device `(44,40)` size `1x1` with type `422`; both decoded tiles matched the corresponding committed P03 state (frame coordinates compared when the shared facts mark the type frame-important).

Additional P06 checks:

{deviceFootprintLines}
- Pixel boxes: {pixelBoxEvidence}
- Timers: {timerEvidence}
- Leashed section tick atomicity: {leashTickAtomicityEvidence}
- Empty-recipient removals: {emptyRecipientRemovalEvidence}

Verification:

- Project: `src/MinimalImplementation/Verification/Mechanisms/MinimalImplementation.Verification.Mechanisms.csproj`.
- Generated protocol output: `{generatedPath}`.
- SC072-SC080: not run.
  ";
}

static IReadOnlyList<string> VerifyDeviceFootprintPackets(
  WorldFacts facts,
  MechanismFactsCatalog factsCatalog,
  Version4WireProfile wireProfile)
{
  const int anchorX = 44;
  const int anchorY = 40;
  var cases = new (ushort Type, int Width, int Height, short FrameStep)[]
  {
    (410, 2, 3, 56),
    (733, 3, 3, 54),
    (207, 2, 4, 72),
    (406, 3, 3, 54)
  };
  var evidence = new List<string>(cases.Length);

  foreach ((ushort tileType, int width, int height, short frameStep) in cases)
  {
    var deviceTiles = new List<(TileCoordinate Coordinate, WorldTile Tile)>(width * height);
    for (int column = 0; column < width; column++)
    {
      for (int row = 0; row < height; row++)
      {
        var coordinate = new TileCoordinate(anchorX + column, anchorY + row);
        var tile = new WorldTile(
          Active: true,
          Type: tileType,
          Wall: 0,
          FrameX: (short)(column * 18),
          FrameY: (short)(row * 18),
          LiquidAmount: 0,
          LiquidType: LiquidType.Water,
          TileColor: 0,
          WallColor: 0,
          Wires: column == 0 && row == 0 ? TileWireMask.Red : TileWireMask.None,
          Actuator: false,
          Inactive: false);
        deviceTiles.Add((coordinate, tile));
      }
    }

    var worldOwner = new WorldOwner(facts);
    WorldPublicationResult publication = worldOwner.CommitPreparedWorld(
      worldOwner.PrepareWorld(CreateFixture(facts, deviceTiles)));
    Require(publication.Published,
      $"SC071 type-{tileType} footprint fixture publication failed: {publication.Rejection}");
    WorldSnapshot before = worldOwner.CaptureSnapshot();
    var propagation = new WiringPropagationService(factsCatalog);
    WiringPropagationPlan plan = propagation.Plan(
      before,
      new HitSwitchPacket { X = 40, Y = 40 });
    Require(plan.Accepted,
      $"SC071 type-{tileType} wiring plan was rejected: {plan.Rejection}");

    var expectedBounds = new WorldSectionBounds(anchorX, anchorY, width, height);
    Require(plan.TileSquareUpdates.Count == 1 && plan.TileSquareUpdates[0] == expectedBounds,
      $"SC071 type-{tileType} plan did not request the complete {width}x{height} footprint.");
    var expectedChangedTiles = new HashSet<TileCoordinate>
    {
      new(40, 40)
    };
    for (int column = 0; column < width; column++)
    {
      for (int row = 0; row < height; row++)
      {
        var coordinate = new TileCoordinate(anchorX + column, anchorY + row);
        short expectedFrameY = checked((short)(row * 18 + frameStep));
        Require(plan.Deltas.TryGetValue(coordinate, out WorldTileDelta delta) &&
          delta.FrameY == expectedFrameY,
          $"SC071 type-{tileType} plan omitted the frame change at {coordinate}.");
        expectedChangedTiles.Add(coordinate);
      }
    }
    Require(plan.Deltas.Count == expectedChangedTiles.Count &&
      expectedChangedTiles.All(plan.Deltas.ContainsKey),
      $"SC071 type-{tileType} plan did not contain exactly the switch and footprint deltas.");

    var producerDeltas = plan.Deltas.ToDictionary(pair => pair.Key, pair => pair.Value);
    producerDeltas.Add(new TileCoordinate(70, 70), new WorldTileDelta(Active: true, Type: 1));
    producerDeltas.Add(new TileCoordinate(71, 70), new WorldTileDelta(Active: true, Type: 2));
    KeyValuePair<TileCoordinate, WorldTileDelta>[] orderedDeltas = producerDeltas
      .OrderBy(pair => pair.Key.Y)
      .ThenBy(pair => pair.Key.X)
      .ToArray();
    var worldPort = new MechanismWorldPort(worldOwner);
    bool preparedSuccessfully = worldPort.TryPrepareTileDeltas(
      before.WorldEpoch,
      before.Version,
      orderedDeltas,
      producesImmediateBroadcast: true,
      out WorldTileMutationPreparation? preparation,
      out WorldValidationResult validation);
    Require(preparedSuccessfully && preparation is not null && validation.IsValid,
      $"SC071 type-{tileType} tile deltas were not prepared: {string.Join("; ", validation.Errors)}");
    WorldTileMutationPreparation prepared = preparation
      ?? throw new InvalidOperationException(
        $"SC071 type-{tileType} preparation was missing after a successful result.");
    CommitVersion commitVersion = prepared.Result.Version
      ?? throw new InvalidOperationException(
        $"SC071 type-{tileType} preparation did not allocate a commit version.");

    SessionEpoch[] recipients = [SessionEpoch.Create(7400UL + tileType)];
    var producer = new MechanismPacketProducer(wireProfile, facts);
    IReadOnlyList<OutboundPacketIntent> intents = producer.CreateTileChangeIntents(
      before,
      prepared,
      recipients,
      plan.TileSquareUpdates);
    Require(intents.Count == 3 && intents.All(intent => intent.MessageId == 20),
      $"SC071 type-{tileType} expected three packet-20 intents for switch, 32x32 bucket and footprint.");

    AreaTileChangePacket[] decodedPackets = intents
      .Select(intent => (AreaTileChangePacket)wireProfile.Decode(
        WireDirection.ServerToClient,
        intent.MessageId,
        intent.Payload))
      .ToArray();
    AreaTileChangePacket switchPacket = decodedPackets.Single(packet =>
      packet.StartX == 40 && packet.StartY == 40);
    Require(switchPacket.Width == 1 && switchPacket.Height == 1 &&
      switchPacket.Tiles.Length == 1,
      $"SC071 type-{tileType} switch change did not remain outside the device square.");
    AreaTileChangePacket bucketPacket = decodedPackets.Single(packet =>
      packet.StartX == 70 && packet.StartY == 70);
    Require(bucketPacket.Width == 2 && bucketPacket.Height == 1 &&
      bucketPacket.Tiles.Length == 2,
      $"SC071 type-{tileType} out-of-footprint changes were not grouped by their 32x32 bucket.");
    AreaTileChangePacket devicePacket = decodedPackets.Single(packet =>
      packet.StartX == anchorX && packet.StartY == anchorY);
    Require(devicePacket.Width == width && devicePacket.Height == height &&
      devicePacket.ChangeType == 0 && devicePacket.Tiles.Length == width * height,
      $"SC071 type-{tileType} packet 20 did not encode the complete {width}x{height} footprint.");

    Require(prepared.Validate().IsValid,
      $"SC071 type-{tileType} world preparation became stale before commit.");
    prepared.Commit();
    WorldSnapshot after = worldOwner.CaptureSnapshot();
    Require(after.Version == commitVersion,
      $"SC071 type-{tileType} committed snapshot version did not match the prepared version.");
    int index = 0;
    for (int column = 0; column < width; column++)
    {
      for (int row = 0; row < height; row++)
      {
        var coordinate = new TileCoordinate(anchorX + column, anchorY + row);
        WorldTile committedTile = after.GetTile(coordinate);
        Packet20Tile decodedTile = devicePacket.Tiles[index++];
        AssertDecodedTileMatches(facts, decodedTile, committedTile, coordinate);
        Require(decodedTile.FramePresent && decodedTile.FrameY == committedTile.FrameY,
          $"SC071 type-{tileType} packet 20 omitted or changed frameY at {coordinate}.");
        Require(committedTile.FrameY == checked((short)(row * 18 + frameStep)),
          $"SC071 type-{tileType} committed frameY was incorrect at {coordinate}.");
      }
    }
    Require(after.GetTile(new TileCoordinate(anchorX, anchorY)).FrameY == frameStep,
      $"SC071 type-{tileType} anchor frameY did not change from 0 to {frameStep}.");
    string mechanismPath = tileType == 406 ? " (shared ShiftFrames)" : string.Empty;
    evidence.Add(
      $"Tile {tileType}{mechanismPath}: packet 20 at ({anchorX},{anchorY}) covered {width}x{height} " +
      $"({devicePacket.Tiles.Length} decoded tiles) and matched the committed frames; " +
      "the switch stayed in its own intent and two extra tiles shared one 32x32 bucket.");
  }

  evidence.Add(VerifySwitchMb(facts, factsCatalog, wireProfile));

  return Array.AsReadOnly(evidence.ToArray());
}

static string VerifySwitchMb(
  WorldFacts facts,
  MechanismFactsCatalog factsCatalog,
  Version4WireProfile wireProfile)
{
  const int anchorX = 44;
  const int anchorY = 40;
  var deviceTiles = new List<(TileCoordinate Coordinate, WorldTile Tile)>(4);
  for (int column = 0; column < 2; column++)
  {
    for (int row = 0; row < 2; row++)
    {
      var coordinate = new TileCoordinate(anchorX + column, anchorY + row);
      var tile = new WorldTile(
        Active: true,
        Type: (ushort)((column + row) % 2 == 0 ? 35 : 139),
        Wall: 0,
        FrameX: (short)(column * 18),
        FrameY: (short)(row * 18),
        LiquidAmount: 0,
        LiquidType: LiquidType.Water,
        TileColor: 0,
        WallColor: 0,
        Wires: column == 0 && row == 0 ? TileWireMask.Red : TileWireMask.None,
        Actuator: false,
        Inactive: false);
      deviceTiles.Add((coordinate, tile));
    }
  }

  var worldOwner = new WorldOwner(facts);
  WorldPublicationResult publication = worldOwner.CommitPreparedWorld(
    worldOwner.PrepareWorld(CreateFixture(facts, deviceTiles)));
  Require(publication.Published,
    $"SC071 SwitchMB fixture publication failed: {publication.Rejection}");
  WorldSnapshot before = worldOwner.CaptureSnapshot();
  var propagation = new WiringPropagationService(factsCatalog);
  WiringPropagationPlan plan = propagation.Plan(
    before,
    new HitSwitchPacket { X = 40, Y = 40 });
  Require(plan.Accepted,
    $"SC071 SwitchMB was rejected: {plan.Rejection}");

  var expectedBounds = new WorldSectionBounds(anchorX, anchorY, 2, 2);
  Require(plan.TileSquareUpdates.Count == 1 && plan.TileSquareUpdates[0] == expectedBounds,
    "SC071 SwitchMB did not request the complete 2x2 tile square.");
  Require(plan.Deltas.Count == 5 &&
    plan.Deltas.TryGetValue(new TileCoordinate(40, 40), out WorldTileDelta switchDelta) &&
    switchDelta.FrameY == 18,
    "SC071 SwitchMB did not retain the trigger tile change.");
  for (int column = 0; column < 2; column++)
  {
    for (int row = 0; row < 2; row++)
    {
      var coordinate = new TileCoordinate(anchorX + column, anchorY + row);
      short expectedFrameX = checked((short)(column * 18 + 36));
      Require(plan.Deltas.TryGetValue(coordinate, out WorldTileDelta delta) &&
        delta.FrameX == expectedFrameX &&
        delta.FrameY == checked((short)(row * 18)),
        $"SC071 SwitchMB produced incorrect frame data at {coordinate}.");
    }
  }

  KeyValuePair<TileCoordinate, WorldTileDelta>[] deltas = plan.Deltas
    .OrderBy(pair => pair.Key.Y)
    .ThenBy(pair => pair.Key.X)
    .ToArray();
  var worldPort = new MechanismWorldPort(worldOwner);
  bool preparedSuccessfully = worldPort.TryPrepareTileDeltas(
    before.WorldEpoch,
    before.Version,
    deltas,
    producesImmediateBroadcast: true,
    out WorldTileMutationPreparation? preparation,
    out WorldValidationResult validation);
  Require(preparedSuccessfully && preparation is not null && validation.IsValid,
    $"SC071 SwitchMB deltas were not prepared: {string.Join("; ", validation.Errors)}");
  WorldTileMutationPreparation prepared = preparation
    ?? throw new InvalidOperationException(
      "SC071 SwitchMB preparation was missing after a successful result.");
  var producer = new MechanismPacketProducer(wireProfile, facts);
  IReadOnlyList<OutboundPacketIntent> intents = producer.CreateTileChangeIntents(
    before,
    prepared,
    [SessionEpoch.Create(7435)],
    plan.TileSquareUpdates);
  Require(intents.Count == 2 && intents.All(intent => intent.MessageId == 20),
    "SC071 SwitchMB did not produce one trigger and one footprint packet.");
  AreaTileChangePacket[] packets = intents
    .Select(intent => (AreaTileChangePacket)wireProfile.Decode(
      WireDirection.ServerToClient,
      intent.MessageId,
      intent.Payload))
    .ToArray();
  AreaTileChangePacket devicePacket = packets.Single(packet =>
    packet.StartX == anchorX && packet.StartY == anchorY);
  Require(devicePacket.Width == 2 && devicePacket.Height == 2 &&
    devicePacket.Tiles.Length == 4,
    "SC071 SwitchMB packet 20 did not cover the complete 2x2 footprint.");
  for (int column = 0; column < 2; column++)
  {
    for (int row = 0; row < 2; row++)
    {
      int index = column * 2 + row;
      Packet20Tile tile = devicePacket.Tiles[index];
      var coordinate = new TileCoordinate(anchorX + column, anchorY + row);
      Require(tile.Active && tile.FramePresent &&
        tile.TileType == (ushort)((column + row) % 2 == 0 ? 35 : 139) &&
        tile.FrameX == checked((short)(column * 18 + 36)) &&
        tile.FrameY == checked((short)(row * 18)),
        $"SC071 SwitchMB packet 20 did not preserve the mixed tile at {coordinate}.");
    }
  }

  Require(prepared.Validate().IsValid,
    "SC071 SwitchMB world preparation became stale before commit.");
  prepared.Commit();
  WorldSnapshot after = worldOwner.CaptureSnapshot();
  Require(after.GetTile(new TileCoordinate(anchorX, anchorY)).FrameX == 36 &&
    after.GetTile(new TileCoordinate(anchorX + 1, anchorY)).FrameX == 54 &&
    after.GetTile(new TileCoordinate(anchorX, anchorY + 1)).FrameX == 36 &&
    after.GetTile(new TileCoordinate(anchorX + 1, anchorY + 1)).FrameX == 54,
    "SC071 SwitchMB world commit did not toggle both registered tile types.");

  return "Tiles 35/139: the mixed 2x2 SwitchMB device toggled its active frames " +
    "and emitted one complete packet-20 footprint.";
}

static IReadOnlyList<string> VerifySingleTileDevicePackets(
  WorldFacts facts,
  MechanismFactsCatalog factsCatalog,
  Version4WireProfile wireProfile)
{
  const TileWireMask allColors = TileWireMask.Red | TileWireMask.Blue |
    TileWireMask.Green | TileWireMask.Yellow;
  var cases = new List<(ushort Type, short InitialFrame, ushort[] Types,
    short[] Frames, int Size)>();
  foreach ((ushort left, ushort right) in new (ushort, ushort)[]
    { (421, 422), (255, 262), (256, 263), (257, 264), (258, 265), (259, 266),
      (260, 267), (261, 268), (546, 557) })
  {
    cases.Add((left, -1, [right, left, right, left], [-1, -1, -1, -1], 1));
    cases.Add((right, -1, [left, right, left, right], [-1, -1, -1, -1], 1));
  }
  foreach ((ushort type, short step, int size) in new (ushort, short, int)[]
    { (4, 66, 1), (33, 18, 3), (49, 18, 3), (174, 18, 3), (372, 18, 3),
      (646, 18, 3), (149, 54, 1) })
  {
    cases.Add((type, 0, [type, type, type, type], [step, 0, step, 0], size));
    cases.Add((type, step, [type, type, type, type], [0, step, 0, step], size));
  }
  cases.Add((429, 0, [429, 429, 429, 429], [18, 90, 126, 270], 1));
  cases.Add((429, 270, [429, 429, 429, 429], [252, 180, 144, 0], 1));

  var evidence = new List<string>();
  var origin = new TileCoordinate(40, 40);
  var device = new TileCoordinate(44, 40);
  SessionEpoch sender = SessionEpoch.Create(7421);
  SessionEpoch observer = SessionEpoch.Create(7422);
  var request = new HitSwitchPacket { X = 40, Y = 40 };
  byte[] rawFrame = PacketFrameCodec.Encode(HitSwitchPacketHandler.MessageId,
    wireProfile.Encode(WireDirection.ClientToServer, request).Payload.ToArray());
  foreach (var testCase in cases)
  {
    foreach (int colorCount in new[] { 1, 4 })
    {
      TileWireMask wires = colorCount == 4 ? allColors : TileWireMask.Red;
      var tiles = new List<(TileCoordinate Coordinate, WorldTile Tile)>();
      for (int x = 40; x <= 44; x++)
      {
        tiles.Add((new TileCoordinate(x, 40), WorldTile.Empty with
        {
          Active = x is 40 or 44,
          Type = x == 40 ? (ushort)136 : x == 44 ? testCase.Type : (ushort)0,
          FrameX = x == 44 ? testCase.InitialFrame : (short)0,
          FrameY = x == 44 && !facts.FrameImportant[testCase.Type] ? (short)-1 : (short)0,
          Wires = wires
        }));
      }
      if (testCase.Size == 3)
      {
        tiles.Add((new TileCoordinate(43, 39), WorldTile.Empty with
          { Type = 35, FrameX = 36, FrameY = 18, Wall = 1, WallColor = 2 }));
        tiles.Add((new TileCoordinate(45, 41), WorldTile.Empty with
          { Active = true, Type = 35, FrameX = 18, FrameY = 36, TileColor = 3 }));
      }
      var owner = new WorldOwner(facts);
      WorldPublicationResult publication = owner.CommitPreparedWorld(
        owner.PrepareWorld(CreateFixture(facts, tiles)));
      Require(publication.Published,
        $"SC071 single-tile type-{testCase.Type} fixture failed: {publication.Rejection}");
      WorldSnapshot before = owner.CaptureSnapshot();
      int offset = (testCase.Size - 1) / 2;
      var bounds = new WorldSectionBounds(device.X - offset, device.Y - offset,
        testCase.Size, testCase.Size);
      var propagation = new WiringPropagationService(factsCatalog);
      WiringPropagationPlan plan = propagation.Plan(before, request);
      Require(plan.Accepted && plan.TileSquareEffects.Count == colorCount &&
        plan.TileSquareUpdates.Count == 1 && plan.TileSquareUpdates[0] == bounds,
        $"SC071 single-tile type-{testCase.Type} lost refresh multiplicity/bounds: " +
          plan.Rejection);
      WorldTile finalDevice = before.GetTile(device) with
        { Type = testCase.Types[colorCount - 1], FrameX = testCase.Frames[colorCount - 1] };
      Require(plan.Deltas.Count == (finalDevice == before.GetTile(device) ? 1 : 2) &&
        plan.Deltas.ContainsKey(origin) && owner.CaptureSnapshot().Version == before.Version &&
        owner.CaptureSnapshot().GetTile(device) == before.GetTile(device),
        "SC071 single-tile plan lost cancellation semantics or mutated the world.");

      var envelope = new InboundPacketEnvelope(sender, new PacketSequence(1),
        DateTimeOffset.UnixEpoch, HitSwitchPacketHandler.MessageId, null, rawFrame, "Active",
        checked((long)before.WorldEpoch), null, null);
      var producer = new MechanismPacketProducer(wireProfile, facts);
      var handler = new HitSwitchPacketHandler(
        new MechanismWorldPort(owner), propagation, producer);
      HitSwitchDispatchResult dispatch = handler.Handle(envelope, 0, request,
        [sender, observer, sender]);
      Require(dispatch.Status == HitSwitchStatus.Applied && dispatch.CommitVersion is not null &&
        dispatch.OutboundIntents.Count == 2 + colorCount &&
        dispatch.OutboundIntents[0].MessageId == 59 &&
        dispatch.OutboundIntents[0].IgnoreEpoch == sender,
        $"SC071 single-tile type-{testCase.Type} lost relay or per-hit refreshes: " +
          dispatch.Rejection);
      Require(dispatch.OutboundIntents.All(intent =>
        intent.CommitVersion == dispatch.CommitVersion && intent.TargetEpochs.Count == 2 &&
        intent.TargetEpochs.Contains(sender) && intent.TargetEpochs.Contains(observer)) &&
        dispatch.OutboundIntents.Skip(1).All(intent =>
          intent.MessageId == 20 && intent.IgnoreEpoch is null),
        "SC071 single-tile outputs lost their common commit/recipients or added animation.");
      var trigger = (AreaTileChangePacket)wireProfile.Decode(WireDirection.ServerToClient,
        20, dispatch.OutboundIntents[1].Payload);
      Require(trigger.StartX == 40 && trigger.StartY == 40 && trigger.Width == 1 &&
        trigger.Height == 1 && trigger.Tiles.Length == 1 && trigger.Tiles[0].FrameY == 18,
        "SC071 single-tile outputs lost the separate switch refresh.");
      for (int hit = 0; hit < colorCount; hit++)
      {
        WiringTileSquareEffect effect = plan.TileSquareEffects[hit];
        Require(effect.Bounds == bounds && effect.AnimationType is null &&
          effect.AnimationSendCount == 0 && effect.Tiles.Count == testCase.Size * testCase.Size,
          "SC071 single-tile effect lost its bounds or added an animation.");
        var square = (AreaTileChangePacket)wireProfile.Decode(WireDirection.ServerToClient,
          20, dispatch.OutboundIntents[2 + hit].Payload);
        Require(square.StartX == bounds.OriginX && square.StartY == bounds.OriginY &&
          square.Width == bounds.Width && square.Height == bounds.Height &&
          square.ChangeType == 0 && square.Tiles.Length == testCase.Size * testCase.Size,
          "SC071 single-tile packet 20 lost its complete refresh footprint.");
        for (int column = 0; column < testCase.Size; column++)
        {
          for (int row = 0; row < testCase.Size; row++)
          {
            var coordinate = new TileCoordinate(bounds.OriginX + column, bounds.OriginY + row);
            WorldTile expected = coordinate == device
              ? before.GetTile(device) with
                { Type = testCase.Types[hit], FrameX = testCase.Frames[hit] }
              : before.GetTile(coordinate);
            Require(effect.Tiles[coordinate] == expected,
              $"SC071 single-tile type-{testCase.Type} lost hit {hit} state at {coordinate}.");
            AssertDecodedTileMatches(facts, square.Tiles[column * testCase.Size + row],
              expected, coordinate);
          }
        }
      }
      WorldSnapshot after = owner.CaptureSnapshot();
      Require(after.Version == dispatch.CommitVersion &&
        after.Version.Value == before.Version.Value + 1 && after.GetTile(device) == finalDevice &&
        after.GetTile(origin) == (before.GetTile(origin) with { FrameY = 18 }),
        "SC071 single-tile handler did not publish exactly one consistent P03 commit.");
      foreach (TileCoordinate coordinate in plan.TileSquareEffects[^1].Tiles.Keys)
      {
        Require(after.GetTile(coordinate) == (coordinate == device
          ? finalDevice : before.GetTile(coordinate)),
          "SC071 a single-tile device modified a refresh-only neighbor.");
      }
      if (testCase.Type == 33 && testCase.InitialFrame == 0 && colorCount == 4)
      {
        tiles.Add((new TileCoordinate(45, 40), WorldTile.Empty with
          { Active = true, Type = 144, Wires = wires }));
        var rejectedOwner = new WorldOwner(facts);
        Require(rejectedOwner.CommitPreparedWorld(rejectedOwner.PrepareWorld(
          CreateFixture(facts, tiles))).Published,
          "SC071 candle rejection fixture failed.");
        WorldSnapshot rejectedBefore = rejectedOwner.CaptureSnapshot();
        var rejectedHandler = new HitSwitchPacketHandler(
          new MechanismWorldPort(rejectedOwner), propagation, producer);
        HitSwitchDispatchResult rejected = rejectedHandler.Handle(envelope, 0, request,
          [sender, observer]);
        WorldSnapshot rejectedAfter = rejectedOwner.CaptureSnapshot();
        Require(rejected.Status == HitSwitchStatus.InvalidMechanism &&
          rejected.Rejection?.Contains("Timer 144", StringComparison.Ordinal) == true &&
          rejected.OutboundIntents.Count == 0 && rejectedAfter.Version == rejectedBefore.Version &&
          rejectedAfter.GetTile(device) == rejectedBefore.GetTile(device) &&
          rejectedAfter.GetTile(origin) == rejectedBefore.GetTile(origin),
          "SC071 a later timer rejection leaked the candle frame, 3x3 refresh or switch state.");
      }
    }
    evidence.Add($"Tile {testCase.Type}, initial frameX {testCase.InitialFrame}: single-color " +
      $"and four-color hits preserved each {testCase.Size}x{testCase.Size} packet-20 snapshot " +
      $"in red/blue/green/yellow order; expected types [{string.Join(",", testCase.Types)}], " +
      $"frames [{string.Join(",", testCase.Frames)}]; " +
      "one P03 commit, two recipients, no packet77.");
  }
  evidence.Add("Single-tile devices: 68 single/four-color cases passed; candle refresh-only " +
    "neighbors were preserved, and a later timer-144 rejection published no frames or intents.");
  return evidence.AsReadOnly();
}

static string VerifyTimerTransactions(
  WorldFacts facts,
  MechanismFactsCatalog factsCatalog,
  Version4WireProfile wireProfile)
{
  const TileWireMask allColors = TileWireMask.Red | TileWireMask.Blue |
    TileWireMask.Green | TileWireMask.Yellow;
  var origin = new TileCoordinate(40, 40);
  var device = new TileCoordinate(44, 40);
  var timerAnchor = new Terraria.WorldStorage.TileCoordinate(40, 40);
  var request = new HitSwitchPacket { X = 40, Y = 40 };
  SessionEpoch sender = SessionEpoch.Create(7144);
  SessionEpoch observer = SessionEpoch.Create(7145);
  SessionEpoch[] recipients = [sender, observer, sender];
  var wiring = new WiringPropagationService(factsCatalog);
  var producer = new MechanismPacketProducer(wireProfile, facts);
  byte[] rawFrame = PacketFrameCodec.Encode(59,
    wireProfile.Encode(WireDirection.ClientToServer, request).Payload.ToArray());
  int cases = 0;
  foreach ((short style, int period) in new (short, int)[]
    { (0, 60), (1, 180), (2, 300), (3, 30), (4, 15) })
  {
    foreach (TileWireMask colors in new[] { TileWireMask.Red, allColors })
    {
      WorldOwner owner = CreateTimerWorld(style, colors);
      long scope = checked((long)owner.CaptureSnapshot().WorldEpoch);
      var cooldowns = new MechanismCooldownService(scope);
      var world = new MechanismWorldPort(owner);
      var timers = new MechanismTimerService(world, cooldowns, wiring, producer);
      HitSwitchDispatchResult enabled = Toggle(owner, cooldowns);
      Require(enabled.Status == HitSwitchStatus.Applied && enabled.VisitedTiles.Count == 0 &&
        enabled.OutboundIntents.Select(intent => intent.MessageId)
          .SequenceEqual(new byte[] { 59, 20 }) &&
        owner.CaptureSnapshot().GetTile(origin).FrameY == 18 &&
        owner.CaptureSnapshot().GetTile(device).Type == 421 &&
        Remaining(cooldowns) == 18000,
        "SC071 timer activation pulsed wires immediately or lost its 18000-tick registration.");
      var enabledTile = Decode(enabled.OutboundIntents[1]);
      Require(enabled.OutboundIntents[0].IgnoreEpoch == sender &&
        enabled.OutboundIntents.All(intent => intent.TargetEpochs.Count == 2 &&
          intent.CommitVersion == enabled.CommitVersion) &&
        enabledTile.StartX == 40 && enabledTile.StartY == 40 &&
        enabledTile.Width == 1 && enabledTile.Height == 1 && enabledTile.Tiles[0].FrameY == 18,
        "SC071 timer activation lost its real 59/20 encoding or recipients.");
      for (int tick = 0; tick < period; tick++)
      {
        WorldSnapshot before = owner.CaptureSnapshot();
        int remaining = Remaining(cooldowns);
        MechanismTimerService.TickPreparation pending = Prepare(timers, tick);
        Require(pending.Validate() && Remaining(cooldowns) == remaining &&
          owner.CaptureSnapshot().Version == before.Version &&
          owner.CaptureSnapshot().GetTile(device) == before.GetTile(device),
          "SC071 timer preparation wrote the live countdown or world.");
        int hits = tick == period - 1 ? colors == allColors ? 4 : 1 : 0;
        Require(pending.Result.OutboundIntents.Count == hits &&
          pending.Result.VisitedTiles.Count == (hits == 0 ? 0 : 5),
          $"SC071 timer style {style} pulsed at the wrong tick {tick}.");
        for (int hit = 0; hit < hits; hit++)
        {
          OutboundPacketIntent intent = pending.Result.OutboundIntents[hit];
          AreaTileChangePacket square = Decode(intent);
          Require(intent.MessageId == 20 && intent.IgnoreEpoch is null &&
            intent.TargetEpochs.Count == 2 && intent.TargetEpochs.Contains(sender) &&
            intent.TargetEpochs.Contains(observer) &&
            intent.CommitVersion == pending.Result.CommitVersion &&
            square.StartX == 44 && square.StartY == 40 && square.Width == 1 &&
            square.Height == 1 && square.Tiles.Length == 1 &&
            square.Tiles[0].TileType == (hit % 2 == 0 ? 422 : 421),
            "SC071 timer pulse lost ordered intermediate packet20 states.");
        }
        pending.Commit();
        Require(!pending.Validate() &&
          Remaining(cooldowns) == (hits == 0 ? remaining - 1 : 18000) &&
          owner.CaptureSnapshot().GetTile(device).Type ==
            (hits == 1 ? (ushort)422 : (ushort)421),
          "SC071 committed timer did not reset its countdown or consume its preparation.");
        Require(!timers.TryPrepareTick(tick, recipients, out _, out _),
          "SC071 timer replayed an already committed tick.");
      }
      HitSwitchDispatchResult disabled = Toggle(owner, cooldowns);
      Require(disabled.Status == HitSwitchStatus.Applied && Remaining(cooldowns) == 18000 &&
        owner.CaptureSnapshot().GetTile(origin).FrameY == 0,
        "SC071 timer disable removed its registration before UpdateMech.");
      MechanismTimerService.TickPreparation cleanup = Prepare(timers, period);
      Require(cleanup.Result.OutboundIntents.Count == 1 && cooldowns.Count == 1 &&
        Decode(cleanup.Result.OutboundIntents[0]).Tiles[0].FrameY == 0,
        "SC071 disabled timer lost its due 1x1 refresh.");
      cleanup.Commit();
      Require(cooldowns.Count == 0, "SC071 disabled timer was not removed at the next tick.");
      cases++;
    }
  }

  WorldOwner staleOwner = CreateTimerWorld(0, TileWireMask.Red);
  long staleScope = checked((long)staleOwner.CaptureSnapshot().WorldEpoch);
  var staleCooldowns = new MechanismCooldownService(staleScope);
  Require(Toggle(staleOwner, staleCooldowns).Status == HitSwitchStatus.Applied,
    "SC071 stale timer activation failed.");
  staleCooldowns.TrySetRemainingTicks(staleScope, timerAnchor, 1);
  var staleWorld = new MechanismWorldPort(staleOwner);
  var staleTimers = new MechanismTimerService(staleWorld, staleCooldowns, wiring, producer);
  MechanismTimerService.TickPreparation worldConflict = Prepare(staleTimers, 100);
  staleWorld.ApplyTileDeltas((ulong)staleScope,
    [new(new TileCoordinate(10, 10), new WorldTileDelta(Wall: 1))], true);
  RejectCommit(worldConflict);
  Require(Remaining(staleCooldowns) == 1 &&
    staleOwner.CaptureSnapshot().GetTile(device).Type == 421,
    "SC071 stale world timer leaked its countdown or earlier pulse.");
  MechanismTimerService.TickPreparation cooldownConflict = Prepare(staleTimers, 100);
  staleCooldowns.TrySetDeviceCooldown(staleScope, MechanismCooldownService.DeviceKind.Cannon, 8);
  RejectCommit(cooldownConflict);
  Require(Remaining(staleCooldowns) == 1 &&
    staleOwner.CaptureSnapshot().GetTile(device).Type == 421,
    "SC071 stale cannon cooldown allowed a timer world commit.");
  Prepare(staleTimers, 100).Commit();
  Require(Remaining(staleCooldowns) == 18000 &&
    staleCooldowns.TryGetDeviceCooldown(staleScope,
      MechanismCooldownService.DeviceKind.Cannon, out int cannon) && cannon == 7 &&
    staleOwner.CaptureSnapshot().GetTile(device).Type == 422,
    "SC071 failed preparation consumed tick100 or decremented cannon more than once.");

  WorldOwner rejectedOwner = CreateTimerWorld(0, TileWireMask.Red);
  long rejectedScope = checked((long)rejectedOwner.CaptureSnapshot().WorldEpoch);
  var rejectedCooldowns = new MechanismCooldownService(rejectedScope);
  Toggle(rejectedOwner, rejectedCooldowns);
  rejectedCooldowns.TrySetRemainingTicks(rejectedScope, timerAnchor, 1);
  var rejectedWorld = new MechanismWorldPort(rejectedOwner);
  var unsupported = new TileCoordinate(45, 40);
  rejectedWorld.ApplyTileDeltas((ulong)rejectedScope,
    [new(unsupported, new WorldTileDelta(Active: true, Type: 144, Wires: TileWireMask.Red))], true);
  WorldSnapshot rejectionBefore = rejectedOwner.CaptureSnapshot();
  var rejectedTimers = new MechanismTimerService(
    rejectedWorld, rejectedCooldowns, wiring, producer);
  Require(!rejectedTimers.TryPrepareTick(0, recipients, out var rejected, out string? reason) &&
    rejected is null && reason?.Contains("Timer 144", StringComparison.Ordinal) == true &&
    Remaining(rejectedCooldowns) == 1 &&
    rejectedOwner.CaptureSnapshot().Version == rejectionBefore.Version &&
    rejectedOwner.CaptureSnapshot().GetTile(device).Type == 421,
    "SC071 later wired144 rejection leaked a previously planned pulse or countdown.");
  rejectedWorld.ApplyTileDeltas((ulong)rejectedScope,
    [new(unsupported, new WorldTileDelta(Active: false))], true);
  Prepare(rejectedTimers, 0).Commit();
  Require(Remaining(rejectedCooldowns) == 18000 &&
    rejectedOwner.CaptureSnapshot().GetTile(device).Type == 422,
    "SC071 timer could not retry the same tick after its unsupported device was removed.");

  WorldOwner fullOwner = CreateTimerWorld(0, TileWireMask.Red);
  long fullScope = checked((long)fullOwner.CaptureSnapshot().WorldEpoch);
  var fullCooldowns = new MechanismCooldownService(fullScope);
  for (int index = 0; index < MechanismCooldownService.MaximumScheduledMechanisms; index++)
  {
    fullCooldowns.Schedule(fullScope,
      new Terraria.WorldStorage.TileCoordinate(index % 70 + 1, index / 70 + 1), 20);
  }
  Require(Toggle(fullOwner, fullCooldowns).Status == HitSwitchStatus.Applied &&
    fullOwner.CaptureSnapshot().GetTile(origin).FrameY == 18 && fullCooldowns.Count == 999 &&
    !fullCooldowns.TryGetRemainingTicks(fullScope, timerAnchor, out _),
    "SC071 a full CheckMech table changed direct timer switch semantics.");
  fullCooldowns.Clear(fullScope);
  fullCooldowns.Schedule(fullScope, timerAnchor, 123);
  Toggle(fullOwner, fullCooldowns);
  Toggle(fullOwner, fullCooldowns);
  Require(Remaining(fullCooldowns) == 123 && fullCooldowns.Count == 1 &&
    fullOwner.CaptureSnapshot().GetTile(origin).FrameY == 18,
    "SC071 duplicate timer activation overwrote the existing CheckMech countdown.");
  MechanismCooldownService.Preparation scheduled = fullCooldowns.Prepare();
  var isolatedAnchor = new Terraria.WorldStorage.TileCoordinate(20, 20);
  Require(scheduled.Schedule(isolatedAnchor, 15) ==
    MechanismCooldownService.ScheduleResult.Scheduled && fullCooldowns.Count == 1,
    "SC071 cooldown preparation exposed its tentative scheduled entry.");
  scheduled.Commit();
  bool consumedRejected = false;
  try
  {
    scheduled.Schedule(new Terraria.WorldStorage.TileCoordinate(21, 20), 15);
  }
  catch (InvalidOperationException)
  {
    consumedRejected = true;
  }
  Require(consumedRejected && !scheduled.Validate() && fullCooldowns.Count == 2,
    "SC071 a consumed cooldown preparation mutated the published candidate collections.");

  WorldOwner sharedOwner = CreateTimerWorld(0, TileWireMask.Red);
  var sharedWorld = new MechanismWorldPort(sharedOwner);
  long sharedScope = checked((long)sharedOwner.CaptureSnapshot().WorldEpoch);
  var secondTimer = new TileCoordinate(44, 36);
  var secondDevice = new TileCoordinate(44, 38);
  var sharedPatches = new List<KeyValuePair<TileCoordinate, WorldTileDelta>>();
  for (int y = 36; y <= 40; y++)
  {
    sharedPatches.Add(new(new TileCoordinate(44, y), new WorldTileDelta(
      Wires: y == 40 ? TileWireMask.Red | TileWireMask.Blue : TileWireMask.Blue)));
  }
  sharedPatches.Add(new(secondTimer,
    new WorldTileDelta(Active: true, Type: 144, FrameX: 0, FrameY: 18)));
  sharedPatches.Add(new(secondDevice, new WorldTileDelta(Active: true, Type: 421)));
  sharedWorld.ApplyTileDeltas((ulong)sharedScope, sharedPatches, true);
  var sharedCooldowns = new MechanismCooldownService(sharedScope);
  Toggle(sharedOwner, sharedCooldowns);
  sharedCooldowns.TrySetRemainingTicks(sharedScope, timerAnchor, 1);
  sharedCooldowns.Schedule(sharedScope, new Terraria.WorldStorage.TileCoordinate(44, 36), 1);
  var sharedTimers = new MechanismTimerService(sharedWorld, sharedCooldowns, wiring, producer);
  MechanismTimerService.TickPreparation shared = Prepare(sharedTimers, 0);
  AreaTileChangePacket[] sharedSquares = shared.Result.OutboundIntents.Select(Decode).ToArray();
  Require(sharedSquares.Select(square => (square.StartX, square.StartY))
      .SequenceEqual(new (short, short)[] { (44, 38), (44, 40), (44, 40) }) &&
    sharedSquares.Select(square => square.Tiles[0].TileType)
      .SequenceEqual(new ushort[] { 422, 422, 421 }) &&
    sharedOwner.CaptureSnapshot().GetTile(secondDevice).Type == 421,
    "SC071 simultaneous timers lost reverse registration or their shared evolving overlay.");
  shared.Commit();
  Require(sharedOwner.CaptureSnapshot().GetTile(device).Type == 421 &&
    sharedOwner.CaptureSnapshot().GetTile(secondDevice).Type == 422,
    "SC071 shared timer device publication disagreed with intermediate packet20 states.");

  WorldOwner resetOwner = CreateTimerWorld(0, TileWireMask.Red);
  var resetWorld = new MechanismWorldPort(resetOwner);
  long resetScope = checked((long)resetOwner.CaptureSnapshot().WorldEpoch);
  resetWorld.ApplyTileDeltas((ulong)resetScope,
    [new(new TileCoordinate(10, 10), new WorldTileDelta(Active: true, Type: 411))], true);
  var resetCooldowns = new MechanismCooldownService(resetScope);
  resetCooldowns.Schedule(resetScope, new Terraria.WorldStorage.TileCoordinate(10, 10), 1);
  Toggle(resetOwner, resetCooldowns);
  resetCooldowns.TrySetRemainingTicks(resetScope, timerAnchor, 1);
  WorldSnapshot resetBefore = resetOwner.CaptureSnapshot();
  var resetTimers = new MechanismTimerService(resetWorld, resetCooldowns, wiring, producer);
  Require(!resetTimers.TryPrepareTick(0, recipients, out _, out string? resetReason) &&
    resetReason?.Contains("411", StringComparison.Ordinal) == true &&
    Remaining(resetCooldowns) == 1 && resetCooldowns.Count == 2 &&
    resetOwner.CaptureSnapshot().Version == resetBefore.Version &&
    resetOwner.CaptureSnapshot().GetTile(device).Type == 421,
    "SC071 later due411 rejection published the preceding timer pulse.");

  return $"{cases} cadence cases: five styles x one/four colors; direct 59/20 has no pulse; " +
    "prepared countdown/world/output is atomic; disable cleans up next tick; stale world/cannon " +
    "and later wired144/due411 reject without writes; same-tick retry, 999-entry capacity, " +
    "dedup registration, reverse order and shared intermediate packet20 states passed.";

  WorldOwner CreateTimerWorld(short style, TileWireMask colors)
  {
    var tiles = new List<(TileCoordinate, WorldTile)>();
    for (int x = 40; x <= 44; x++)
    {
      tiles.Add((new TileCoordinate(x, 40), WorldTile.Empty with { Wires = colors }));
    }
    tiles.Add((origin, WorldTile.Empty with
      { Active = true, Type = 144, FrameX = (short)(style * 18), FrameY = 0, Wires = colors }));
    tiles.Add((device, WorldTile.Empty with
      { Active = true, Type = 421, FrameX = -1, FrameY = -1, Wires = colors }));
    var owner = new WorldOwner(facts);
    Require(owner.CommitPreparedWorld(owner.PrepareWorld(CreateFixture(facts, tiles))).Published,
      "SC071 timer fixture publication failed.");
    return owner;
  }

  HitSwitchDispatchResult Toggle(WorldOwner owner, MechanismCooldownService cooldowns)
  {
    var envelope = new InboundPacketEnvelope(sender, new PacketSequence(1),
      DateTimeOffset.UnixEpoch, 59, null, rawFrame, "Active",
      checked((long)owner.CaptureSnapshot().WorldEpoch), null, null);
    return new HitSwitchPacketHandler(new MechanismWorldPort(owner), wiring, producer, cooldowns)
      .Handle(envelope, 0, request, recipients);
  }

  MechanismTimerService.TickPreparation Prepare(MechanismTimerService timers, long tick)
  {
    Require(timers.TryPrepareTick(tick, recipients, out var pending, out string? reason),
      $"SC071 timer preparation failed: {reason}");
    return pending ?? throw new InvalidOperationException("SC071 missing timer preparation.");
  }

  int Remaining(MechanismCooldownService cooldowns)
  {
    Require(cooldowns.TryGetRemainingTicks(cooldowns.WorldScope, timerAnchor, out int ticks),
      "SC071 timer registration disappeared.");
    return ticks;
  }

  AreaTileChangePacket Decode(OutboundPacketIntent intent)
  {
    return (AreaTileChangePacket)wireProfile.Decode(
      WireDirection.ServerToClient, 20, intent.Payload);
  }

  void RejectCommit(MechanismTimerService.TickPreparation pending)
  {
    bool rejected = false;
    try
    {
      pending.Commit();
    }
    catch (InvalidOperationException)
    {
      rejected = true;
    }
    Require(rejected && !pending.Validate(), "SC071 stale timer preparation was committed.");
  }
}

static string VerifyPixelBoxPackets(
  WorldFacts facts,
  MechanismFactsCatalog factsCatalog,
  Version4WireProfile wireProfile)
{
  const TileWireMask allColors = TileWireMask.Red | TileWireMask.Blue |
    TileWireMask.Green | TileWireMask.Yellow;
  var origin = new TileCoordinate(40, 40);
  var pixel = new TileCoordinate(44, 40);
  var request = new HitSwitchPacket { X = 40, Y = 40 };
  SessionEpoch sender = SessionEpoch.Create(7445);
  SessionEpoch observer = SessionEpoch.Create(7446);
  byte[] rawFrame = PacketFrameCodec.Encode(HitSwitchPacketHandler.MessageId,
    wireProfile.Encode(WireDirection.ClientToServer, request).Payload.ToArray());
  var propagation = new WiringPropagationService(factsCatalog);
  var producer = new MechanismPacketProducer(wireProfile, facts);
  int caseCount = 0;
  foreach ((TileWireMask horizontal, TileWireMask vertical) in
    new (TileWireMask, TileWireMask)[]
    {
      (TileWireMask.Red, TileWireMask.None),
      (TileWireMask.None, TileWireMask.Blue),
      (TileWireMask.Red, TileWireMask.Red),
      (TileWireMask.Red, TileWireMask.Blue),
      (allColors, allColors)
    })
  {
    foreach (short initialFrame in new short[] { 0, 18, 36 })
    {
      var tiles = new Dictionary<TileCoordinate, WorldTile>();
      // Remove the base fixture's wire/device before drawing these independent paths.
      for (int x = 40; x <= 44; x++)
      {
        tiles[new TileCoordinate(x, 40)] = WorldTile.Empty;
      }
      void AddWire(int x, int y, TileWireMask color)
      {
        var coordinate = new TileCoordinate(x, y);
        WorldTile tile = tiles.GetValueOrDefault(coordinate, WorldTile.Empty);
        tiles[coordinate] = tile with { Wires = tile.Wires | color };
      }
      for (int x = 40; x <= 44; x++)
      {
        AddWire(x, 40, horizontal);
        AddWire(x, 43, vertical);
      }
      for (int y = 40; y <= 43; y++)
      {
        AddWire(40, y, vertical);
        AddWire(44, y, vertical);
      }
      // All-color exit stubs expose an incorrect turn even in single-axis cases.
      AddWire(45, 40, allColors);
      AddWire(44, 39, allColors);
      tiles[origin] = tiles[origin] with { Active = true, Type = 136 };
      tiles[pixel] = tiles[pixel] with
        { Active = true, Type = 445, FrameX = initialFrame };
      var owner = new WorldOwner(facts);
      WorldPreparationRequest fixture = CreateFixture(facts,
        tiles.Select(pair => (pair.Key, pair.Value)).ToArray());
      WorldPublicationResult publication = owner.CommitPreparedWorld(owner.PrepareWorld(fixture));
      Require(publication.Published,
        $"SC071 pixel-box fixture failed: {publication.Rejection}");
      WorldSnapshot before = owner.CaptureSnapshot();
      bool crossed = horizontal != TileWireMask.None && vertical != TileWireMask.None;
      short finalFrame = crossed ? initialFrame == 18 ? (short)0 : (short)18 : initialFrame;
      WiringPropagationPlan plan = propagation.Plan(before, request);
      var bounds = new WorldSectionBounds(pixel.X, pixel.Y, 1, 1);
      WorldTile expectedPixel = before.GetTile(pixel) with { FrameX = finalFrame };
      Require(plan.Accepted && plan.VisitedTiles.Contains(pixel) &&
        plan.VisitedTiles.Contains(new TileCoordinate(45, 40)) ==
          (horizontal != TileWireMask.None) &&
        plan.VisitedTiles.Contains(new TileCoordinate(44, 39)) ==
          (vertical != TileWireMask.None) &&
        plan.TileSquareEffects.Count == (crossed ? 1 : 0) &&
        plan.Deltas.Count == (crossed ? 2 : 1),
        $"SC071 pixel-box routing or end-of-trip aggregation failed: {plan.Rejection}");
      if (crossed)
      {
        WiringTileSquareEffect effect = plan.TileSquareEffects[0];
        Require(effect.Bounds == bounds && effect.Tiles.Count == 1 &&
          effect.Tiles[pixel] == expectedPixel && effect.AnimationType is null &&
          effect.AnimationSendCount == 0,
          "SC071 pixel box did not retain one final 1x1 snapshot after all colors.");
      }
      Require(owner.CaptureSnapshot().Version == before.Version &&
        owner.CaptureSnapshot().GetTile(pixel) == before.GetTile(pixel),
        "SC071 pixel-box planning mutated the world.");
      var envelope = new InboundPacketEnvelope(sender, new PacketSequence(1),
        DateTimeOffset.UnixEpoch, HitSwitchPacketHandler.MessageId, null, rawFrame, "Active",
        checked((long)before.WorldEpoch), null, null);
      var handler = new HitSwitchPacketHandler(new MechanismWorldPort(owner),
        propagation, producer);
      HitSwitchDispatchResult dispatch = handler.Handle(envelope, 0, request,
        [sender, observer, sender]);
      Require(dispatch.Status == HitSwitchStatus.Applied && dispatch.CommitVersion is not null &&
        dispatch.OutboundIntents.Count == (crossed ? 3 : 2) &&
        dispatch.OutboundIntents[0].MessageId == 59 &&
        dispatch.OutboundIntents[0].IgnoreEpoch == sender &&
        dispatch.OutboundIntents.Skip(1).All(intent => intent.MessageId == 20 &&
          intent.IgnoreEpoch is null) &&
        dispatch.OutboundIntents.All(intent => intent.CommitVersion == dispatch.CommitVersion &&
          intent.TargetEpochs.Count == 2 && intent.TargetEpochs.Contains(sender) &&
          intent.TargetEpochs.Contains(observer)),
        $"SC071 pixel-box handler lost exact outputs or recipients: {dispatch.Rejection}");
      var switchSquare = (AreaTileChangePacket)wireProfile.Decode(WireDirection.ServerToClient,
        20, dispatch.OutboundIntents[1].Payload);
      Require(switchSquare.StartX == 40 && switchSquare.StartY == 40 &&
        switchSquare.Width == 1 && switchSquare.Height == 1 &&
        switchSquare.Tiles.Length == 1 && switchSquare.Tiles[0].FrameY == 18,
        "SC071 pixel-box output lost its preceding separate switch refresh.");
      if (crossed)
      {
        var square = (AreaTileChangePacket)wireProfile.Decode(WireDirection.ServerToClient,
          20, dispatch.OutboundIntents[2].Payload);
        Require(square.StartX == pixel.X && square.StartY == pixel.Y && square.Width == 1 &&
          square.Height == 1 && square.ChangeType == 0 && square.Tiles.Length == 1,
          "SC071 pixel-box packet20 lost its exact 1x1 bounds.");
        AssertDecodedTileMatches(facts, square.Tiles[0], expectedPixel, pixel);
      }
      WorldSnapshot after = owner.CaptureSnapshot();
      Require(after.Version == dispatch.CommitVersion &&
        after.Version.Value == before.Version.Value + 1 &&
        after.GetTile(pixel) == expectedPixel &&
        after.GetTile(origin) == (before.GetTile(origin) with { FrameY = 18 }),
        "SC071 pixel-box publication did not match its final refresh and single commit.");
      foreach (TileCoordinate coordinate in tiles.Keys.Where(key => key != origin && key != pixel))
      {
        Require(after.GetTile(coordinate) == before.GetTile(coordinate),
          "SC071 pixel-box propagation modified a wire-only tile.");
      }
      if (horizontal == allColors && initialFrame == 0)
      {
        var timer = new TileCoordinate(45, 40);
        tiles[timer] = tiles[timer] with { Active = true, Type = 144 };
        var rejectedOwner = new WorldOwner(facts);
        WorldPublicationResult rejectedPublication = rejectedOwner.CommitPreparedWorld(
          rejectedOwner.PrepareWorld(CreateFixture(facts,
            tiles.Select(pair => (pair.Key, pair.Value)).ToArray())));
        Require(rejectedPublication.Published,
          $"SC071 pixel-box rejection fixture failed: {rejectedPublication.Rejection}");
        WorldSnapshot rejectedBefore = rejectedOwner.CaptureSnapshot();
        var rejectedHandler = new HitSwitchPacketHandler(new MechanismWorldPort(rejectedOwner),
          propagation, producer);
        HitSwitchDispatchResult rejected = rejectedHandler.Handle(envelope, 0, request,
          [sender, observer]);
        WorldSnapshot rejectedAfter = rejectedOwner.CaptureSnapshot();
        Require(rejected.Status == HitSwitchStatus.InvalidMechanism &&
          rejected.Rejection?.Contains("Timer 144", StringComparison.Ordinal) == true &&
          rejected.OutboundIntents.Count == 0 && rejectedAfter.Version == rejectedBefore.Version &&
          rejectedAfter.GetTile(pixel) == rejectedBefore.GetTile(pixel) &&
          rejectedAfter.GetTile(origin) == rejectedBefore.GetTile(origin),
          "SC071 a later rejection leaked the accumulated pixel-box trigger or switch frame.");
      }
      caseCount++;
    }
  }
  return $"{caseCount} cases passed: horizontal-only and vertical-only do not turn or toggle; " +
    "same-color, red/blue and four-color crossings aggregate into one final 1x1 packet20; " +
    "initial frames 0/18/36 follow the source branch; one P03 commit, exact recipients and " +
    "later timer144 rejection atomicity preserved.";
}

static IReadOnlyList<string> VerifyAnimatedDevicePackets(
  WorldFacts facts,
  MechanismFactsCatalog factsCatalog,
  Version4WireProfile wireProfile)
{
  const TileWireMask allColors = TileWireMask.Red | TileWireMask.Blue |
    TileWireMask.Green | TileWireMask.Yellow;
  var evidence = new List<string>();
  foreach (ushort tileType in new ushort[] { 349, 593, 594 })
  {
    int width = tileType == 593 ? 1 : 2;
    int height = tileType == 349 ? 3 : width;
    int frameStep = tileType == 349 ? 216 : width * 18;
    int animationSendCount = tileType == 349 ? 1 : 2;
    int intentsPerHit = 1 + animationSendCount;
    var anchor = new TileCoordinate(45 - width, 41 - height);
    var cases = new List<(int ColorCount, int InitialFrame, bool Mixed)>
      { (1, 0, false), (1, frameStep, false), (4, 0, false) };
    if (tileType is 349 or 594)
    {
      cases.Add((1, 0, true));
    }
    foreach ((int colorCount, int initialFrame, bool mixed) in cases)
    {
      TileWireMask wires = colorCount == 4 ? allColors : TileWireMask.Red;
      var additionalTiles = new List<(TileCoordinate Coordinate, WorldTile Tile)>();
      for (int x = 40; x <= 44; x++)
      {
        additionalTiles.Add((new TileCoordinate(x, 40), WorldTile.Empty with
        {
          Active = x == 40,
          Type = x == 40 ? (ushort)136 : (ushort)0,
          Wires = x == 40 ? wires : TileWireMask.None
        }));
        additionalTiles.Add((new TileCoordinate(x, 41), WorldTile.Empty with { Wires = wires }));
      }
      for (int column = 0; column < width; column++)
      {
        for (int row = 0; row < height; row++)
        {
          additionalTiles.Add((new TileCoordinate(anchor.X + column, anchor.Y + row),
            WorldTile.Empty with
            {
              Active = !(mixed && column == 0 && row == 0),
              Type = mixed && row == 0 ? (ushort)35 : tileType,
              FrameX = checked((short)(initialFrame + column * 18)),
              FrameY = checked((short)(row * 18 + (height > 1 ? height * 18 : 0))),
              Wires = wires
            }));
        }
      }

      var owner = new WorldOwner(facts);
      WorldPublicationResult publication = owner.CommitPreparedWorld(
        owner.PrepareWorld(CreateFixture(facts, additionalTiles)));
      Require(publication.Published,
        $"SC071 animated type-{tileType} fixture failed: {publication.Rejection}");
      WorldSnapshot before = owner.CaptureSnapshot();
      var request = new HitSwitchPacket { X = 40, Y = 40 };
      var propagation = new WiringPropagationService(factsCatalog);
      WiringPropagationPlan plan = propagation.Plan(before, request);
      Require(plan.Accepted && plan.TileSquareEffects.Count == colorCount &&
        plan.TileSquareUpdates.Count == 1,
        $"SC071 animated type-{tileType} did not hit once per wire color: {plan.Rejection}");
      Require(owner.CaptureSnapshot().Version == before.Version &&
        owner.CaptureSnapshot().GetTile(anchor) == before.GetTile(anchor),
        "SC071 animated planning mutated the authoritative world.");
      if (colorCount == 4)
      {
        Require(plan.Deltas.Count == 1 && plan.Deltas.ContainsKey(new TileCoordinate(40, 40)),
          "SC071 four-color device should return to its original state with only trigger delta.");
      }

      SessionEpoch sender = SessionEpoch.Create(7593);
      SessionEpoch observer = SessionEpoch.Create(7594);
      byte[] rawFrame = PacketFrameCodec.Encode(HitSwitchPacketHandler.MessageId,
        wireProfile.Encode(WireDirection.ClientToServer, request).Payload.ToArray());
      var envelope = new InboundPacketEnvelope(sender, new PacketSequence(1), DateTimeOffset.UnixEpoch,
        HitSwitchPacketHandler.MessageId, null, rawFrame, "Active",
        checked((long)before.WorldEpoch), null, null);
      var producer = new MechanismPacketProducer(wireProfile, facts);
      var handler = new HitSwitchPacketHandler(new MechanismWorldPort(owner), propagation, producer);
      HitSwitchDispatchResult dispatch = handler.Handle(envelope, 0, request,
        [sender, observer, sender]);
      Require(dispatch.Status == HitSwitchStatus.Applied && dispatch.CommitVersion is not null,
        $"SC071 animated type-{tileType} handler rejected: {dispatch.Rejection}");
      Require(dispatch.OutboundIntents.Count == 2 + colorCount * intentsPerHit &&
        dispatch.OutboundIntents[0].MessageId == 59 &&
        dispatch.OutboundIntents[0].IgnoreEpoch == sender,
        "SC071 animated handler lost trigger relay or device effect multiplicity.");
      Require(dispatch.OutboundIntents.All(intent =>
        intent.CommitVersion == dispatch.CommitVersion && intent.TargetEpochs.Count == 2 &&
        intent.TargetEpochs.Contains(sender) && intent.TargetEpochs.Contains(observer)),
        "SC071 animated outputs lost their common commit version or recipient set.");
      var trigger = (AreaTileChangePacket)wireProfile.Decode(WireDirection.ServerToClient,
        20, dispatch.OutboundIntents[1].Payload);
      Require(dispatch.OutboundIntents[1].MessageId == 20 && trigger.StartX == 40 &&
        trigger.StartY == 40 && trigger.Width == 1 && trigger.Height == 1 &&
        trigger.Tiles[0].FrameY == 18,
        "SC071 animated outputs lost the committed switch frame.");

      int currentFrame = initialFrame;
      for (int hit = 0; hit < colorCount; hit++)
      {
        int delta = currentFrame != 0 ? -frameStep : frameStep;
        currentFrame += delta;
        short animationType = (short)(tileType == 349
          ? delta > 0 ? 0 : 1
          : delta > 0 ? 4 : 3);
        int intentIndex = 2 + hit * intentsPerHit;
        OutboundPacketIntent refresh = dispatch.OutboundIntents[intentIndex];
        Require(refresh.MessageId == 20 && refresh.IgnoreEpoch is null,
          "SC071 animated device must refresh before its animation.");
        var square = (AreaTileChangePacket)wireProfile.Decode(
          WireDirection.ServerToClient, 20, refresh.Payload);
        Require(square.StartX == anchor.X && square.StartY == anchor.Y &&
          square.Width == width && square.Height == height && square.Tiles.Length == width * height,
          $"SC071 type-{tileType} omitted its complete tile square or resolved wrong anchor.");
        WiringTileSquareEffect effect = plan.TileSquareEffects[hit];
        Require(effect.Bounds == new WorldSectionBounds(anchor.X, anchor.Y, width, height) &&
          effect.AnimationType == animationType && effect.AnimationTileType == tileType &&
          effect.AnimationSendCount == animationSendCount,
          "SC071 animated effect lost hit order or type.");
        for (int column = 0; column < width; column++)
        {
          for (int row = 0; row < height; row++)
          {
            Packet20Tile tile = square.Tiles[column * height + row];
            var coordinate = new TileCoordinate(anchor.X + column, anchor.Y + row);
            WorldTile expected = before.GetTile(coordinate) with
              { FrameX = checked((short)(currentFrame + column * 18)) };
            AssertDecodedTileMatches(facts, tile, expected, coordinate);
            Require(effect.Tiles[coordinate] == expected,
              $"SC071 animated type-{tileType} encoded final state instead of hit {hit} snapshot.");
          }
        }
        for (int send = 1; send <= animationSendCount; send++)
        {
          OutboundPacketIntent intent = dispatch.OutboundIntents[intentIndex + send];
          Require(intent.MessageId == 77 && intent.IgnoreEpoch is null && intent.Payload.Length == 8,
            "SC071 animated device lost a source packet-77 send.");
          var animation = (TemporaryAnimationPacket)wireProfile.Decode(
            WireDirection.ServerToClient, 77, intent.Payload);
          Require(animation.AnimationType == animationType && animation.TileType == tileType &&
            animation.X == anchor.X && animation.Y == anchor.Y,
            "SC071 animated packet 77 lost its type, direction or anchor.");
        }
        if (animationSendCount == 2)
        {
          Require(dispatch.OutboundIntents[intentIndex + 1].Payload.Span.SequenceEqual(
            dispatch.OutboundIntents[intentIndex + 2].Payload.Span),
            "SC071 the two source animation sends should be byte-identical.");
        }
      }
      WorldSnapshot after = owner.CaptureSnapshot();
      Require(after.Version == dispatch.CommitVersion && after.Version.Value == before.Version.Value + 1,
        "SC071 animated handler did not publish exactly one P03 commit.");
      for (int column = 0; column < width; column++)
      {
        for (int row = 0; row < height; row++)
        {
          var coordinate = new TileCoordinate(anchor.X + column, anchor.Y + row);
          Require(after.GetTile(coordinate) == (before.GetTile(coordinate) with
            { FrameX = checked((short)(currentFrame + column * 18)) }),
            "SC071 animated world state diverged from the final device refresh.");
        }
      }
      evidence.Add($"Tile {tileType}, {colorCount} color(s), initial frameX {initialFrame}, " +
        $"mixed footprint {mixed}: " +
        $"one P03 commit; {colorCount} ordered complete {width}x{height} refresh(es), each followed " +
        $"by {animationSendCount} packet 77 intent(s) with decoded type/anchor and both recipients; " +
        "per-hit frames retained even when four colors cancel the final tile delta.");
      if (tileType == 593 && colorCount == 1 && initialFrame == 0)
      {
        additionalTiles.Add((new TileCoordinate(45, 40), WorldTile.Empty with
          { Active = true, Type = 144, Wires = TileWireMask.Red }));
        var rejectedOwner = new WorldOwner(facts);
        Require(rejectedOwner.CommitPreparedWorld(rejectedOwner.PrepareWorld(
          CreateFixture(facts, additionalTiles))).Published,
          "SC071 animated rejection fixture publication failed.");
        WorldSnapshot rejectedBefore = rejectedOwner.CaptureSnapshot();
        var rejectedHandler = new HitSwitchPacketHandler(
          new MechanismWorldPort(rejectedOwner), propagation, producer);
        HitSwitchDispatchResult rejected = rejectedHandler.Handle(
          envelope, 0, request, [sender, observer]);
        WorldSnapshot rejectedAfter = rejectedOwner.CaptureSnapshot();
        Require(rejected.Status == HitSwitchStatus.InvalidMechanism &&
          rejected.Rejection?.Contains("Timer 144", StringComparison.Ordinal) == true &&
          rejected.OutboundIntents.Count == 0 && rejectedAfter.Version == rejectedBefore.Version &&
          rejectedAfter.GetTile(anchor) == rejectedBefore.GetTile(anchor) &&
          rejectedAfter.GetTile(new TileCoordinate(40, 40)).FrameY == 0,
          "SC071 a later rejected device leaked animated frames, intents or trigger state.");
        evidence.Add("A later timer-144 rejection discarded the previously planned type-593 " +
          "frame and animation effects without publishing a world commit or any outbound intent.");
      }
    }
  }
  return evidence.AsReadOnly();
}

static string VerifyEmptyRecipientRemovals(
  WorldFacts facts,
  MechanismFactsCatalog factsCatalog,
  Version4WireProfile wireProfile)
{
  const long worldScope = 7071;
  var anchor = new TileCoordinate(30, 30);
  var entityAnchor = new Terraria.WorldStorage.TileCoordinate(30, 30);
  var pylonTile = new WorldTile(
    Active: true,
    Type: 597,
    Wall: 0,
    FrameX: 0,
    FrameY: 0,
    LiquidAmount: 0,
    LiquidType: LiquidType.Water,
    TileColor: 0,
    WallColor: 0,
    Wires: TileWireMask.None,
    Actuator: false,
    Inactive: false);
  var additionalTiles = new (TileCoordinate Coordinate, WorldTile Tile)[]
  {
    (anchor, pylonTile)
  };
  var worldOwner = new WorldOwner(facts);
  WorldPublicationResult publication = worldOwner.CommitPreparedWorld(
    worldOwner.PrepareWorld(CreateFixture(facts, additionalTiles)));
  Require(publication.Published,
    $"SC071 empty-recipient removal fixture publication failed: {publication.Rejection}");
  WorldSnapshot before = worldOwner.CaptureSnapshot();
  var identityAuthority = new ScenarioMechanismIdentityAuthority(worldScope);
  MechanismEntityHandle handle = identityAuthority.Create();
  var entities = new TileEntityRegistry(factsCatalog, identityAuthority, worldScope);
  var anchorTile = new MechanismTileSnapshot(true, 597, 0, 0);
  TileEntityRuntimeState entity = entities.Create(
    handle,
    new TileEntityCreateRequest(
      Type: 7,
      Anchor: entityAnchor,
      Section: new MechanismSectionCoordinate(0, 0),
      AnchorTile: anchorTile,
      Items: []));
  var pylons = new TeleportPylonRegistry(factsCatalog, worldScope);
  Require(pylons.TryRegister(entity, anchorTile, out TeleportPylonSnapshot? registeredPylon) &&
    registeredPylon is not null,
    "SC071 empty-recipient removal fixture did not register its pylon.");
  var leashedEntities = new LeashedEntityRegistry(
    factsCatalog,
    identityAuthority,
    identityAuthority.Create,
    worldScope);
  var producer = new MechanismPacketProducer(wireProfile, facts);
  var coordinator = new TileEntityRemovalCoordinator(
    new MechanismWorldPort(worldOwner),
    entities,
    new TileEntityProjection(worldScope),
    pylons,
    leashedEntities,
    identityAuthority,
    producer);
  TileEntityRemovalDispatchResult removal = coordinator.Remove(
    handle,
    before,
    [new WorldTilePatch(anchor, Active: false, Type: 0)],
    Array.Empty<SessionEpoch>(),
    before.Version);

  CommitVersion committedVersion = removal.CommitVersion
    ?? throw new InvalidOperationException(
      $"SC071 empty-recipient pylon removal did not return a commit version: {removal.Status} " +
      removal.Detail);
  Require(removal.Status == TileEntityRemovalStatus.Removed,
    $"SC071 empty-recipient pylon removal was not committed: {removal.Status} {removal.Detail}");
  Require(removal.OutboundIntents.Count == 0,
    "SC071 empty-recipient pylon removal emitted an outbound intent.");
  Require(removal.Removal?.State.Lifecycle == MechanismEntityLifecycle.Retired &&
    entities.Count == 0 && !identityAuthority.IsActive(handle),
    "SC071 empty-recipient pylon removal did not retire the TileEntity identity.");
  Require(removal.RemovedPylon == registeredPylon && pylons.Count == 0,
    "SC071 empty-recipient pylon removal did not commit the pylon index removal.");
  Require(leashedEntities.Count == 0,
    "SC071 empty-recipient pylon removal unexpectedly changed the leash registry.");
  WorldSnapshot after = worldOwner.CaptureSnapshot();
  Require(after.Version == committedVersion && !after.GetTile(anchor).Active,
    "SC071 empty-recipient pylon removal did not commit its world tile patch.");

  string kiteEvidence = VerifyEmptyRecipientKiteRemoval(facts, factsCatalog, wireProfile);
  return $"Type-7 pylon: removed and retired the TileEntity at {anchor}, committed its " +
    $"pylon-index removal and world tile patch at version {committedVersion.Value}, and emitted " +
    $"zero packet 86/20 intents. Type-9 Kite: {kiteEvidence}";
}

static string VerifyLeashedSectionTickAtomicity(
  MechanismFactsCatalog factsCatalog,
  Version4WireProfile wireProfile)
{
  const long worldScope = 7073;
  var identityAuthority = new ScenarioMechanismIdentityAuthority(worldScope);
  var registry = new LeashedEntityRegistry(
    factsCatalog,
    identityAuthority,
    identityAuthority.Create,
    worldScope);
  MechanismSectionCoordinate section = new(0, 0);
  for (int index = 0; index < 2; index++)
  {
    short anchorX = checked((short)(10 + index));
    LeashedEntityChangeResult created = registry.ApplyFull(
      new Version4ModulePayloads.LeashedEntityMessage(
        Action: 1,
        Slot: index,
        EntityType: 1,
        AnchorX: anchorX,
        AnchorY: 10,
        State: new Version4ModulePayloads.KiteLeashedEntityState(
          ProjectileType: 100 + index,
          PositionX: 0,
          PositionY: 0,
          PackedVelocity: 0,
          Rotation: 0,
          WindTarget: 0,
          CloudAlpha: 0,
          TimeCounter: 0)),
      section);
    Require(created.Kind == LeashedEntityChangeKind.Created,
      "SC071 leash-tick fixture failed to create a Kite slot.");
  }

  var sections = new LeashedEntitySectionService(registry);
  LeashedEntitySectionService.SectionActivationResult activation = sections.ActivateSection(
    section,
    tick: 0,
    PrepareScenarioKiteSpawn);
  Require(activation.WasActivated && activation.FullEntities.Count == 2,
    "SC071 leash-tick fixture did not activate both Kite slots.");

  long beforeFailureRevision = registry.Revision;
  IReadOnlyList<LeashedEntityRuntimeSnapshot> beforeFailure = registry.CaptureSection(section);
  var failingMotion = new ScenarioKiteMotionPort(failOnCall: 2);
  bool failedAfterFirstPreparation = false;
  try
  {
    sections.AdvanceActiveSections(tick: 1, failingMotion);
  }
  catch (InvalidOperationException exception) when (
      exception.Message == ScenarioKiteMotionPort.InjectedFailureMessage)
  {
    failedAfterFirstPreparation = true;
  }
  Require(failedAfterFirstPreparation && failingMotion.CallCount == 2,
    "SC071 leash-tick fault injection did not fail after preparing the first Kite.");
  Require(registry.Revision == beforeFailureRevision &&
    registry.CaptureSection(section).SequenceEqual(beforeFailure) &&
    beforeFailure.All(entity => identityAuthority.IsActive(entity.Handle)),
    "SC071 a failed leash section tick partially committed state or retired an identity.");

  LeashedEntitySectionService.SectionTickPreparation discarded =
    sections.PrepareAdvanceActiveSections(tick: 1, new ScenarioKiteMotionPort());
  Require(discarded.Validate() && discarded.Result.Tick == 1 &&
    discarded.Result.StateChanges.Count == 2 &&
    discarded.Result.StateChanges.All(change => change.Current?.Motion?.LastTick == 1) &&
    registry.Revision == beforeFailureRevision &&
    registry.CaptureSection(section).SequenceEqual(beforeFailure),
    "SC071 prepared leash effects were unavailable before commit or changed live state.");
  int encodedCandidates = 0;
  bool encodingRejected = false;
  try
  {
    foreach (LeashedEntityChangeResult change in discarded.Result.StateChanges)
    {
      LeashedEntityRuntimeSnapshot candidate = change.Current!;
      var partial = (Version4ModulePayloads.KiteLeashedEntityState)candidate.State;
      // Reject the second candidate at the real codec boundary, after encoding the first.
      var packet = new NetModulesPacket
      {
        ModuleId = LeashedEntityModuleHandler.ModuleId,
        Data = new Version4ModulePayloads.LeashedEntityMessage(
          Action: 2,
          candidate.Slot,
          candidate.Type,
          AnchorX: encodedCandidates == 1 ? (short)candidate.Anchor.X : null,
          AnchorY: null,
          partial with { ProjectileType = null })
      };
      Version4EncodedPacket encoded = wireProfile.Encode(WireDirection.ServerToClient, packet);
      Require(encoded.MessageId == 82, "SC071 prepared leash output was not module packet 82.");
      encodedCandidates++;
    }
  }
  catch (Exception exception) when (exception is InvalidOperationException or PacketWireFormatException)
  {
    encodingRejected = true;
  }
  Require(encodingRejected && encodedCandidates == 1 && discarded.Validate() &&
    registry.Revision == beforeFailureRevision &&
    registry.CaptureSection(section).SequenceEqual(beforeFailure) &&
    beforeFailure.All(entity => identityAuthority.IsActive(entity.Handle)),
    "SC071 rejected pre-encoding partially committed a prepared leash tick.");

  LeashedEntitySectionService.SectionTickPreparation recovery =
    sections.PrepareAdvanceActiveSections(tick: 1, new ScenarioKiteMotionPort());
  LeashedEntitySectionService.SectionTickResult recovered = recovery.Commit();
  Require(recovered.StateChanges.Count == 2 && registry.Revision == beforeFailureRevision + 2 &&
    registry.CaptureSection(section).All(entity => entity.Motion?.LastTick == 1) &&
    ReferenceEquals(recovered, recovery.Result),
    "SC071 the leash section did not commit atomically after a rejected tick preparation.");
  RequireLeashedTickCommitRejected(discarded, "a competing tick committed");
  RequireLeashedTickCommitRejected(recovery, "the same preparation committed twice");

  LeashedEntityRuntimeSnapshot oldEntity = registry.CaptureSection(section)[0];
  Require(registry.DeactivateAt(oldEntity.Anchor),
    "SC071 the replaced leash anchor was not deactivated.");
  LeashedEntityChangeResult replacementChange = registry.Spawn(
    oldEntity.Type,
    oldEntity.Anchor,
    section,
    new LeashedEntitySpawnState(oldEntity.State, oldEntity.Motion!));
  LeashedEntityRuntimeSnapshot replacement = replacementChange.Current!;
  Require(replacementChange.Kind == LeashedEntityChangeKind.Created &&
    replacement.Slot != oldEntity.Slot && replacement.Handle != oldEntity.Handle &&
    identityAuthority.IsActive(oldEntity.Handle) && identityAuthority.IsActive(replacement.Handle) &&
    registry.Count == 3 && registry.TryGet(oldEntity.Slot, out var detached) &&
    detached is { Active: false } &&
    registry.TryGetAt(oldEntity.Anchor, out var indexedReplacement) &&
    indexedReplacement == replacement,
    "SC071 same-anchor replacement did not retain distinct old and new leash identities.");

  long replacementRevision = registry.Revision;
  IReadOnlyList<LeashedEntityRuntimeSnapshot> beforeReplacementTick =
    registry.CaptureSection(section);
  var replacementFailure = new ScenarioKiteMotionPort(failOnCall: 2);
  bool replacementPreparationFailed = false;
  try
  {
    sections.AdvanceActiveSections(tick: 2, replacementFailure);
  }
  catch (InvalidOperationException exception) when (
      exception.Message == ScenarioKiteMotionPort.InjectedFailureMessage)
  {
    replacementPreparationFailed = true;
  }
  Require(replacementPreparationFailed && replacementFailure.CallCount == 2 &&
    registry.Revision == replacementRevision &&
    registry.CaptureSection(section).SequenceEqual(beforeReplacementTick) &&
    beforeReplacementTick.All(entity => identityAuthority.IsActive(entity.Handle)),
    "SC071 a rejected replacement tick partially removed the old leash or advanced its peers.");

  var replacementMotion = new ScenarioKiteMotionPort();
  LeashedEntitySectionService.SectionTickPreparation replacementPreparation =
    sections.PrepareAdvanceActiveSections(tick: 2, replacementMotion);
  Require(replacementPreparation.Validate() && replacementPreparation.Result.StateChanges.Count == 3 &&
    registry.Revision == replacementRevision &&
    registry.CaptureSection(section).SequenceEqual(beforeReplacementTick) &&
    beforeReplacementTick.All(entity => identityAuthority.IsActive(entity.Handle)),
    "SC071 preparing mixed leash removal/update effects retired an identity or changed state.");
  identityAuthority.RejectRetirements = true;
  bool retirementRejected = false;
  try
  {
    replacementPreparation.Commit();
  }
  catch (InvalidOperationException)
  {
    retirementRejected = true;
  }
  identityAuthority.RejectRetirements = false;
  Require(retirementRejected && replacementPreparation.Validate() &&
    registry.Revision == replacementRevision &&
    registry.CaptureSection(section).SequenceEqual(beforeReplacementTick) &&
    beforeReplacementTick.All(entity => identityAuthority.IsActive(entity.Handle)),
    "SC071 rejected identity retirement partially committed or consumed the leash preparation.");
  LeashedEntitySectionService.SectionTickResult replacementTick = replacementPreparation.Commit();
  Require(replacementMotion.CallCount == 2 && replacementTick.StateChanges.Count == 3 &&
    replacementTick.StateChanges.Count(change => change.Kind == LeashedEntityChangeKind.Removed &&
      change.Previous?.Handle == oldEntity.Handle) == 1 &&
    replacementTick.StateChanges.Count(change => change.Kind == LeashedEntityChangeKind.Updated) == 2 &&
    replacementTick.StateChanges.Any(change => change.Kind == LeashedEntityChangeKind.Updated &&
      change.Current?.Handle == replacement.Handle),
    "SC071 replacement tick did not remove only the old slot and advance the two active slots.");
  Require(registry.Revision == replacementRevision + 3 && registry.Count == 2 &&
    !registry.TryGet(oldEntity.Slot, out _) && !identityAuthority.IsActive(oldEntity.Handle) &&
    identityAuthority.IsActive(replacement.Handle) &&
    registry.TryGetAt(oldEntity.Anchor, out indexedReplacement) &&
    indexedReplacement?.Handle == replacement.Handle && indexedReplacement.Active &&
    indexedReplacement.Motion?.LastTick == 2 &&
    registry.CaptureSection(section).All(entity => entity.Active && entity.Motion?.LastTick == 2) &&
    registry.CaptureSection(section).Select(entity => entity.Handle).OrderBy(handle => handle.Generation)
      .SequenceEqual(beforeReplacementTick.Where(entity => entity.Active)
        .Select(entity => entity.Handle).OrderBy(handle => handle.Generation)),
    "SC071 replacement cleanup lost the new anchor, section membership or identity lifecycle.");

  long beforeReactivationRevision = registry.Revision;
  IReadOnlyList<LeashedEntityRuntimeSnapshot> beforeReactivation = registry.CaptureSection(section);
  LeashedEntitySectionService.SectionTickPreparation beforeDeactivation =
    sections.PrepareAdvanceActiveSections(tick: 3, new ScenarioKiteMotionPort());
  LeashedEntitySectionService.SectionDeactivationResult deactivation =
    sections.DeactivateSection(section);
  RequireLeashedTickCommitRejected(beforeDeactivation, "its section was deactivated");
  Require(deactivation.WasDeactivated && !sections.IsActive(section) &&
    deactivation.RetainedEntities.SequenceEqual(beforeReactivation) &&
    registry.Revision == beforeReactivationRevision &&
    beforeReactivation.All(entity => identityAuthority.IsActive(entity.Handle)),
    "SC071 section deactivation changed retained motion, revisions or identities.");
  var inactiveMotion = new ScenarioKiteMotionPort(failOnCall: 1);
  LeashedEntitySectionService.SectionTickResult inactiveTick =
    sections.AdvanceActiveSections(tick: 3, inactiveMotion);
  Require(inactiveMotion.CallCount == 0 && inactiveTick.StateChanges.Count == 0 &&
    inactiveTick.PeriodicUpdates.Count == 0 && registry.Revision == beforeReactivationRevision &&
    registry.CaptureSection(section).SequenceEqual(beforeReactivation),
    "SC071 an inactive section advanced retained entities.");

  const long activationTick = 10;
  foreach (long invalidTick in new[] { activationTick - 1, activationTick + 1 })
  {
    int spawnCalls = 0;
    bool rejected = false;
    try
    {
      sections.ActivateSection(section, activationTick, (entity, tick) =>
      {
        spawnCalls++;
        return PrepareScenarioKiteSpawn(entity, spawnCalls == 2 ? invalidTick : tick);
      });
    }
    catch (InvalidOperationException)
    {
      rejected = true;
    }
    Require(rejected && spawnCalls == 2 && !sections.IsActive(section) &&
      sections.ActiveSections.Count == 0 && registry.Revision == beforeReactivationRevision &&
      registry.CaptureSection(section).SequenceEqual(beforeReactivation) &&
      beforeReactivation.All(entity => identityAuthority.IsActive(entity.Handle)),
      $"SC071 activation accepted tick {invalidTick} or partially committed its first spawn.");
  }

  LeashedEntitySectionService.SectionActivationPreparation beforeRegistryChange =
    sections.PrepareActivateSection(section, activationTick, PrepareScenarioKiteSpawn);
  var individualMotion = new ScenarioKiteMotionPort();
  Require(individualMotion.TryAdvance(beforeReactivation[0], tick: 3, out var individualStep),
    "SC071 could not prepare the competing leash registry update.");
  registry.ApplyMotion(beforeReactivation[0], individualStep!);
  beforeReactivationRevision = registry.Revision;
  beforeReactivation = registry.CaptureSection(section);
  RequireLeashedActivationCommitRejected(beforeRegistryChange, "the registry changed");
  Require(!sections.IsActive(section) && registry.Revision == beforeReactivationRevision &&
    registry.CaptureSection(section).SequenceEqual(beforeReactivation),
    "SC071 a stale activation overwrote a competing registry update.");

  MechanismSectionCoordinate emptySection = new(1, 0);
  LeashedEntitySectionService.SectionActivationPreparation beforeActivationCycle =
    sections.PrepareActivateSection(section, activationTick, PrepareScenarioKiteSpawn);
  sections.ActivateSection(emptySection);
  sections.DeactivateSection(emptySection);
  RequireLeashedActivationCommitRejected(beforeActivationCycle, "an empty section cycled");
  Require(!sections.IsActive(section) && sections.ActiveSections.Count == 0 &&
    registry.Revision == beforeReactivationRevision &&
    registry.CaptureSection(section).SequenceEqual(beforeReactivation),
    "SC071 a stale activation committed after an empty-section cycle.");

  LeashedEntitySectionService.SectionActivationPreparation discardedActivation =
    sections.PrepareActivateSection(section, activationTick, PrepareScenarioKiteSpawn);
  Require(discardedActivation.Validate() && discardedActivation.Result.WasActivated &&
    discardedActivation.Result.FullEntities.Count == 2 &&
    discardedActivation.Result.FullEntities.All(entity =>
      entity.Motion?.LastTick == activationTick) &&
    !sections.IsActive(section) && registry.Revision == beforeReactivationRevision &&
    registry.CaptureSection(section).SequenceEqual(beforeReactivation),
    "SC071 a prepared Full baseline was unavailable before commit or changed live state.");
  int encodedFullCandidates = 0;
  bool fullEncodingRejected = false;
  try
  {
    foreach (LeashedEntityRuntimeSnapshot candidate in discardedActivation.Result.FullEntities)
    {
      var packet = new NetModulesPacket
      {
        ModuleId = LeashedEntityModuleHandler.ModuleId,
        Data = new Version4ModulePayloads.LeashedEntityMessage(
          Action: 1,
          candidate.Slot,
          candidate.Type,
          AnchorX: (short)candidate.Anchor.X,
          AnchorY: encodedFullCandidates == 1 ? null : (short)candidate.Anchor.Y,
          candidate.State)
      };
      Version4EncodedPacket encoded = wireProfile.Encode(WireDirection.ServerToClient, packet);
      Require(encoded.MessageId == 82, "SC071 a prepared Full was not module packet 82.");
      encodedFullCandidates++;
    }
  }
  catch (Exception exception) when (
      exception is InvalidOperationException or PacketWireFormatException)
  {
    fullEncodingRejected = true;
  }
  Require(fullEncodingRejected && encodedFullCandidates == 1 && discardedActivation.Validate() &&
    !sections.IsActive(section) && registry.Revision == beforeReactivationRevision &&
    registry.CaptureSection(section).SequenceEqual(beforeReactivation) &&
    beforeReactivation.All(entity => identityAuthority.IsActive(entity.Handle)),
    "SC071 rejected Full encoding committed respawn state or activated the section.");

  LeashedEntitySectionService.SectionActivationPreparation preparedActivation =
    sections.PrepareActivateSection(section, activationTick, PrepareScenarioKiteSpawn);
  LeashedEntitySectionService.SectionActivationResult reactivation = preparedActivation.Commit();
  Require(ReferenceEquals(reactivation, preparedActivation.Result),
    "SC071 activation committed a different baseline from its prepared result.");
  RequireLeashedActivationCommitRejected(discardedActivation, "a competing activation committed");
  RequireLeashedActivationCommitRejected(preparedActivation,
    "the same activation committed twice");
  Require(reactivation.WasActivated && reactivation.FullEntities.Count == 2 &&
    sections.IsActive(section) && sections.ActiveSections.Count == 1 &&
    registry.Revision == beforeReactivationRevision + 1 &&
    reactivation.FullEntities.All(entity => entity.Motion?.LastTick == activationTick) &&
    reactivation.FullEntities.Select(entity => entity.Handle)
      .SequenceEqual(beforeReactivation.Select(entity => entity.Handle)) &&
    sections.CaptureForObserver(section).SequenceEqual(reactivation.FullEntities),
    "SC071 section reactivation did not publish one complete baseline at its activation tick.");
  long afterReactivationRevision = registry.Revision;
  IReadOnlyList<LeashedEntityRuntimeSnapshot> laterObserverBaseline =
    sections.CaptureForObserver(section);
  foreach (LeashedEntityRuntimeSnapshot entity in laterObserverBaseline)
  {
    var packet = new NetModulesPacket
    {
      ModuleId = LeashedEntityModuleHandler.ModuleId,
      Data = new Version4ModulePayloads.LeashedEntityMessage(
        Action: 1,
        entity.Slot,
        entity.Type,
        AnchorX: checked((short)entity.Anchor.X),
        AnchorY: checked((short)entity.Anchor.Y),
        entity.State)
    };
    Version4EncodedPacket encoded = wireProfile.Encode(WireDirection.ServerToClient, packet);
    Require(encoded.MessageId == 82,
      "SC071 a later-observer Full baseline was not module packet 82.");
  }
  Require(laterObserverBaseline.SequenceEqual(reactivation.FullEntities) &&
    registry.Revision == afterReactivationRevision && sections.IsActive(section) &&
    registry.CaptureSection(section).SequenceEqual(reactivation.FullEntities),
    "SC071 capturing and encoding a later-observer Full baseline changed retained state.");
  LeashedEntitySectionService.SectionActivationResult duplicateActivation = sections.ActivateSection(
    section, activationTick + 1, static (_, _) =>
      throw new InvalidOperationException("An active section cannot spawn twice."));
  Require(!duplicateActivation.WasActivated && duplicateActivation.FullEntities.Count == 0 &&
    registry.Revision == afterReactivationRevision &&
    registry.CaptureSection(section).SequenceEqual(reactivation.FullEntities),
    "SC071 repeated section activation respawned or changed retained entities.");
  LeashedEntitySectionService.SectionActivationPreparation noOpActivation =
    sections.PrepareActivateSection(section, activationTick + 1, static (_, _) =>
      throw new InvalidOperationException("A prepared active section cannot spawn twice."));
  Require(!noOpActivation.Commit().WasActivated &&
    registry.Revision == afterReactivationRevision &&
    registry.CaptureSection(section).SequenceEqual(reactivation.FullEntities),
    "SC071 a no-op activation preparation respawned or changed retained entities.");
  RequireLeashedActivationCommitRejected(noOpActivation,
    "a no-op activation committed twice");
  LeashedEntitySectionService.SectionTickResult reactivatedTick = sections.AdvanceActiveSections(
    activationTick + 1, new ScenarioKiteMotionPort());
  Require(reactivatedTick.StateChanges.Count == 2 &&
    registry.CaptureSection(section).All(entity => entity.Motion?.LastTick == activationTick + 1),
    "SC071 a valid reactivation could not advance on the next tick.");

  IReadOnlyList<LeashedEntityRuntimeSnapshot> beforePeriodic = registry.CaptureSection(section);
  long periodicTick = LeashedEntitySectionService.PeriodicUpdateIntervalTicks - beforePeriodic[0].Slot;
  long beforePeriodicRevision = registry.Revision;
  LeashedEntitySectionService.SectionTickPreparation beforeSectionCycle =
    sections.PrepareAdvanceActiveSections(periodicTick, new ScenarioKiteMotionPort());
  sections.ActivateSection(emptySection);
  sections.DeactivateSection(emptySection);
  RequireLeashedTickCommitRejected(beforeSectionCycle, "an empty section was activated and deactivated");
  Require(registry.Revision == beforePeriodicRevision &&
    registry.CaptureSection(section).SequenceEqual(beforePeriodic) &&
    sections.ActiveSections.SequenceEqual(new[] { section }),
    "SC071 an obsolete section preparation changed retained state after a section cycle.");

  LeashedEntitySectionService.SectionTickPreparation periodicPreparation =
    sections.PrepareAdvanceActiveSections(periodicTick, new ScenarioKiteMotionPort());
  LeashedEntitySectionService.PeriodicPartialUpdate periodic =
    periodicPreparation.Result.PeriodicUpdates.Single();
  Require(periodic.Previous == beforePeriodic[0] &&
    periodic.Current.Motion?.LastTick == periodicTick &&
    periodic.Current == periodicPreparation.Result.StateChanges.Single(change =>
      change.Current?.Slot == periodic.Current.Slot).Current &&
    registry.Revision == beforePeriodicRevision &&
    registry.CaptureSection(section).SequenceEqual(beforePeriodic),
    "SC071 periodic leash effects did not contain candidate motion before commit.");
  Require(ReferenceEquals(periodicPreparation.Commit(), periodicPreparation.Result) &&
    registry.TryGet(periodic.Current.Slot, out var periodicCommitted) &&
    periodicCommitted == periodic.Current,
    "SC071 the periodic leash commit differed from its prepared effect snapshot.");

  LeashedEntitySectionService.SectionTickPreparation beforeUnload =
    sections.PrepareAdvanceActiveSections(periodicTick + 1, new ScenarioKiteMotionPort());
  LeashedEntitySectionService.SectionActivationPreparation activationBeforeUnload =
    sections.PrepareActivateSection(emptySection, periodicTick + 1, PrepareScenarioKiteSpawn);
  LeashedEntitySectionService.WorldUnloadResult unload = sections.ClearForWorldUnload();
  RequireLeashedTickCommitRejected(beforeUnload, "the world was unloaded");
  RequireLeashedActivationCommitRejected(activationBeforeUnload, "the world was unloaded");
  Require(unload.RetiredEntityCount == 2 && unload.ClearedSectionCount == 1 &&
    registry.Count == 0 && sections.ActiveSections.Count == 0 &&
    beforePeriodic.All(entity => !identityAuthority.IsActive(entity.Handle)),
    "SC071 a stale prepared leash tick restored unloaded state or identities.");
  long emptyRevision = registry.Revision;
  LeashedEntitySectionService.SectionTickPreparation beforeEmptyUnload =
    sections.PrepareAdvanceActiveSections(periodicTick + 2, new ScenarioKiteMotionPort(failOnCall: 1));
  LeashedEntitySectionService.SectionActivationPreparation activationBeforeEmptyUnload =
    sections.PrepareActivateSection(emptySection, periodicTick + 2, PrepareScenarioKiteSpawn);
  sections.ClearForWorldUnload();
  RequireLeashedTickCommitRejected(beforeEmptyUnload, "an empty world was unloaded again");
  RequireLeashedActivationCommitRejected(activationBeforeEmptyUnload,
    "an empty world unloaded again");
  LeashedEntitySectionService.SectionTickPreparation emptyPreparation =
    sections.PrepareAdvanceActiveSections(periodicTick + 2, new ScenarioKiteMotionPort(failOnCall: 1));
  Require(emptyPreparation.Commit().StateChanges.Count == 0 && registry.Revision == emptyRevision,
    "SC071 an empty prepared leash tick mutated the registry.");
  RequireLeashedTickCommitRejected(emptyPreparation, "an empty preparation committed twice");
  LeashedEntitySectionService.SectionActivationPreparation emptyActivation =
    sections.PrepareActivateSection(emptySection, periodicTick + 2, PrepareScenarioKiteSpawn);
  Require(emptyActivation.Commit().WasActivated && sections.IsActive(emptySection) &&
    emptyActivation.Result.FullEntities.Count == 0 && registry.Revision == emptyRevision,
    "SC071 a prepared empty activation changed the registry or failed to activate.");
  RequireLeashedActivationCommitRejected(emptyActivation, "an empty activation committed twice");

  var retiredIdentityAuthority = new ScenarioMechanismIdentityAuthority(worldScope);
  var retiredRegistry = new LeashedEntityRegistry(factsCatalog,
    retiredIdentityAuthority, retiredIdentityAuthority.Create, worldScope);
  LeashedEntityRuntimeSnapshot template = reactivation.FullEntities[0];
  retiredRegistry.ApplyFull(new Version4ModulePayloads.LeashedEntityMessage(
    Action: 1,
    template.Slot,
    template.Type,
    AnchorX: checked((short)template.Anchor.X),
    AnchorY: checked((short)template.Anchor.Y),
    template.State), section);
  var retiredSections = new LeashedEntitySectionService(retiredRegistry);
  LeashedEntityRuntimeSnapshot beforeRetirement = retiredSections.ActivateSection(
    section, activationTick, PrepareScenarioKiteSpawn).FullEntities.Single();
  long beforeRejectedDeactivationRevision = retiredRegistry.Revision;
  Require(retiredIdentityAuthority.TryRetireMany([beforeRetirement.Handle]),
    "SC071 could not invalidate the identity before section deactivation.");
  bool deactivationRejected = false;
  try
  {
    retiredSections.DeactivateSection(section);
  }
  catch (InvalidOperationException)
  {
    deactivationRejected = true;
  }
  Require(deactivationRejected && retiredSections.IsActive(section) &&
    retiredSections.ActiveSections.SequenceEqual(new[] { section }) &&
    retiredRegistry.Revision == beforeRejectedDeactivationRevision &&
    retiredRegistry.Count == 1 &&
    retiredRegistry.TryGet(beforeRetirement.Slot, out LeashedEntityRuntimeSnapshot? retained) &&
    Equals(retained, beforeRetirement) &&
    !retiredIdentityAuthority.IsActive(beforeRetirement.Handle),
    "SC071 a rejected deactivation changed section membership or retained registry state.");
  return "a second-entity motion failure left both slots and the registry revision unchanged; " +
    "prepared effects were readable without mutation, a real second-output codec rejection left " +
    "state unchanged, and a fresh preparation committed both motion states; obsolete and duplicate " +
    "commits were rejected; same-anchor replacement retained " +
    "both identities until a successful tick removed only the old inactive slot and advanced " +
    "the active replacement; motion and identity-retirement failures changed no state; " +
    "deactivation preserved state and skipped motion; reactivation rejected past/future spawn " +
    "ticks on its second entity without publishing state, then retried at one activation tick " +
    "with the same identities, one observer baseline and a successful next tick; repeated " +
    "activation did not respawn; Full baselines were prepared before activation, a real " +
    "second-Full codec rejection left respawn and section state unchanged, and fresh activation " +
    "committed the prepared baseline; registry changes and empty-section cycles rejected " +
    "stale activations; " +
    "normal, no-op and empty activations remained single-use; later-observer Full capture and " +
    "real codec encoding preserved retained state; a retired-identity deactivation rejected " +
    "without changing active sections or registry state; " +
    "empty-section activation cycles invalidated pending effects; " +
    "periodic effects used candidate motion before commit; populated and empty world unloads " +
    "invalidated pending ticks and activations, and empty commits remained single-use";
}

static void RequireLeashedActivationCommitRejected(
  LeashedEntitySectionService.SectionActivationPreparation preparation,
  string reason)
{
  Require(!preparation.Validate(), $"SC071 a leash activation remained valid after {reason}.");
  bool rejected = false;
  try
  {
    preparation.Commit();
  }
  catch (InvalidOperationException)
  {
    rejected = true;
  }
  Require(rejected, $"SC071 a leash activation committed after {reason}.");
}

static void RequireLeashedTickCommitRejected(
  LeashedEntitySectionService.SectionTickPreparation preparation,
  string reason)
{
  Require(!preparation.Validate(), $"SC071 a leash preparation remained valid after {reason}.");
  bool rejected = false;
  try
  {
    preparation.Commit();
  }
  catch (InvalidOperationException)
  {
    rejected = true;
  }
  Require(rejected, $"SC071 a leash preparation committed after {reason}.");
}

static LeashedEntitySpawnState PrepareScenarioKiteSpawn(
  LeashedEntityRuntimeSnapshot entity,
  long tick)
{
  Version4ModulePayloads.KiteLeashedEntityState state =
    (Version4ModulePayloads.KiteLeashedEntityState)entity.State;
  return new LeashedEntitySpawnState(
    state,
    new LeashedEntityMotionState(
      new Vector2(state.PositionX, state.PositionY),
      Vector2.Zero,
      SpriteDirection: 0,
      LastTick: tick));
}

static string VerifyEmptyRecipientKiteRemoval(
  WorldFacts facts,
  MechanismFactsCatalog factsCatalog,
  Version4WireProfile wireProfile)
{
  const long worldScope = 7072;
  const short anchorX = 35;
  const short anchorY = 30;
  var worldAnchor = new TileCoordinate(anchorX, anchorY);
  var entityAnchor = new Terraria.WorldStorage.TileCoordinate(anchorX, anchorY);
  var kiteTile = new WorldTile(
    Active: true,
    Type: 723,
    Wall: 0,
    FrameX: 0,
    FrameY: 0,
    LiquidAmount: 0,
    LiquidType: LiquidType.Water,
    TileColor: 0,
    WallColor: 0,
    Wires: TileWireMask.None,
    Actuator: false,
    Inactive: false);
  var additionalTiles = new (TileCoordinate Coordinate, WorldTile Tile)[]
  {
    (worldAnchor, kiteTile)
  };
  var worldOwner = new WorldOwner(facts);
  WorldPublicationResult publication = worldOwner.CommitPreparedWorld(
    worldOwner.PrepareWorld(CreateFixture(facts, additionalTiles)));
  Require(publication.Published,
    $"SC071 empty-recipient Kite-removal fixture publication failed: {publication.Rejection}");
  WorldSnapshot before = worldOwner.CaptureSnapshot();
  var identityAuthority = new ScenarioMechanismIdentityAuthority(worldScope);
  MechanismEntityHandle tileEntityHandle = identityAuthority.Create();
  var entities = new TileEntityRegistry(factsCatalog, identityAuthority, worldScope);
  var anchorTile = new MechanismTileSnapshot(true, 723, 0, 0);
  TileEntityRuntimeState entity = entities.Create(
    tileEntityHandle,
    new TileEntityCreateRequest(
      Type: 9,
      Anchor: entityAnchor,
      Section: new MechanismSectionCoordinate(0, 0),
      AnchorTile: anchorTile,
      Items: []));
  var leashedEntities = new LeashedEntityRegistry(
    factsCatalog,
    identityAuthority,
    identityAuthority.Create,
    worldScope);
  LeashedEntityChangeResult createdLeash = leashedEntities.ApplyFull(
    new Version4ModulePayloads.LeashedEntityMessage(
      Action: 1,
      Slot: 5,
      EntityType: 1,
      AnchorX: anchorX,
      AnchorY: anchorY,
      State: new Version4ModulePayloads.KiteLeashedEntityState(
        ProjectileType: 1,
        PositionX: 0,
        PositionY: 0,
        PackedVelocity: 0,
        Rotation: 0,
        WindTarget: 0,
        CloudAlpha: 0,
        TimeCounter: 0)),
    new MechanismSectionCoordinate(0, 0));
  Require(createdLeash.Kind == LeashedEntityChangeKind.Created &&
    createdLeash.Current is not null,
    "SC071 empty-recipient Kite-removal fixture did not create its leashed entity.");
  LeashedEntityRuntimeSnapshot activeLeash = createdLeash.Current
    ?? throw new InvalidOperationException(
      "SC071 empty-recipient Kite-removal fixture did not retain its leash snapshot.");
  var producer = new MechanismPacketProducer(wireProfile, facts);
  var coordinator = new TileEntityRemovalCoordinator(
    new MechanismWorldPort(worldOwner),
    entities,
    new TileEntityProjection(worldScope),
    new TeleportPylonRegistry(factsCatalog, worldScope),
    leashedEntities,
    identityAuthority,
    producer);
  TileEntityRemovalDispatchResult removal = coordinator.Remove(
    entity.Handle,
    before,
    [new WorldTilePatch(worldAnchor, Active: false, Type: 0)],
    Array.Empty<SessionEpoch>(),
    before.Version);

  CommitVersion committedVersion = removal.CommitVersion
    ?? throw new InvalidOperationException(
      $"SC071 empty-recipient Kite removal did not return a commit version: {removal.Status} " +
      removal.Detail);
  Require(removal.Status == TileEntityRemovalStatus.Removed && removal.OutboundIntents.Count == 0,
    $"SC071 empty-recipient Kite removal did not complete without intents: {removal.Status} " +
    removal.Detail);
  Require(removal.LeashedEntityDeactivated &&
    removal.Removal?.State.Lifecycle == MechanismEntityLifecycle.Retired &&
    entities.Count == 0 && !identityAuthority.IsActive(tileEntityHandle),
    "SC071 empty-recipient Kite removal did not retire its TileEntity and deactivate its leash.");
  LeashedEntityRuntimeSnapshot inactiveLeash = leashedEntities.Capture().Single();
  Require(inactiveLeash.Handle == activeLeash.Handle && !inactiveLeash.Active &&
    identityAuthority.IsActive(activeLeash.Handle) && leashedEntities.Count == 1 &&
    !leashedEntities.TryGetAt(entityAnchor, out _),
    "SC071 empty-recipient Kite removal did not retain the inactive slot until section removal.");
  WorldSnapshot after = worldOwner.CaptureSnapshot();
  Require(after.Version == committedVersion && !after.GetTile(worldAnchor).Active,
    "SC071 empty-recipient Kite removal did not commit its world tile patch.");

  return $"removed the type-9 TileEntity at {worldAnchor}, committed its world tile patch and " +
    $"deactivated the Kite leash at version {committedVersion.Value}; the inactive slot and its " +
    "identity remain until the later active-section removal, with zero packet 86/20 intents";
}

static Version4ProtocolFactsInput CreateProtocolFacts(WorldFacts facts)
{
  return new Version4ProtocolFactsInput(
    facts.CopyFrameImportantForProtocolInputs(),
    facts.CopyCompressionBatchingForProtocolInputs(),
    PacketTileEntityCodecsV4.Create(),
    [true],
    static (_, _, _) => 1,
    static _ => false,
    40,
    0,
    static _ => false,
    WorldFacts.Version4MainSha256);
}

static WorldPreparationRequest CreateFixture(
  WorldFacts facts,
  IReadOnlyList<(TileCoordinate Coordinate, WorldTile Tile)>? additionalTiles = null)
{
  const int width = 80;
  const int height = 80;
  var geometry = new WorldGeometry(width, height);
  var headerFacts = new WorldHeaderFacts(
    seedText: "SC071",
    worldGeneratorVersion: 1,
    leftWorld: 0,
    rightWorld: width,
    topWorld: 0,
    bottomWorld: height,
    gameMode: 0,
    drunkWorld: false,
    getGoodWorld: false,
    tenthAnniversaryWorld: false,
    dontStarveWorld: false,
    notTheBeesWorld: false,
    remixWorld: false,
    noTrapsWorld: false,
    zenithWorld: false,
    skyblockWorld: false,
    creationTime: null,
    lastPlayed: null,
    moonType: 0,
    treeX: [0, 0, 0],
    treeStyle: [0, 0, 0, 0],
    caveBackX: [0, 0, 0],
    caveBackStyle: [0, 0, 0, 0],
    iceBackStyle: 0,
    jungleBackStyle: 0,
    hellBackStyle: 0,
    worldSurface: 40,
    rockLayer: 50,
    bloodMoon: false,
    eclipse: false,
    dungeonX: 0,
    dungeonY: 0,
    crimson: false);
  var header = new WorldHeaderSnapshot(
    "SC071",
    geometry,
    new TileCoordinate(1, 1),
    Guid.Parse("0d8bf3d4-2c8f-4f7a-bf10-071071071071"),
    dayTime: true,
    time: 100,
    moonPhase: 0,
    worldId: 71,
    headerFacts);

  var tiles = Enumerable.Repeat(WorldTile.Empty, geometry.TileCount).ToArray();
  for (int x = 40; x <= 44; x++)
  {
    tiles[(40 * width) + x] = WorldTile.Empty with { Wires = TileWireMask.Red };
  }
  tiles[(40 * width) + 40] = new WorldTile(
    Active: true,
    Type: 136,
    Wall: 0,
    FrameX: 0,
    FrameY: 0,
    LiquidAmount: 0,
    LiquidType: LiquidType.Water,
    TileColor: 0,
    WallColor: 0,
    Wires: TileWireMask.Red,
    Actuator: false,
    Inactive: false);
  tiles[(40 * width) + 44] = new WorldTile(
    Active: true,
    Type: 421,
    Wall: 0,
    FrameX: -1,
    FrameY: -1,
    LiquidAmount: 0,
    LiquidType: LiquidType.Water,
    TileColor: 0,
    WallColor: 0,
    Wires: TileWireMask.Red,
    Actuator: false,
    Inactive: false);
  foreach ((TileCoordinate coordinate, WorldTile tile) in
    additionalTiles ?? Array.Empty<(TileCoordinate, WorldTile)>())
  {
    if (!geometry.Contains(coordinate))
    {
      throw new ArgumentOutOfRangeException(nameof(additionalTiles));
    }
    tiles[(coordinate.Y * width) + coordinate.X] = tile;
  }
  return new WorldPreparationRequest(header, tiles, facts.Fingerprint);
}

static async Task<IReadOnlyList<RawPacketFrame>> ReceiveFramesAsync(
  Socket socket,
  int expectedCount,
  CancellationToken cancellationToken)
{
  if (expectedCount <= 0)
  {
    throw new ArgumentOutOfRangeException(nameof(expectedCount));
  }

  var assembler = new PacketFrameAssembler(64 * 1024);
  var frames = new List<RawPacketFrame>(expectedCount);
  byte[] buffer = new byte[32];
  while (frames.Count < expectedCount)
  {
    int received = await socket.ReceiveAsync(
      buffer,
      SocketFlags.None,
      cancellationToken).ConfigureAwait(false);
    if (received == 0)
    {
      throw new InvalidOperationException(
        $"The SC071 client socket closed after receiving {frames.Count} frame(s).");
    }

    assembler.Append(buffer.AsSpan(0, received));
    while (assembler.TryReadFrame(out RawPacketFrame? frame))
    {
      frames.Add(frame!);
      if (frames.Count == expectedCount)
      {
        break;
      }
    }
  }

  assembler.Dispose();
  return frames.AsReadOnly();
}

static async Task SendAllAsync(
  Socket socket,
  ReadOnlyMemory<byte> bytes,
  CancellationToken cancellationToken)
{
  int offset = 0;
  while (offset < bytes.Length)
  {
    int sent = await socket.SendAsync(
      bytes[offset..],
      SocketFlags.None,
      cancellationToken).ConfigureAwait(false);
    if (sent <= 0)
    {
      throw new InvalidOperationException("The SC071 client socket made no send progress.");
    }
    offset += sent;
  }
}

static void AssertDecodedTileMatches(
  WorldFacts facts,
  Packet20Tile actual,
  WorldTile expected,
  TileCoordinate coordinate)
{
  Require(actual.Active == expected.Active, $"SC071 packet 20 active mismatch at {coordinate}.");
  Require(actual.TileType == (expected.Active ? expected.Type : 0),
    $"SC071 packet 20 type mismatch at {coordinate}.");
  Require(actual.Wire == ((expected.Wires & TileWireMask.Red) != 0),
    $"SC071 packet 20 red wire mismatch at {coordinate}.");
  Require(actual.Wire2 == ((expected.Wires & TileWireMask.Blue) != 0),
    $"SC071 packet 20 blue wire mismatch at {coordinate}.");
  Require(actual.Wire3 == ((expected.Wires & TileWireMask.Green) != 0),
    $"SC071 packet 20 green wire mismatch at {coordinate}.");
  Require(actual.Wire4 == ((expected.Wires & TileWireMask.Yellow) != 0),
    $"SC071 packet 20 yellow wire mismatch at {coordinate}.");
  Require(actual.Actuator == expected.Actuator && actual.Inactive == expected.Inactive,
    $"SC071 packet 20 actuator/inactive mismatch at {coordinate}.");
  Require(actual.HalfBrick == expected.HalfBlock && actual.Slope == expected.Slope,
    $"SC071 packet 20 geometry mismatch at {coordinate}.");
  Require(actual.TileColor == (expected.Active ? expected.TileColor : 0) &&
    actual.Wall == expected.Wall &&
    actual.WallColor == (expected.Wall > 0 ? expected.WallColor : 0),
    $"SC071 packet 20 color/wall mismatch at {coordinate}.");
  if (expected.Active && expected.Type < facts.FrameImportant.Count &&
      facts.FrameImportant[expected.Type])
  {
    Require(actual.FrameX == expected.FrameX && actual.FrameY == expected.FrameY,
      $"SC071 packet 20 frame mismatch at {coordinate}.");
  }
  else
  {
    Require(!actual.FramePresent && actual.FrameX == 0 && actual.FrameY == 0,
      $"SC071 packet 20 transmitted an absent frame at {coordinate}.");
  }
}

static string Hex(ReadOnlySpan<byte> bytes)
{
  return string.Join(' ', Convert.ToHexString(bytes).Chunk(2).Select(chars => new string(chars)));
}

static bool TryReadArguments(
  IReadOnlyList<string> args,
  out string scenario,
  out string outputRoot)
{
  scenario = string.Empty;
  outputRoot = string.Empty;
  if (args.Count != 4 || args[0] != "--scenario" || args[2] != "--output-root" ||
      string.IsNullOrWhiteSpace(args[1]) || string.IsNullOrWhiteSpace(args[3]))
  {
    return false;
  }

  scenario = args[1];
  outputRoot = Path.GetFullPath(args[3]);
  return true;
}

static void Require(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

sealed class ScenarioMechanismIdentityAuthority : IMechanismIdentityAuthority
{
  private readonly long _worldScope;
  private readonly HashSet<MechanismEntityHandle> _active = [];
  private long _nextGeneration = 1;

  public bool RejectRetirements { get; set; }

  public ScenarioMechanismIdentityAuthority(long worldScope)
  {
    if (worldScope <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldScope));
    }
    _worldScope = worldScope;
  }

  public MechanismEntityHandle Create()
  {
    var handle = new MechanismEntityHandle(
      EntityUuid.Create(),
      _worldScope,
      _nextGeneration);
    _nextGeneration = checked(_nextGeneration + 1);
    _active.Add(handle);
    return handle;
  }

  public bool IsActive(MechanismEntityHandle handle)
  {
    return handle.IsValid && handle.WorldScope == _worldScope && _active.Contains(handle);
  }

  public bool TryRetireMany(IReadOnlyList<MechanismEntityHandle> handles)
  {
    ArgumentNullException.ThrowIfNull(handles);
    if (RejectRetirements || handles.Distinct().Count() != handles.Count ||
        handles.Any(handle => !IsActive(handle)))
    {
      return false;
    }
    foreach (MechanismEntityHandle handle in handles)
    {
      _active.Remove(handle);
    }
    return true;
  }
}

sealed class ScenarioKiteMotionPort : ILeashedEntityMotionPort
{
  public const string InjectedFailureMessage = "Injected section-tick motion failure.";

  private readonly int _failOnCall;

  public ScenarioKiteMotionPort(int failOnCall = 0)
  {
    _failOnCall = failOnCall;
  }

  public int CallCount { get; private set; }

  public bool TryAdvance(
    LeashedEntityRuntimeSnapshot entity,
    long tick,
    out LeashedEntityMotionStep? step)
  {
    CallCount++;
    if (_failOnCall == CallCount)
    {
      step = null;
      throw new InvalidOperationException(InjectedFailureMessage);
    }

    Version4ModulePayloads.KiteLeashedEntityState state =
      entity.State as Version4ModulePayloads.KiteLeashedEntityState
      ?? throw new InvalidOperationException("The SC071 Kite motion received a non-Kite slot.");
    Vector2 position = new(100 + entity.Slot, 200 + entity.Slot);
    var motion = new LeashedEntityMotionState(
      position,
      Vector2.Zero,
      SpriteDirection: 1,
      LastTick: tick);
    step = new LeashedEntityMotionStep(
      state with
      {
        ProjectileType = null,
        PositionX = position.X,
        PositionY = position.Y,
        PackedVelocity = 0,
        TimeCounter = state.TimeCounter + 1
      },
      motion);
    return true;
  }
}
