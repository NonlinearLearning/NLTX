using Terraria.DeathPenaltyAndRevenge;
using Terraria.Items;
using Terraria.Npc;
using Terraria.Relationships;
using Terraria.Teleportation;
using Terraria.WorldInteraction.Wiring;
using Terraria.WorldStorage;
using WiringTileCoordinate = Terraria.WorldInteraction.Tiles.TileCoordinate;

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

static PylonRegistryEntry Pylon(
  int x,
  int y,
  byte kind,
  int entityId,
  bool isValid = true)
{
  return new PylonRegistryEntry(
    new TileCoordinate(x, y),
    kind,
    new TileEntityId(entityId),
    isValid);
}

LiquidFlowBudgetPolicy policy = new(
  maximumLiquid: 100,
  cyclesPerUpdate: 10);
LiquidFlowBudgetDecision budget = LiquidFlowBudgetQuery.Evaluate(
  policy,
  activeLiquidCount: 20,
  requestedWorkBudget: 50,
  bufferedLiquidCount: 0,
  panicCounter: 0,
  panicMode: false,
  quickSettle: false);
Assert(
  budget.AvailableWorkBudget == 50 && budget.CanConsume &&
    !budget.ShouldEnterPanic,
  "Liquid budget must cap work by remaining capacity without entering panic.");

LiquidFlowBudgetDecision panicBudget = LiquidFlowBudgetQuery.Evaluate(
  policy,
  activeLiquidCount: 20,
  requestedWorkBudget: 50,
  bufferedLiquidCount: LiquidFlowBudgetPolicy.PanicBufferHighWaterMark,
  panicCounter: LiquidFlowBudgetPolicy.PanicStartAfterTicks + 1,
  panicMode: false,
  quickSettle: false);
Assert(
  panicBudget.ShouldEnterPanic && panicBudget.NextPanicCounter ==
    LiquidFlowBudgetPolicy.PanicStartAfterTicks + 2,
  "Liquid high-water input must advance the panic counter and request panic after the threshold.");

LiquidWorkQueueState queueState = new() { Capacity = 2 };
LiquidWorkQueueSystem queue = new(
  new LiquidWorkQueueStateCommitPort(queueState));
LiquidCellWorkItemStateCommand delayed = new(4, 5, kill: 0, delay: 2);
Assert(
  queue.Enqueue(in delayed, worldWidth: 20, worldHeight: 20).Succeeded,
  "A bounded liquid work item must enqueue.");
Assert(
  queue.Enqueue(in delayed, worldWidth: 20, worldHeight: 20).WasDuplicate,
  "The same liquid coordinate must not be scheduled twice.");
LiquidWorkQueueDrainResult deferred = queue.DrainNext(20, 20);
Assert(
  deferred.Status == LiquidWorkQueueDrainStatus.Deferred &&
    deferred.Item is { Delay: 1 },
  "A delayed liquid work item must decrement its delay at the queue boundary.");
LiquidCellWorkItemStateCommand doomed = new(6, 7, kill: 8, delay: 0);
Assert(queue.Enqueue(in doomed, 20, 20).Succeeded, "A second bounded item must enqueue.");
LiquidWorkQueueDrainResult deferredAgain = queue.DrainNext(20, 20);
Assert(
  deferredAgain.Status == LiquidWorkQueueDrainStatus.Deferred &&
    deferredAgain.Item is { Delay: 0 },
  "A delayed liquid work item must require one drain per remaining delay tick.");
LiquidWorkQueueDrainResult ready = queue.DrainNext(20, 20);
Assert(
  ready.Status == LiquidWorkQueueDrainStatus.Ready &&
    ready.Item is { X: 4, Y: 5, Delay: 0 },
  "The queue must preserve FIFO order and expose a ready item after its delay reaches zero.");
LiquidWorkQueueDrainResult removed = queue.DrainNext(20, 20);
Assert(
  removed.Status == LiquidWorkQueueDrainStatus.Removed &&
    removed.Item is { X: 6, Y: 7 },
  "A work item at the kill threshold must be removed at the queue boundary.");

LiquidReplicationDirtySet dirtySet = new();
LiquidChangePublicationProjection publication =
  new(dirtySet);
