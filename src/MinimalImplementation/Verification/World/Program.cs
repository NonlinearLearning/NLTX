using MinimalImplementation.Contracts;
using MinimalImplementation.Server.World;
using Terraria.NonAuthoritative.Persistence;

if (!TryReadArguments(args, out string scenario, out string outputRoot))
{
  Console.Error.WriteLine("Usage: --scenario SC026 --output-root <Build/diagnostics directory>");
  return 2;
}

Directory.CreateDirectory(outputRoot);
string result = scenario switch
{
  "SC026" => RunSc026WorldSlice(),
  _ => throw new InvalidOperationException($"Unknown scenario '{scenario}'.")
};

string resultPath = Path.Combine(outputRoot, $"{scenario}-world-slice-result.md");
File.WriteAllText(resultPath, result);
Console.WriteLine("SC026 P03 world and WorldFile tile encoder slice PASS");
Console.WriteLine($"Evidence: {resultPath}");
return 0;

static string RunSc026WorldSlice()
{
  WorldFacts facts = WorldFacts.CreateVersion4();
  VerifyWorldFileTilePayloadEncoder(facts);
  WorldGeometry geometry = new(600, 450);
  TileCoordinate spawn = new(300, 225);
  WorldHeaderFacts headerFacts = CreateHeaderFacts();
  WorldHeaderSnapshot header = new(
    "P03 fixed SC026 world",
    geometry,
    spawn,
    Guid.Parse("7A4B6B7C-4F08-4A8B-9A2B-9B2D3A1E6C40"),
    dayTime: true,
    time: 1234,
    moonPhase: 2,
    worldId: 26026,
    headerFacts);

  bool oversizedGeometryRejected = false;
  try
  {
    _ = new WorldHeaderSnapshot(
      "P03 oversized geometry",
      new WorldGeometry(5001, 5000),
      new TileCoordinate(1, 1),
      Guid.Parse("CA3B48F5-4A5E-4D63-B99D-FB6B3E2AA026"),
      dayTime: true,
      time: 0,
      moonPhase: 0,
      worldId: 26027,
      headerFacts);
  }
  catch (ArgumentOutOfRangeException)
  {
    oversizedGeometryRejected = true;
  }

  Require(oversizedGeometryRejected,
    "A world geometry above the bounded tile budget was accepted.");

  WorldTile[] tiles = CreateFixedTiles(geometry);
  var owner = new WorldOwner(facts, new ScenarioLiquidRandomSource());
  var request = new WorldPreparationRequest(header, tiles, facts.Fingerprint);
  tiles[Index(geometry, new TileCoordinate(300, 225))] = WorldTile.Empty;
  WorldPreparedWorld prepared = owner.PrepareWorld(request);
  WorldValidationResult validation = owner.ValidatePreparedWorld(prepared);
  Require(validation.IsValid, string.Join("; ", validation.Errors));

  WorldPublicationResult publication = owner.CommitPreparedWorld(prepared);
  Require(publication.Published, publication.Rejection ?? "World publication failed.");
  ulong worldEpoch = publication.WorldEpoch;
  WorldSnapshot initial = owner.CaptureSnapshot();
  Require(initial.WorldEpoch == worldEpoch, "Published world epoch was not captured.");
  Require(initial.Header.Geometry == geometry, "Published world geometry changed.");
  Require(initial.GetTile(new TileCoordinate(300, 225)).Type == 1,
    "The fixed ordinary spawn tile was not published.");
  Require(initial.GetTile(new TileCoordinate(300, 230)).Type == 3,
    "The fixed frame-important tile was not published.");
  Require(initial.DirtySections.Count == 9,
    "The 600x450 world must publish all nine baseline sections as dirty.");

  WorldDirtyBatch initialDirty = owner.CaptureDirtySections();
  WorldPreparedWorld stalePrepared = owner.PrepareWorld(request);
  WorldMutationResult ordinaryTilePatch = owner.ApplyTileDeltas(
    worldEpoch,
    [new KeyValuePair<TileCoordinate, WorldTileDelta>(
      new TileCoordinate(300, 225),
      new WorldTileDelta(Wall: 1, FrameX: 12, FrameY: 8))],
    producesImmediateBroadcast: false);
  Require(ordinaryTilePatch.Applied,
    ordinaryTilePatch.Rejection ?? "The ordinary tile boundary patch was rejected.");
  WorldSnapshot afterOrdinaryPatch = owner.CaptureSnapshot();
  WorldTile patchedOrdinaryTile = afterOrdinaryPatch.GetTile(new TileCoordinate(300, 225));
  Require(patchedOrdinaryTile.FrameX == -1 && patchedOrdinaryTile.FrameY == -1,
    "A non-frame-important tile retained a frame coordinate after mutation.");
  WorldValidationResult staleValidation = owner.ValidatePreparedWorld(stalePrepared);
  Require(!staleValidation.IsValid,
    "A world candidate prepared before a newer commit remained valid.");
  WorldPublicationResult stalePublication = owner.CommitPreparedWorld(stalePrepared);
  Require(!stalePublication.Published,
    "A stale world candidate was published over a newer tile commit.");
  Require(owner.AcknowledgeDirtySections(initialDirty, initialDirty.Sections),
    "The initial dirty section batch was not acknowledged.");
  WorldDirtyBatch remainingDirty = owner.CaptureDirtySections();
  Require(remainingDirty.Sections.Count == 1 &&
          remainingDirty.Sections.Contains(new WorldSectionCoordinate(1, 1)),
    "A newer section change was incorrectly cleared by an older dirty batch.");

  IReadOnlyList<WorldSectionCoordinate> crossingSections = geometry.GetIntersectingSections(
    new WorldSectionBounds(199, 149, 2, 2));
  Require(crossingSections.SequenceEqual([
      new WorldSectionCoordinate(0, 0),
      new WorldSectionCoordinate(1, 0),
      new WorldSectionCoordinate(0, 1),
      new WorldSectionCoordinate(1, 1)]),
    "A tile rectangle crossing section edges was not ordered row-major.");
  IReadOnlyList<WorldSectionCoordinate> edgeSections = geometry
    .GetSectionsAroundPixelPosition(0, 0, radius: 1);
  Require(edgeSections.Count == 4 &&
          edgeSections.First() == new WorldSectionCoordinate(0, 0) &&
          edgeSections.Last() == new WorldSectionCoordinate(1, 1),
    "The world-edge section neighborhood was not clipped deterministically.");

  IReadOnlyList<WorldSectionCoordinate> plannedSections = geometry
    .GetSectionsAroundPixelPosition(spawn.X * 16, spawn.Y * 16, radius: 1);
  Require(plannedSections.Count == 9, "The spawn neighborhood must contain nine sections.");
  WorldSectionSnapshot section = owner.CaptureSection(new WorldSectionCoordinate(1, 1));
  Require(section.WorldEpoch == worldEpoch, "Section snapshot lost its world epoch.");
  Require(section.Bounds == new WorldSectionBounds(200, 150, 200, 150),
    "The spawn section bounds are incorrect.");
  Require(section.GetTile(100, 75).Type == 1, "Section row-major tile order is incorrect.");
  Require(section.GetTile(100, 80).Type == 3,
    "Section frame-important tile content is missing.");

  WorldSectionPlan transferPlan = owner.PlanInitialSections(
    playerSpawn: null,
    additionalSpawns: Array.Empty<TileCoordinate>(),
    portalSections: Array.Empty<WorldSectionCoordinate>());
  var transferCoordinator = new WorldSectionTransferCoordinator(owner);
  WorldSectionTransferStartResult transferStart = transferCoordinator.StartSession(
    SessionEpoch.Create(26026),
    transferPlan,
    Guid.Parse("7B4B6B7C-4F08-4A8B-9A2B-9B2D3A1E6C40"));
  Require(transferStart.Accepted,
    $"The World section transfer session was not accepted: {transferStart.Status}.");

  WorldSectionTransferToken transferToken = transferStart.Token;
  WorldSectionTransferCaptureResult transferCapture =
    transferCoordinator.CaptureInitialBaseline(transferToken);
  Require(transferCapture.Status == WorldSectionTransferStatus.Captured &&
          transferCapture.Snapshots.Count == transferPlan.Sections.Count &&
          transferCapture.CommonVersion == transferPlan.Version,
    "The initial section baseline was not captured at one plan version.");

  WorldSectionCoordinate firstTransferSection = transferPlan.Sections[0];
  WorldSectionCoordinate secondTransferSection = transferPlan.Sections[1];
  WorldLiquidDeltaQueueResult blockedDelta = transferCoordinator.QueueLiquidDelta(
    transferToken,
    [firstTransferSection],
    transferPlan.Version);
  Require(blockedDelta.Status == WorldSectionTransferStatus.BaselinePending,
    "A liquid delta crossed the section baseline barrier before queueing.");

  WorldSectionTransferQueueResult firstBaseline = transferCoordinator.QueueBaseline(
    transferToken,
    [firstTransferSection]);
  Require(firstBaseline.Status == WorldSectionTransferStatus.Queued,
    "A captured section baseline was not queued.");
  WorldLiquidDeltaQueueResult firstDelta = transferCoordinator.QueueLiquidDelta(
    transferToken,
    [firstTransferSection],
    transferPlan.Version);
  Require(firstDelta.Status == WorldSectionTransferStatus.Queued,
    "A queued section did not admit its later liquid delta.");

  WorldLiquidDeltaQueueResult secondBlockedDelta = transferCoordinator.QueueLiquidDelta(
    transferToken,
    [secondTransferSection],
    transferPlan.Version);
  Require(secondBlockedDelta.Status == WorldSectionTransferStatus.BaselinePending,
    "A captured-but-not-queued section admitted a liquid delta.");
  Require(transferCoordinator.RecordSendOutcome(
      transferToken,
      firstTransferSection,
      WorldSectionTransferSendOutcome.NotSubmitted) ==
    WorldSectionTransferStatus.NotSubmitted,
    "A not-submitted baseline was not returned to the retryable state.");
  Require(transferCoordinator.TryGetSectionState(
      transferToken,
      firstTransferSection,
      out WorldSectionTransferState retryState) &&
    retryState == WorldSectionTransferState.Captured,
    "A not-submitted baseline did not retain its captured snapshot.");
  Require(transferCoordinator.QueueBaseline(transferToken, [firstTransferSection]).Status ==
    WorldSectionTransferStatus.Queued,
    "A captured baseline could not be retried without recapturing the world.");
  Require(transferCoordinator.RecordSendOutcome(
      transferToken,
      firstTransferSection,
      WorldSectionTransferSendOutcome.LocallySent) ==
    WorldSectionTransferStatus.LocallySent,
    "A queued baseline did not reach the locally-sent state.");
  Require(transferCoordinator.QueueBaseline(transferToken, [secondTransferSection]).Status ==
    WorldSectionTransferStatus.Queued,
    "The second captured baseline was not queued.");
  Require(transferCoordinator.RecordSendOutcome(
      transferToken,
      secondTransferSection,
      WorldSectionTransferSendOutcome.OutcomeUnknown) ==
    WorldSectionTransferStatus.OutcomeUnknown &&
    !transferCoordinator.IsSessionValid(transferToken),
    "OutcomeUnknown did not invalidate the section transfer session.");

  var requestCoordinator = new WorldSectionTransferCoordinator(
    owner,
    new WorldSectionTransferOptions(
      maximumPendingSections: 16,
      maximumRequestBatchSections: 2));
  WorldSectionTransferStartResult requestStart = requestCoordinator.StartSession(
    SessionEpoch.Create(26027),
    transferPlan,
    Guid.Parse("7C4B6B7C-4F08-4A8B-9A2B-9B2D3A1E6C40"));
  Require(requestStart.Accepted,
    $"The Packet 159 boundary session was not accepted: {requestStart.Status}.");
  WorldSection159RequestResult repeatedRequest = requestCoordinator.HandleRequestSection159(
    requestStart.Token,
    (ushort)transferPlan.Sections[0].X,
    (ushort)transferPlan.Sections[0].Y);
  Require(repeatedRequest.Status == WorldSectionTransferStatus.AlreadyScheduled,
    "A Packet 159 request for a planned section was not idempotently classified.");
  WorldSection159RequestBatchResult duplicateRequest =
    requestCoordinator.HandleRequestSection159Batch(
      requestStart.Token,
      [((ushort)transferPlan.Sections[0].X, (ushort)transferPlan.Sections[0].Y),
       ((ushort)transferPlan.Sections[0].X, (ushort)transferPlan.Sections[0].Y)]);
  Require(duplicateRequest.Status == WorldSectionTransferStatus.DuplicateRequest,
    "Duplicate Packet 159 coordinates were not rejected atomically.");
  WorldSection159RequestResult invalidRequest = requestCoordinator.HandleRequestSection159(
    requestStart.Token,
    ushort.MaxValue,
    ushort.MaxValue);
  Require(invalidRequest.Status == WorldSectionTransferStatus.InvalidSection,
    "An out-of-world UInt16 Packet 159 coordinate was accepted.");
  WorldSection159RequestBatchResult oversizedRequest =
    requestCoordinator.HandleRequestSection159Batch(
      requestStart.Token,
      [(0, 0), (1, 0), (2, 0)]);
  Require(oversizedRequest.Status == WorldSectionTransferStatus.BudgetExceeded,
    "A Packet 159 request batch above the configured budget was accepted.");

  WorldMutationResult moduleUpdate = owner.ApplyLiquidModuleValues(
    worldEpoch,
    [new LiquidUpdate(new TileCoordinate(301, 225), 90, LiquidType.Water)],
    maximumUpdates: 32);
  Require(moduleUpdate.Applied && !moduleUpdate.ProducesImmediateBroadcast,
    "Module 0 liquid assignment must commit without an immediate broadcast.");
  Require(owner.CaptureDirtyLiquidValues(32).Values.Count == 0,
    "Module 0 direct assignment must not create an immediate liquid network delta.");

  var tickCoordinator = new WorldTickCoordinator(owner);
  WorldTickResult tick = tickCoordinator.Advance(
    worldEpoch,
    tick: 0,
    elapsedGameTicks: 0,
    dayRate: 0,
    liquidOperationBudget: 32);
  Require(tick.Applied, tick.Rejection ?? "The first world tick was rejected.");
  WorldSnapshot afterLiquidTick = owner.CaptureSnapshot();
  int firstLiquidY = -1;
  int liquidTotal = 0;
  for (int y = 0; y < geometry.Height; y++)
  {
    byte amount = afterLiquidTick.GetTile(new TileCoordinate(301, y)).LiquidAmount;
    liquidTotal += amount;
    if (amount > 0 && firstLiquidY < 0)
    {
      firstLiquidY = y;
    }
  }

  Require(firstLiquidY > 225 && liquidTotal == 90,
    $"The bounded liquid step did not preserve and move the fixed source; " +
    $"firstY={firstLiquidY}, total={liquidTotal}, changed={tick.ChangedLiquidTiles.Count}.");

  WorldLiquidDirtyBatch liquidDirty = owner.CaptureDirtyLiquidValues(32);
  Require(liquidDirty.Values.Count > 0,
    "The bounded liquid tick did not expose a dirty liquid batch.");
  LiquidDirtyValue replacedLiquid = liquidDirty.Values[0];
  byte replacementAmount = replacedLiquid.Amount == 0 ? (byte)1 : (byte)0;
  WorldMutationResult newerLiquidMutation = owner.ApplyTileDeltas(
    worldEpoch,
    [new KeyValuePair<TileCoordinate, WorldTileDelta>(
      replacedLiquid.Coordinate,
      new WorldTileDelta(LiquidAmount: replacementAmount))],
    producesImmediateBroadcast: false);
  Require(newerLiquidMutation.Applied,
    newerLiquidMutation.Rejection ?? "The newer liquid mutation was rejected.");
  Require(owner.AcknowledgeDirtyLiquidValues(liquidDirty),
    "The captured liquid dirty batch was not acknowledged.");
  WorldLiquidDirtyBatch retainedLiquidDirty = owner.CaptureDirtyLiquidValues(32);
  Require(retainedLiquidDirty.Values.Any(value =>
      value.Coordinate == replacedLiquid.Coordinate && value.Version != replacedLiquid.Version),
    "A newer liquid value was incorrectly cleared by an older dirty batch.");

  WorldSectionPlan liquidProjectionPlan = owner.PlanInitialSections(
    playerSpawn: null,
    additionalSpawns: Array.Empty<TileCoordinate>(),
    portalSections: Array.Empty<WorldSectionCoordinate>());
  var liquidTransfer = new WorldSectionTransferCoordinator(owner);
  WorldSectionTransferStartResult liquidTransferStart = liquidTransfer.StartSession(
    SessionEpoch.Create(26028),
    liquidProjectionPlan,
    Guid.Parse("7D4B6B7C-4F08-4A8B-9A2B-9B2D3A1E6C40"));
  Require(liquidTransferStart.Accepted,
    $"The liquid projection transfer session was not accepted: {liquidTransferStart.Status}.");
  Require(liquidTransfer.CaptureInitialBaseline(liquidTransferStart.Token).Status ==
    WorldSectionTransferStatus.Captured,
    "The liquid projection baseline was not captured.");
  Require(liquidTransfer.QueueBaseline(
      liquidTransferStart.Token,
      liquidProjectionPlan.Sections).Status == WorldSectionTransferStatus.Queued,
    "The liquid projection baseline was not queued.");
  WorldLiquidProjectionResult liquidProjection = new WorldLiquidSnapshotProjection(
    liquidTransfer).QueueDirtyBatch(liquidTransferStart.Token, retainedLiquidDirty);
  Require(liquidProjection.Status == WorldSectionTransferStatus.Queued &&
          liquidProjection.Sections.Count > 0 &&
          liquidProjection.Version == retainedLiquidDirty.Version,
    "The liquid dirty batch did not project onto queued section baselines.");
  Require(liquidTransfer.TryGetPendingLiquidDeltaVersion(
      liquidTransferStart.Token,
      liquidProjection.Sections[0],
      out CommitVersion projectedDeltaVersion) &&
    projectedDeltaVersion == retainedLiquidDirty.Version,
    "The liquid projection did not retain its latest section version.");

  WorldSnapshot beforeInvalidBudget = owner.CaptureSnapshot();
  WorldTickResult invalidBudget = tickCoordinator.Advance(
    worldEpoch,
    tick: 1,
    elapsedGameTicks: 54000,
    dayRate: 1,
    liquidOperationBudget: 0);
  Require(!invalidBudget.Applied, "An invalid liquid budget must be rejected.");
  Require(owner.CaptureSnapshot().Version == beforeInvalidBudget.Version,
    "An invalid liquid budget must not commit world time first.");

  WorldTickResult duplicateTick = tickCoordinator.Advance(
    worldEpoch,
    tick: 0,
    elapsedGameTicks: 0,
    dayRate: 0,
    liquidOperationBudget: 32);
  Require(!duplicateTick.Applied && duplicateTick.Rejection is not null,
    "A duplicate world tick must be rejected.");

  WorldTimeState dayEndpoint = WorldTimeSystem.Advance(
    new WorldTimeState(true, WorldTimeState.DayLength - 1, 2),
    elapsedGameTicks: 1,
    dayRate: 1);
  Require(dayEndpoint == new WorldTimeState(true, WorldTimeState.DayLength, 2),
    "Equality at the day endpoint must remain daytime.");
  WorldTimeState nightBoundary = WorldTimeSystem.Advance(dayEndpoint, 1, 1);
  Require(nightBoundary == new WorldTimeState(false, 0, 2),
    "Strict day overflow must switch to night and reset time.");
  WorldTimeState nightEndpoint = WorldTimeSystem.Advance(
    new WorldTimeState(false, WorldTimeState.NightLength - 1, 7),
    elapsedGameTicks: 1,
    dayRate: 1);
  Require(nightEndpoint == new WorldTimeState(false, WorldTimeState.NightLength, 7),
    "Equality at the night endpoint must remain nighttime without advancing the moon.");
  Require(WorldTimeSystem.Advance(nightEndpoint, 123, 2) == new WorldTimeState(true, 0, 0),
    "Night overflow must discard excess time and wrap moon phase seven to zero.");
  Require(WorldTimeSystem.Advance(dayEndpoint, 0, 1) == dayEndpoint &&
          WorldTimeSystem.Advance(nightEndpoint, 123, 0) == nightEndpoint,
    "Zero elapsed time and zero day rate must freeze time even at a period endpoint.");
  VerifyWorldTickCaptures(facts, header);

  WorldPublicationResult unload = owner.UnloadWorld();
  Require(unload.Published && unload.WorldEpoch == worldEpoch + 1,
    "World unload did not advance the world epoch.");
  Require(!owner.IsCurrentWorldEpoch(worldEpoch),
    "The unloaded world epoch remained current.");
  WorldMutationResult staleEpochMutation = owner.ApplyLiquidModuleValues(
    worldEpoch,
    [new LiquidUpdate(new TileCoordinate(301, 225), 1, LiquidType.Water)],
    maximumUpdates: 1);
  Require(!staleEpochMutation.Applied,
    "A mutation using an unloaded world epoch was accepted.");
  bool dirtyCaptureRejected = false;
  try
  {
    owner.CaptureDirtySections();
  }
  catch (InvalidOperationException)
  {
    dirtyCaptureRejected = true;
  }

  Require(dirtyCaptureRejected,
    "Dirty sections were captured from an unloaded world.");

  return """
# SC026 P03 world-slice evidence

Status: PASS for the P03 world-owner preflight; the full network SC026 remains pending.

Fixed input: one deterministic 600x450 Version4 facts world, spawn `(300,225)`,
one ordinary tile, one frame-important tile, and one water source. The world
published nine baseline sections. The spawn neighborhood returned nine sections;
section `(1,1)` preserved the 200x150 bounds, world epoch, row-major tile order,
ordinary tile and frame-important tile. A geometry above the 25,000,000-tile
budget was rejected before any tile map allocation.

Observed: module 0 direct liquid assignment committed without an immediate
liquid broadcast/delta; the next bounded world tick moved the source downward.
An invalid liquid budget did not commit time first and duplicate tick 0 was rejected.
Equality at both period endpoints retained the current period; strict overflow
reset time and discarded excess, frozen updates retained endpoints, and moon phase
seven wrapped to zero. A huge single update produced at most one morning. Published
endpoint headers were valid. Same-revision captures committed; foreign-owner, stale
version/epoch, invalid/overflowing time and invalid-budget captures were rejected
without changing header, tiles, version, section dirtiness or liquid dirtiness.
Rejection left the same tick available for a valid retry, and a successful frozen
tick retained its actual version. The prepared
world retained its defensive tile copy. A non-frame-important tile mutation was
normalized to no frame coordinates, and acknowledging an older dirty batch retained
a newer section change. Unloading advanced the epoch so a stale liquid mutation and
dirty capture were rejected.

This evidence covers the P03 world store, section snapshot, direct-liquid,
section-transfer barrier, bounded Packet 159 request boundary, liquid dirty
projection and world-time boundary. It does not claim TCP handshake, Packet 7/10
wire exchange, P02 admission, P04/P05/P06/P07 attachment capture, or full SC026
client spawn. The runtime WorldFile tile encoder also passed field round-trip,
frame-important, timer-frame normalization, slope policy, high tile/wall id and
compression-batching checks.
SC027-SC040 remain unrun.
""";
}