Assert(
  publication.RecordCommittedChange(new TileCoordinate(2, 4)) &&
    !publication.RecordCommittedChange(new TileCoordinate(2, 4)),
  "Liquid publication must deduplicate committed coordinates before projection.");
LiquidPublicationProjectionResult firstPublication =
  publication.BeginPublication(publicationEnabled: true);
Assert(
  firstPublication.Status == LiquidPublicationProjectionStatus.Ready &&
    firstPublication.Snapshot is { Count: 1, Revision: 1 },
  "The first liquid publication must swap a stable revisioned dirty snapshot.");
publication.RecordCommittedChange(new TileCoordinate(3, 4));
LiquidPublicationProjectionResult inFlight = publication.BeginPublication(true);
Assert(
  inFlight.Status == LiquidPublicationProjectionStatus.InFlight &&
    inFlight.Snapshot is { Count: 1 },
  "New liquid changes must remain pending while the prior publication is in flight.");
LiquidPublicationProjectionResult acknowledged =
  publication.AcknowledgePublication(1);
Assert(
  acknowledged.Status == LiquidPublicationProjectionStatus.Acknowledged &&
    publication.LastPublishedRevision == 1,
  "A matching publication revision must acknowledge exactly one dirty batch.");
LiquidPublicationProjectionResult secondPublication =
  publication.BeginPublication(true);
Assert(
  secondPublication.Status == LiquidPublicationProjectionStatus.Ready &&
    secondPublication.Snapshot is { Count: 1, Revision: 2 },
  "Pending liquid changes must publish in the next revision after acknowledgement.");

WiringMechanismScheduleComponent mechanismSchedule = new();
WiringDeviceCooldownComponent deviceCooldowns = new();
WiringMechanismCooldownSystem mechanisms = new(
  mechanismSchedule,
  deviceCooldowns);
WiringMechanismScheduleResult scheduled = mechanisms.Schedule(
  new WiringTileCoordinate(3, 4),
  remainingTicks: 1,
  worldWidth: 20,
  worldHeight: 20);
Assert(
  scheduled.Succeeded,
  "A bounded wiring mechanism must be schedulable.");
WiringMechanismScheduleResult duplicateSchedule = mechanisms.Schedule(
  new WiringTileCoordinate(3, 4),
  remainingTicks: 1,
  worldWidth: 20,
  worldHeight: 20);
Assert(
  !duplicateSchedule.Succeeded && duplicateSchedule.FailureReason == "duplicate",
  "A wiring mechanism must not be scheduled twice at the same coordinate.");
WiringMechanismAdvanceResult mechanismAdvance = mechanisms.Advance(
  worldWidth: 20,
  worldHeight: 20);
Assert(
  mechanismAdvance.ReadyEntries is [{ Position: var position }] &&
    position == new WiringTileCoordinate(3, 4),
  "A wiring mechanism must become ready after its cooldown reaches zero.");

WirePropagationScratchComponent wiringState = new();
WiringPropagationSystem wiring = new(
  new WiringPropagationCommitPort(wiringState));
Assert(
  wiring.Execute(WiringPropagationCommand.Initialize(), 20, 20).Succeeded,
  "Wiring propagation initialization must commit the empty scratch state.");
Assert(
  wiring.Execute(
    WiringPropagationCommand.BeginTrip(2, 3, 1, 1),
    20,
    20).Succeeded,
  "A bounded wiring trip must begin once.");
Assert(
  wiring.Execute(
    WiringPropagationCommand.BeginWireColorPass(1),
    20,
    20).Succeeded,
  "A valid wire color pass must be accepted.");
Assert(
  wiring.Execute(WiringPropagationCommand.QueueNextGate(20, 19), 20, 20)
    .Status == WiringPropagationCommitStatus.Rejected,
  "QueueNextGate must reject coordinates outside the world bounds.");
Assert(
    wiring.Execute(WiringPropagationCommand.QueueNextGate(19, 19), 20, 20)
    .Succeeded &&
    wiring.Snapshot().NextGates.Contains(new WiringTileCoordinate(19, 19)),
  "QueueNextGate must preserve a valid bounded gate coordinate in the snapshot.");
Assert(
  wiring.Execute(WiringPropagationCommand.EndTrip(), 20, 20).Succeeded,
  "A wiring trip must end through the propagation commit port.");

PylonRegistryComponent registry = new();
PylonRegistrySystem pylons = new(registry);
PylonRegistryEntry validPylon = Pylon(12, 8, kind: 3, entityId: 11);
PylonRegistryEntry invalidKind = Pylon(4, 2, kind: 0, entityId: 12);
PylonRegistryRefreshResult refresh = pylons.Refresh(
  [validPylon, invalidKind],
  refreshCooldownTicks: int.MaxValue,
  expectedRevision: 0);
Assert(
  refresh.Accepted && refresh.CurrentRevision == 1 && refresh.Added.Count == 2,
  "A pylon refresh must commit a new registry revision and preserve the discovered diff.");
PylonRegistrySnapshot pylonSnapshot = pylons.CurrentSnapshot;
Assert(
  pylonSnapshot.HasType(3) &&
    !pylonSnapshot.HasTeleportableEntry(invalidKind.Position) &&
    !PylonRegistryQuery.HasTeleportableEntry(pylonSnapshot.Entries, invalidKind.Position),
  "Pylon queries must reject invalid or zero-kind entries.");
IReadOnlyList<PylonProjectionMessage> refreshMessages =
  PylonNetworkProjection.BuildRefreshMessages(in refresh);
Assert(
  refreshMessages.Count == 1 &&
    refreshMessages[0].Kind == PylonProjectionMessageKind.Added &&
    refreshMessages[0].Entry == validPylon &&
    refreshMessages[0].Revision == 1,
  "Refresh projection must emit only valid pylon additions at the committed revision.");
IReadOnlyList<PylonProjectionMessage> joinMessages =
  PylonNetworkProjection.BuildJoinMessages(pylonSnapshot);
Assert(
  joinMessages.Count == 1 && joinMessages[0].Entry == validPylon,
  "Join projection must emit the valid committed registry snapshot only.");
PylonRegistryRefreshResult staleRefresh = pylons.Refresh(
  [],
  refreshCooldownTicks: 0,
  expectedRevision: 0);
Assert(
  staleRefresh.RejectedAsStale && !staleRefresh.Accepted,
  "A pylon refresh with a stale registry revision must not write authority.");

RevengeRespawnDecision spawnDecision = RevengeRespawnSystem.Evaluate(
  isExpired: false,
  isInvalid: false,
  intersectsPlayerOuterBox: true,
  respawnAttemptLocked: false,
  wouldBeDiscouraged: false);
Assert(
  spawnDecision.Kind == RevengeRespawnDecisionKind.RequestSpawn,
  "A valid unlocked revenge marker in the player outer box must request spawn.");
RevengeRespawnDecision unlockDecision = RevengeRespawnSystem.Evaluate(
  isExpired: false,
  isInvalid: false,
  intersectsPlayerOuterBox: false,
  respawnAttemptLocked: true,
  wouldBeDiscouraged: false);
Assert(
  unlockDecision.Kind == RevengeRespawnDecisionKind.UnlockAttempt,
  "A marker outside the player outer box must unlock its respawn attempt.");
RevengeRegistrySystem revenge = new(
  new RevengeMarkerRegistryComponent(),
  new RevengeMarkerIdAllocator(),
  new RevengeClockState());
RevengeTargetSnapshotComponent target = RevengeTargetSnapshotComponent.Capture(
  new WorldPosition(2000, 2000),
  new NpcNetId(5),
  new NpcTypeId(5),
  legacyAiStyle: 0,
  lifeFraction: 1.0f,
  coinValue: 1000,
  baseValue: 1.0f,
  spawnedFromStatue: false);
RevengeMarkerSnapshot marker = revenge.Capture(target, expiresAtGameTime: 100)!;
RevengeRegistryCommitResult committed = revenge.CommitDecision(
  marker.MarkerId,
  marker.Revision,
  spawnDecision);
Assert(
  committed.Accepted && committed.Snapshot is { RespawnAttemptLocked: true },
  "Revenge attempt state must commit against the marker revision.");