static void VerifyWorldFileTilePayloadEncoder(WorldFacts facts)
{
  WorldGeometry geometry = new(3, 4);
  var tiles = Enumerable.Repeat(WorldTile.Empty, geometry.TileCount).ToArray();
  tiles[0] = WorldTile.Empty with
  {
    Active = true,
    Type = 1,
    Wall = 366,
    FrameX = -1,
    FrameY = -1,
    LiquidAmount = 200,
    LiquidType = LiquidType.Shimmer,
    TileColor = 31,
    WallColor = 31,
    Wires = TileWireMask.Red | TileWireMask.Blue | TileWireMask.Green | TileWireMask.Yellow,
    Actuator = true,
    Inactive = true,
    HalfBlock = true,
    TileHeader3 = 0xe0,
    WallHeader = 0x80
  };
  tiles[1] = WorldTile.Empty with
  {
    Active = true,
    Type = 3,
    FrameX = 18,
    FrameY = -18,
    TileColor = 5
  };
  tiles[2] = WorldTile.Empty with
  {
    Active = true,
    Type = 144,
    FrameX = 36,
    FrameY = 9
  };
  tiles[3] = WorldTile.Empty with
  {
    Active = true,
    Type = 3,
    FrameX = 0,
    FrameY = 0,
    Slope = 4
  };
  tiles[4] = WorldTile.Empty with
  {
    Active = true,
    Type = 520,
    FrameX = -1,
    FrameY = -1
  };

  WorldFileTilePayloadEncoder encoder = new();
  WorldFileTilePayloadDecoder decoder = new();
  WorldFileTilePayloadSection encoded = encoder.Encode(geometry, tiles, facts);
  IReadOnlyList<WorldTile> decoded = decoder.Decode(encoded, facts);
  WorldTile[] expected = (WorldTile[])tiles.Clone();
  expected[2] = expected[2] with { FrameY = 0 };
  expected[3] = expected[3] with { Slope = 0 };
  Require(decoded.SequenceEqual(expected),
    "The runtime WorldFile tile encoder did not preserve normalized Version4 tile fields.");

  WorldGeometry batchGeometry = new(1, 2);
  WorldTile repeatedEmpty = WorldTile.Empty;
  WorldFileTilePayloadSection batched = encoder.Encode(
    batchGeometry,
    [repeatedEmpty, repeatedEmpty],
    facts);
  Require(batched.CompressedPayload.Length == 2 &&
      batched.CompressedPayload.Span[0] == 0x40 && batched.CompressedPayload.Span[1] == 1,
    "A batchable vertical tile run was not encoded with one run-length field.");

  WorldTile repeatedUnbatchable = WorldTile.Empty with
  {
    Active = true,
    Type = 520,
    FrameX = -1,
    FrameY = -1
  };
  WorldFileTilePayloadSection unbatched = encoder.Encode(
    batchGeometry,
    [repeatedUnbatchable, repeatedUnbatchable],
    facts);
  Require(unbatched.CompressedPayload.Length == 14 &&
      unbatched.CompressedPayload.Span[0] == 0x22 &&
      unbatched.CompressedPayload.Span[7] == 0x22,
    "A tile type that forbids save batching was run-length encoded.");

  WorldTile inactiveUnbatchable = WorldTile.Empty with { Type = 520 };
  WorldFileTilePayloadSection inactiveUnbatched = encoder.Encode(
    batchGeometry,
    [inactiveUnbatchable, inactiveUnbatchable],
    facts);
  Require(inactiveUnbatched.CompressedPayload.Length == 2 &&
      inactiveUnbatched.CompressedPayload.Span[0] == 0 &&
      inactiveUnbatched.CompressedPayload.Span[1] == 0,
    "An inactive tile's original type did not control save batching.");

  bool invalidWallRejected = false;
  try
  {
    _ = encoder.Encode(
      new WorldGeometry(1, 1),
      [WorldTile.Empty with { Wall = WorldFacts.WallCount }],
      facts);
  }
  catch (InvalidDataException)
  {
    invalidWallRejected = true;
  }

  Require(invalidWallRejected, "A wall outside the Version4 facts table was encoded.");

  using var cancelled = new CancellationTokenSource();
  cancelled.Cancel();
  bool cancellationObserved = false;
  try
  {
    _ = encoder.Encode(new WorldGeometry(1, 1), [WorldTile.Empty], facts, cancelled.Token);
  }
  catch (OperationCanceledException)
  {
    cancellationObserved = true;
  }

  Require(cancellationObserved, "Tile payload encoding ignored cancellation.");
}

static void VerifyWorldTickCaptures(WorldFacts facts, WorldHeaderSnapshot template) {
  var owner = new WorldOwner(facts, new ScenarioLiquidRandomSource());
  var coordinator = new WorldTickCoordinator(owner);
  PublishTickWorld(owner, template, dayTime: true, WorldTimeState.DayLength, moonPhase: 2);
  WorldTickCapture dayCapture = owner.CaptureWorldTick();
  Require(dayCapture.Time == new WorldTimeState(true, WorldTimeState.DayLength, 2) &&
          ReferenceEquals(dayCapture.EnvironmentFacts, template.Facts) &&
          ReferenceEquals(dayCapture.Facts, facts),
    "A published endpoint capture lost its time or authoritative facts.");
  WorldTickResult frozen = coordinator.Advance(dayCapture, 0, 1, 0, 32);
  Require(frozen.Applied && !frozen.TimeChanged && !frozen.MorningTransition &&
          frozen.MorningTransitionCount == 0 && frozen.SourceVersion == dayCapture.Version &&
          frozen.Version == dayCapture.Version,
    "A frozen successful tick must retain the source revision and report no morning.");
  WorldTickResult hugeDay = coordinator.Advance(owner.CaptureWorldTick(), 1, 1_000_000, 1, 32);
  Require(hugeDay.Applied && hugeDay.MorningTransitionCount == 0 &&
          owner.CaptureWorldTick().Time == new WorldTimeState(false, 0, 2),
    "One huge daytime update must switch only to night and discard excess.");

  PublishTickWorld(owner, template, dayTime: false, WorldTimeState.NightLength, moonPhase: 7);
  WorldTickCapture nightCapture = owner.CaptureWorldTick();
  var foreign = new WorldOwner(facts);
  PublishTickWorld(foreign, template, dayTime: false, WorldTimeState.NightLength, moonPhase: 7);
  WorldSnapshot beforeRejections = owner.CaptureSnapshot();
  WorldLiquidDirtyBatch beforeLiquids = owner.CaptureDirtyLiquidValues(32);
  RequireRejectedTick(owner, beforeRejections, beforeLiquids,
    coordinator.Advance(foreign.CaptureWorldTick(), 0, 1, 1, 32));
  RequireRejectedTick(owner, beforeRejections, beforeLiquids,
    coordinator.Advance(nightCapture, 0, 1, 1, 0));
  foreach (double invalidElapsed in new[] { -1, double.NaN, double.PositiveInfinity,
      double.NegativeInfinity, double.MaxValue }) {
    RequireRejectedTick(owner, beforeRejections, beforeLiquids,
      coordinator.Advance(nightCapture, 0, invalidElapsed, 2, 32));
  }
  RequireRejectedTick(owner, beforeRejections, beforeLiquids,
    coordinator.Advance(nightCapture, 0, 1, -1, 32));

  WorldMutationResult intervening = owner.ApplyTileDeltas(
    nightCapture.WorldEpoch,
    [new KeyValuePair<TileCoordinate, WorldTileDelta>(template.Spawn,
      new WorldTileDelta(Wall: 1))],
    producesImmediateBroadcast: false);
  Require(intervening.Applied, "The intervening revision change was rejected.");
  WorldSnapshot afterIntervening = owner.CaptureSnapshot();
  WorldLiquidDirtyBatch afterInterveningLiquids = owner.CaptureDirtyLiquidValues(32);
  RequireRejectedTick(owner, afterIntervening, afterInterveningLiquids,
    coordinator.Advance(nightCapture, 0, 1, 1, 32));
  WorldTickCapture current = owner.CaptureWorldTick();
  WorldTickResult hugeNight = coordinator.Advance(current, 0, 1_000_000, 1, 32);
  Require(hugeNight.Applied && hugeNight.MorningTransition &&
          hugeNight.MorningTransitionCount == 1 && hugeNight.SourceVersion == current.Version &&
          hugeNight.Version == owner.CaptureWorldTick().Version &&
          hugeNight.Version.Value > current.Version.Value &&
          owner.CaptureWorldTick().Time == new WorldTimeState(true, 0, 0),
    hugeNight.Rejection ??
      "A valid same-tick retry must commit exactly one morning at the actual new revision.");
  RequireRejectedTick(owner, owner.CaptureSnapshot(), owner.CaptureDirtyLiquidValues(32),
    coordinator.Advance(owner.CaptureWorldTick(), 0, 1, 1, 32));

  WorldTickCapture oldEpoch = owner.CaptureWorldTick();
  PublishTickWorld(owner, template, dayTime: false, WorldTimeState.NightLength, moonPhase: 7);
  RequireRejectedTick(owner, owner.CaptureSnapshot(), owner.CaptureDirtyLiquidValues(32),
    coordinator.Advance(oldEpoch, 1, 1, 1, 32));
  WorldTickResult newEpoch = coordinator.Advance(owner.CaptureWorldTick(), 0, 1, 1, 32);
  Require(newEpoch.Applied && newEpoch.MorningTransitionCount == 1,
    "A rejected stale epoch must not consume tick zero of the new published world.");
}