RevengeRegistryCommitResult staleCommit = revenge.CommitDecision(
  marker.MarkerId,
  marker.Revision,
  new RevengeRespawnDecision(RevengeRespawnDecisionKind.NoAction));
Assert(
  staleCommit.RejectedAsStale,
  "A second revenge commit using the old marker revision must be rejected as stale.");

var teleportRuntimeId = new EntityRuntimeId(Guid.NewGuid());
EntityReference sourceEndpoint = new(
  new EntityUuid(Guid.Parse("10000000-0000-0000-0000-000000000001")),
  teleportRuntimeId,
  EntityReferenceScope.Any);
EntityReference destinationEndpoint = new(
  new EntityUuid(Guid.Parse("10000000-0000-0000-0000-000000000002")),
  teleportRuntimeId,
  EntityReferenceScope.Any);
EntityReference player = new(
  new EntityUuid(Guid.Parse("20000000-0000-0000-0000-000000000001")),
  teleportRuntimeId,
  EntityReferenceScope.Player);
TeleportEndpointSnapshot sourceSnapshot = new(
  sourceEndpoint,
  new WiringTileCoordinate(10, 12),
  Revision: 7,
  IsActive: true);
TeleportEndpointSnapshot destinationSnapshot = new(
  destinationEndpoint,
  new WiringTileCoordinate(40, 12),
  Revision: 9,
  IsActive: true);
TeleportSubjectSnapshot playerSnapshot = new(
  player,
  PortalSubjectKind.Player,
  Revision: 4,
  IsActive: true,
  IsDead: false,
  IsTeleporting: false,
  IsTeleportationImmune: false);
TeleportCooldownSystem teleportCooldown = new();
TeleportTransitionSnapshot teleportSnapshot = new(
  sourceSnapshot,
  destinationSnapshot,
  playerSnapshot,
  teleportCooldown.Snapshot());
TeleportTransitionRequest teleportRequest = new(
  CommandId: Guid.Parse("30000000-0000-0000-0000-000000000001"),
  SourceEndpoint: sourceEndpoint,
  DestinationEndpoint: destinationEndpoint,
  Subject: player,
  SubjectKind: PortalSubjectKind.Player,
  SourcePosition: sourceSnapshot.Position,
  DestinationPosition: destinationSnapshot.Position,
  Source: TeleportSource.Mechanism,
  CooldownTicks: 3,
  ExpectedSourceRevision: sourceSnapshot.Revision,
  ExpectedDestinationRevision: destinationSnapshot.Revision,
  ExpectedSubjectRevision: playerSnapshot.Revision,
  StartedAtTick: 100);
RecordingTeleportCommitPort teleportPort = new();
TeleportTransitionCommitSystem teleportSystem = new(
  teleportPort,
  teleportCooldown);
TeleportCommitResult teleportCommit = teleportSystem.Commit(
  in teleportRequest,
  in teleportSnapshot);
Assert(
  teleportCommit.Succeeded && teleportPort.CommitCount == 1 &&
    teleportCooldown.Snapshot().RemainingTicks == 3 &&
    !teleportSnapshot.Cooldown.IsOnCooldown,
  "Teleport commit must accept one immutable snapshot and arm cooldown without mutating that snapshot.");
TeleportCommitResult duplicateTeleport = teleportSystem.Commit(
  in teleportRequest,
  in teleportSnapshot);
Assert(
  duplicateTeleport.WasDuplicate && teleportPort.CommitCount == 1,
  "A replayed teleport command must be idempotently rejected before the external commit port.");

TeleportTransitionSnapshot readyTeleportSnapshot = new(
  sourceSnapshot,
  destinationSnapshot,
  playerSnapshot,
  new TeleportCooldownSnapshot(0, TeleportSource.None, null));
TeleportTransitionRequest unsupportedSubjectRequest = teleportRequest with
{
  CommandId = Guid.Parse("30000000-0000-0000-0000-000000000002"),
  SubjectKind = PortalSubjectKind.Projectile,
};
TeleportTransitionCommitSystem readyTeleportSystem = new(
  new RecordingTeleportCommitPort(),
  new TeleportCooldownSystem());