static void PublishTickWorld(
    WorldOwner owner,
    WorldHeaderSnapshot template,
    bool dayTime,
    double time,
    byte moonPhase) {
  var header = new WorldHeaderSnapshot(template.Name, template.Geometry, template.Spawn,
    template.UniqueId, dayTime, time, moonPhase, template.WorldId, template.Facts);
  var tiles = Enumerable.Repeat(WorldTile.Empty, header.Geometry.TileCount).ToArray();
  WorldPreparedWorld prepared = owner.PrepareWorld(
    new WorldPreparationRequest(header, tiles, owner.Facts.Fingerprint));
  WorldPublicationResult publication = owner.CommitPreparedWorld(prepared);
  Require(publication.Published, publication.Rejection ?? "Endpoint world publication failed.");
}

static void RequireRejectedTick(
    WorldOwner owner,
    WorldSnapshot before,
    WorldLiquidDirtyBatch beforeLiquids,
    WorldTickResult result) {
  Require(!result.Applied && result.Rejection is not null && !result.TimeChanged &&
          result.MorningTransitionCount == 0 && result.MergeActions.Count == 0 &&
          result.ChangedLiquidTiles.Count == 0,
    "An invalid capture/input must reject without reporting committed effects.");
  WorldSnapshot after = owner.CaptureSnapshot();
  WorldLiquidDirtyBatch afterLiquids = owner.CaptureDirtyLiquidValues(32);
  Require(after.WorldEpoch == before.WorldEpoch && after.Version == before.Version &&
          ReferenceEquals(after.Header, before.Header) && after.Tiles.SequenceEqual(before.Tiles) &&
          after.DirtySections.SequenceEqual(before.DirtySections) &&
          afterLiquids.Values.SequenceEqual(beforeLiquids.Values),
    "A rejected world tick changed published world state or dirtiness.");
}