TeleportCommitResult unsupportedSubject = readyTeleportSystem.Commit(
  in unsupportedSubjectRequest,
  in readyTeleportSnapshot);
Assert(
  unsupportedSubject.Reason == TeleportCommitReason.UnsupportedSubject,
  "Teleport eligibility must reject a subject kind that is not supported by the current Wiring boundary.");

TeleportTransitionRequest cooldownRequest = teleportRequest with
{
  CommandId = Guid.Parse("30000000-0000-0000-0000-000000000003"),
};
TeleportTransitionSnapshot cooldownSnapshot = new(
  sourceSnapshot,
  destinationSnapshot,
  playerSnapshot,
  teleportCooldown.Snapshot());
TeleportCommitResult cooldownRejected = teleportSystem.Commit(
  in cooldownRequest,
  in cooldownSnapshot);
Assert(
  cooldownRejected.Reason == TeleportCommitReason.CooldownActive,
  "A teleport subject on cooldown must be rejected by the read-only eligibility query.");

TeleportTransitionRequest staleRevisionRequest = teleportRequest with
{
  CommandId = Guid.Parse("30000000-0000-0000-0000-000000000004"),
};
TeleportTransitionSnapshot changedEndpointSnapshot = new(
  sourceSnapshot with { Revision = sourceSnapshot.Revision + 1 },
  destinationSnapshot,
  playerSnapshot,
  new TeleportCooldownSnapshot(0, TeleportSource.None, null));
TeleportCommitResult staleRevision = readyTeleportSystem.Commit(
  in staleRevisionRequest,
  in changedEndpointSnapshot);
Assert(
  staleRevision.Reason == TeleportCommitReason.StaleRevision,
  "A teleport command must be rejected when an endpoint revision changed after Query.");

TeleportTransitionRequest identityRequest = teleportRequest with
{
  CommandId = Guid.Parse("30000000-0000-0000-0000-000000000005"),
  SourceEndpoint = new EntityReference(
    new EntityUuid(Guid.Parse("10000000-0000-0000-0000-000000000099")),
    teleportRuntimeId,
    EntityReferenceScope.Any),
};
TeleportCommitResult identityRejected = readyTeleportSystem.Commit(
  in identityRequest,
  in readyTeleportSnapshot);
Assert(
  identityRejected.Reason == TeleportCommitReason.EndpointIdentityMismatch,
  "A teleport command must be rejected when an endpoint identity no longer matches the snapshot.");

TeleportTransitionRequest invalidEndpointRequest = teleportRequest with
{
  CommandId = Guid.Parse("30000000-0000-0000-0000-000000000006"),
  DestinationEndpoint = EntityReference.None,
};
TeleportCommitResult invalidEndpoint = readyTeleportSystem.Commit(
  in invalidEndpointRequest,
  in readyTeleportSnapshot);
Assert(
  invalidEndpoint.Reason == TeleportCommitReason.InvalidEndpoint,
  "A teleport command with an empty endpoint must be rejected before any external side effect.");

Assert(
  WiringTeleportCommand.TryCreate(
    new WiringTileCoordinate(10, 12),
    new WiringTileCoordinate(40, 12),
    wireColor: 1,
    subject: player,
    blockPlayerTeleportation: false,
    out WiringTeleportCommand wiringTeleportCommand),
  "A valid Wiring teleport intent must carry a stable command identity.");
RecordingWiringTraversalCommitPort innerWiringPort = new();
WiringTeleportTransitionAdapter wiringTeleportAdapter =
  new(
    innerWiringPort,
    new TeleportTransitionCommitSystem(
      new RecordingTeleportCommitPort(),
      new TeleportCooldownSystem()),
    new FixedWiringTeleportSnapshotProvider(readyTeleportSnapshot),
    cooldownTicks: 2);
WiringTraversalCommitResult wiringTeleportCommit =
  wiringTeleportAdapter.CommitTeleport(in wiringTeleportCommand);
Assert(
  wiringTeleportCommit.Succeeded &&
    wiringTeleportCommit.Status == WiringTraversalCommitStatus.Accepted &&
    innerWiringPort.TeleportCommitCount == 0,
  "The Wiring adapter must route teleport intents through Teleportation without delegating movement to the pump port.");