static WorldHeaderFacts CreateHeaderFacts()
{
  return new WorldHeaderFacts(
    seedText: "P03-SC026-fixed",
    worldGeneratorVersion: 1,
    leftWorld: 0,
    rightWorld: 9600,
    topWorld: 0,
    bottomWorld: 7200,
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
    treeX: [100, 300, 500],
    treeStyle: [0, 1, 2, 3],
    caveBackX: [120, 300, 480],
    caveBackStyle: [0, 1, 2, 3],
    iceBackStyle: 0,
    jungleBackStyle: 0,
    hellBackStyle: 0,
    worldSurface: 180,
    rockLayer: 300,
    bloodMoon: false,
    eclipse: false,
    dungeonX: 500,
    dungeonY: 250,
    crimson: false);
}

static WorldTile[] CreateFixedTiles(WorldGeometry geometry)
{
  var tiles = Enumerable.Repeat(WorldTile.Empty, geometry.TileCount).ToArray();
  tiles[Index(geometry, new TileCoordinate(300, 225))] = WorldTile.Empty with
  {
    Active = true,
    Type = 1,
    FrameX = -1,
    FrameY = -1
  };
  tiles[Index(geometry, new TileCoordinate(300, 230))] = WorldTile.Empty with
  {
    Active = true,
    Type = 3,
    FrameX = 18,
    FrameY = 0
  };
  tiles[Index(geometry, new TileCoordinate(301, 225))] = WorldTile.Empty.WithLiquid(
    200,
    LiquidType.Water);
  return tiles;
}

static int Index(WorldGeometry geometry, TileCoordinate coordinate)
{
  return checked((coordinate.Y * geometry.Width) + coordinate.X);
}

static bool TryReadArguments(
  IReadOnlyList<string> args,
  out string scenario,
  out string outputRoot)
{
  scenario = string.Empty;
  outputRoot = string.Empty;
  if (args.Count != 4
    || args[0] != "--scenario"
    || args[2] != "--output-root"
    || string.IsNullOrWhiteSpace(args[1])
    || string.IsNullOrWhiteSpace(args[3]))
  {
    return false;
  }

  scenario = args[1];
  outputRoot = Path.GetFullPath(args[3]);
  return scenario == "SC026";
}

static void Require(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

sealed class ScenarioLiquidRandomSource : ILiquidRandomSource {
  private readonly Random _random = new(26026);

  public int Next(int maximumExclusive) {
    return _random.Next(maximumExclusive);
  }
}