WiringTraversalCommitResult wiringDuplicate =
  wiringTeleportAdapter.CommitTeleport(in wiringTeleportCommand);
Assert(
  wiringDuplicate.WasDuplicate,
  "The Wiring adapter must preserve Teleportation command idempotency for a replayed intent.");

WiringTeleportTransitionAdapter unknownWiringTeleportAdapter =
  new(
    new RecordingWiringTraversalCommitPort(),
    new TeleportTransitionCommitSystem(
      new UnknownTeleportCommitPort(),
      new TeleportCooldownSystem()),
    new FixedWiringTeleportSnapshotProvider(readyTeleportSnapshot),
    cooldownTicks: 2);
WiringTraversalCommitResult unknownWiringTeleport =
  unknownWiringTeleportAdapter.CommitTeleport(in wiringTeleportCommand);
Assert(
  unknownWiringTeleport.Status == WiringTraversalCommitStatus.Unknown &&
    !unknownWiringTeleport.Succeeded &&
    unknownWiringTeleport.FailureReason == "authority-unknown",
  "A Wiring teleport adapter must preserve an unknown Teleport port result.");

Assert(
  WiringTeleportCommand.TryCreate(
    new WiringTileCoordinate(10, 12),
    new WiringTileCoordinate(40, 12),
    wireColor: 1,
    subject: player,
    blockPlayerTeleportation: true,
    out WiringTeleportCommand blockedWiringCommand),
  "A Wiring teleport intent with the one-iteration player block must be constructible.");
WiringTeleportTransitionAdapter blockedWiringAdapter =
  new(
    new RecordingWiringTraversalCommitPort(),
    new TeleportTransitionCommitSystem(
      new RecordingTeleportCommitPort(),
      new TeleportCooldownSystem()),
    new FixedWiringTeleportSnapshotProvider(readyTeleportSnapshot),
    cooldownTicks: 2);
WiringTraversalCommitResult blockedWiring =
  blockedWiringAdapter.CommitTeleport(in blockedWiringCommand);
Assert(
  !blockedWiring.Succeeded &&
    blockedWiring.FailureReason == TeleportCommitReason.BlockedByIteration.ToString(),
  "The Wiring one-iteration player block must remain an explicit Teleportation rejection.");

Console.WriteLine("PASS: P01 liquid, wiring, pylon, and revenge core cases");

sealed class RecordingTeleportCommitPort : ITeleportCommitPort
{
  public int CommitCount { get; private set; }

  public TeleportCommitPortResult Commit(
    in TeleportTransitionRequest request,
    in TeleportTransitionSnapshot snapshot)
  {
    CommitCount++;
    return TeleportCommitPortResult.Accepted;
  }
}

sealed class FixedWiringTeleportSnapshotProvider : IWiringTeleportSnapshotProvider
{
  private readonly TeleportTransitionSnapshot _snapshot;

  public FixedWiringTeleportSnapshotProvider(
    in TeleportTransitionSnapshot snapshot)
  {
    _snapshot = snapshot;
  }

  public bool TryGetSnapshot(
    in WiringTeleportCommand command,
    out TeleportTransitionSnapshot snapshot)
  {
    snapshot = _snapshot;
    return true;
  }
}

sealed class RecordingWiringTraversalCommitPort : IWiringTraversalCommitPort
{
  public int PumpCommitCount { get; private set; }

  public int TeleportCommitCount { get; private set; }

  public WiringTraversalCommitResult CommitPump(
    in PumpTransferCommand command)
  {
    PumpCommitCount++;
    return WiringTraversalCommitResult.Accepted(changed: true);
  }

  public WiringTraversalCommitResult CommitTeleport(
    in WiringTeleportCommand command)
  {
    TeleportCommitCount++;
    return WiringTraversalCommitResult.Accepted(changed: true);
  }
}

sealed class UnknownTeleportCommitPort : ITeleportCommitPort
{
  public TeleportCommitPortResult Commit(
    in TeleportTransitionRequest request,
    in TeleportTransitionSnapshot snapshot)
  {
    return TeleportCommitPortResult.Unknown("authority-unknown");
  }
}
