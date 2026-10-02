using Terraria.Player;

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

var rest = new PlayerRestComponent();
var sleeping = new PlayerSleepingComponent();
var initiallyResting = PlayerRestInteractionSystem.QueryState(rest, sleeping).IsResting;
var sleepStarted = PlayerRestInteractionSystem.BeginSleeping(
  rest,
  sleeping,
  new PlayerRestInteractionSystem.SleepingEntryDetails(
    new TileCoordinate(6, 7),
    DirectionKind.Right,
    2,
    default));
for (var tick = 0; tick < 119; tick++)
{
  PlayerRestInteractionSystem.AdvanceSleepTimer(rest, sleeping, hasReasonToActUp: false);
}

bool asleepAt119 = PlayerRestInteractionSystem.QueryState(rest, sleeping).IsFullyAsleep;
PlayerRestInteractionSystem.AdvanceSleepTimer(rest, sleeping, hasReasonToActUp: false);
bool fullyAsleep = PlayerRestInteractionSystem.QueryState(rest, sleeping).IsFullyAsleep;
bool pettingStarted = PlayerRestInteractionSystem.BeginPetting(rest);
bool timerResetWhileSleeping = PlayerRestInteractionSystem.AdvanceSleepTimer(
  rest,
  sleeping,
  hasReasonToActUp: true) &&
  sleeping.TimeSleeping == 0 &&
  (rest.Activities & PlayerRestActivity.Sleeping) != 0;
Assert(!initiallyResting && sleepStarted && !asleepAt119 && fullyAsleep && pettingStarted &&
  timerResetWhileSleeping &&
  rest.Activities == (PlayerRestActivity.Sleeping | PlayerRestActivity.Petting),
  "Rest activities coexist, reach the 120-tick threshold, and reset sleep time " +
  "when a wake reason is present.");

var stopRest = new PlayerRestComponent();
var sitting = new PlayerSittingComponent();
var stopSleeping = new PlayerSleepingComponent { TimeSleeping = 40 };
var sittingDetails = new PlayerRestInteractionSystem.SittingEntryDetails(
  new TileCoordinate(2, 3),
  DirectionKind.Left,
  RestSeatFeatures.Toilet,
  new System.Numerics.Vector2(0f, -4f),
  1);
var sleepingEntryDetails = new PlayerRestInteractionSystem.SleepingEntryDetails(
  new TileCoordinate(6, 7),
  DirectionKind.Right,
  2,
  new System.Numerics.Vector2(0f, -2f));
bool pettingStartedForCleanup = PlayerRestInteractionSystem.BeginPetting(stopRest);
PlayerRestInteractionSystem.EntryPreparationResult missingSittingPreparation =
  PlayerRestInteractionSystem.PrepareEntry(
    new PlayerRestInteractionSystem.EntryInput(
      PlayerRestInteractionSystem.EntrySource.Sitting,
      IsEligible: true,
      IsActivityActive: false,
      ResolvedPositionMatches: false),
    stopRest,
    sitting,
    stopSleeping,
    isLocalPlayer: false,
    multiplayerBroadcast: false);
bool missingDetailsRejectedBeforeCleanup =
  missingSittingPreparation.Decision == PlayerRestInteractionSystem.EntryDecision.Rejected &&
  stopRest.Activities == PlayerRestActivity.Petting;
PlayerRestInteractionSystem.EntryPreparationResult sittingPreparation =
  PlayerRestInteractionSystem.PrepareEntry(
    new PlayerRestInteractionSystem.EntryInput(
      PlayerRestInteractionSystem.EntrySource.Sitting,
      IsEligible: true,
      IsActivityActive: false,
      ResolvedPositionMatches: false,
      SittingDetails: sittingDetails),
    stopRest,
    sitting,
    stopSleeping,
    isLocalPlayer: false,
    multiplayerBroadcast: false);
bool sittingCommitted = PlayerRestInteractionSystem.CommitEntry(
  stopRest,
  sitting,
  stopSleeping,
  sittingPreparation);
bool sittingDetailsCommitted = sitting.AnchorTile == sittingDetails.AnchorTile &&
  sitting.RequiredFacing == sittingDetails.RequiredFacing &&
  sitting.SeatFeatures == sittingDetails.SeatFeatures &&
  sitting.SeatOffset == sittingDetails.SeatOffset &&
  sitting.StackIndex == sittingDetails.StackIndex;
PlayerRestInteractionSystem.EntryPreparationResult sleepingPreparation =
  PlayerRestInteractionSystem.PrepareEntry(
    new PlayerRestInteractionSystem.EntryInput(
      PlayerRestInteractionSystem.EntrySource.Sleeping,
      IsEligible: true,
      IsActivityActive: false,
      ResolvedPositionMatches: false,
      SleepingDetails: sleepingEntryDetails),
    stopRest,
    sitting,
    stopSleeping,
    isLocalPlayer: false,
    multiplayerBroadcast: false);
bool sleepingCommitted = PlayerRestInteractionSystem.CommitEntry(
  stopRest,
  sitting,
  stopSleeping,
  sleepingPreparation);
bool sleepingDetailsCommitted = stopSleeping.AnchorTile == sleepingEntryDetails.AnchorTile &&
  stopSleeping.RequiredFacing == sleepingEntryDetails.RequiredFacing &&
  stopSleeping.StackIndex == sleepingEntryDetails.StackIndex &&
  stopSleeping.BedVisualOffset == sleepingEntryDetails.BedVisualOffset &&
  stopSleeping.TimeSleeping == 0;
PlayerRestStopResult restStop = PlayerRestInteractionSystem.StopAll(
  stopRest,
  sitting,
  stopSleeping,
  isLocalPlayer: false,
  multiplayerBroadcast: false);
PlayerRestStopResult repeatedRestStop = PlayerRestInteractionSystem.StopAll(
  stopRest,
  sitting,
  stopSleeping,
  isLocalPlayer: false,
  multiplayerBroadcast: false);
Assert(pettingStartedForCleanup && missingDetailsRejectedBeforeCleanup && sittingCommitted &&
  sittingDetailsCommitted && sleepingCommitted && sleepingDetailsCommitted &&
  sittingPreparation.RestStopResult.StoppedActivities == PlayerRestActivity.Petting &&
  sleepingPreparation.RestStopResult.StoppedActivities == PlayerRestActivity.Sitting &&
  restStop.StoppedActivities == PlayerRestActivity.Sleeping &&
  repeatedRestStop.StoppedActivities == PlayerRestActivity.None &&
  stopRest.Activities == PlayerRestActivity.None && sitting.AnchorTile is null &&
  sitting.RequiredFacing == DirectionKind.None && sitting.SeatFeatures == RestSeatFeatures.None &&
  sitting.SeatOffset == default && sitting.StackIndex == -1 && stopSleeping.AnchorTile is null &&
  stopSleeping.RequiredFacing == DirectionKind.None && stopSleeping.StackIndex == -1 &&
  stopSleeping.TimeSleeping == 0 && stopSleeping.BedVisualOffset == default,
  "Rest entry commits validated sitting/sleeping details; stop clears active state synchronously.");

var respawningLifecycle = new PlayerLifecycleComponent(PlayerLifecyclePhase.Dead, 1);
PlayerLifecycleSystem.DeadTickResult respawnTick = PlayerLifecycleSystem.AdvanceDeadTick(
  ref respawningLifecycle,
  new PlayerLifecycleSystem.DeadTickInput(
    IsHardcoreWithoutRespawn: false,
    IsGhost: false,
    IsLocalPlayer: true,
    IsServer: false,
    HasCursorItem: true));
var ghostLifecycle = new PlayerLifecycleComponent(PlayerLifecyclePhase.Dead, 3);
PlayerLifecycleSystem.DeadTickResult ghostTick = PlayerLifecycleSystem.AdvanceDeadTick(
  ref ghostLifecycle,
  new PlayerLifecycleSystem.DeadTickInput(
    IsHardcoreWithoutRespawn: true,
    IsGhost: true,
    IsLocalPlayer: false,
    IsServer: true,
    HasCursorItem: false));
var expiredHardcoreLifecycle = new PlayerLifecycleComponent(PlayerLifecyclePhase.Dead, -1);
PlayerLifecycleSystem.DeadTickResult expiredHardcoreTick =
  PlayerLifecycleSystem.AdvanceDeadTick(
    ref expiredHardcoreLifecycle,
    new PlayerLifecycleSystem.DeadTickInput(
      IsHardcoreWithoutRespawn: true,
      IsGhost: false,
      IsLocalPlayer: true,
      IsServer: false,
      HasCursorItem: false));
var savedAt = new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);
var worldJoinLifecycle = new PlayerLifecycleComponent(PlayerLifecyclePhase.Dead, 900);
PlayerLifecycleSystem.SpawnCommitResult worldJoin = PlayerLifecycleSystem.CommitSpawn(
  new PlayerIdentityComponent(),
  ref worldJoinLifecycle,
  new PlayerDeathRecordComponent(),
  new PlayerRestComponent(),
  new PlayerSittingComponent(),
  new PlayerSleepingComponent(),
  new PlayerLifecycleSystem.SpawnCommitInput(
    IsSpawningIntoWorld: true,
    LastTimePlayerWasSavedBinary: savedAt.ToBinary(),
    CurrentUtcTime: savedAt.AddSeconds(10),
    IsLocalPlayer: true,
    MultiplayerBroadcast: false));
var expiredWorldJoinLifecycle = new PlayerLifecycleComponent(PlayerLifecyclePhase.Dead, 600);
PlayerLifecycleSystem.SpawnCommitResult expiredWorldJoin = PlayerLifecycleSystem.CommitSpawn(
  new PlayerIdentityComponent(),
  ref expiredWorldJoinLifecycle,
  new PlayerDeathRecordComponent(),
  new PlayerRestComponent(),
  new PlayerSittingComponent(),
  new PlayerSleepingComponent(),
  new PlayerLifecycleSystem.SpawnCommitInput(
    IsSpawningIntoWorld: true,
    LastTimePlayerWasSavedBinary: savedAt.ToBinary(),
    CurrentUtcTime: savedAt.AddSeconds(10),
    IsLocalPlayer: true,
    MultiplayerBroadcast: false));
var remoteWorldJoinLifecycle = new PlayerLifecycleComponent(PlayerLifecyclePhase.Dead, 900);
PlayerLifecycleSystem.CommitSpawn(
  new PlayerIdentityComponent(),
  ref remoteWorldJoinLifecycle,
  new PlayerDeathRecordComponent(),
  new PlayerRestComponent(),
  new PlayerSittingComponent(),
  new PlayerSleepingComponent(),
  new PlayerLifecycleSystem.SpawnCommitInput(
    IsSpawningIntoWorld: true,
    LastTimePlayerWasSavedBinary: savedAt.ToBinary(),
    CurrentUtcTime: savedAt.AddSeconds(10),
    IsLocalPlayer: false,
    MultiplayerBroadcast: false));
Assert(respawnTick.Advanced && respawnTick.ShouldRequestRespawn &&
  respawnTick.ShouldOpenInventory && respawningLifecycle.DeadElapsedTicks == 1 &&
  respawningLifecycle.RespawnRemainingTicks == 0 && !ghostTick.Advanced &&
  ghostLifecycle.DeadElapsedTicks == 0 && ghostLifecycle.RespawnRemainingTicks == 3 &&
  expiredHardcoreTick.ShouldBecomeGhost && expiredHardcoreLifecycle.RespawnRemainingTicks == -1 &&
  worldJoin.PreservedDeathState && worldJoinLifecycle.RespawnRemainingTicks == 300 &&
  expiredWorldJoinLifecycle.Phase == PlayerLifecyclePhase.Alive &&
  expiredWorldJoinLifecycle.RespawnRemainingTicks == 0 &&
  !expiredWorldJoin.PreservedDeathState && remoteWorldJoinLifecycle.IsDead &&
  remoteWorldJoinLifecycle.RespawnRemainingTicks == 900,
  "Dead ticks preserve ghost timers, and world joins only consume local saved respawn time.");

var identity = new PlayerIdentityComponent();
PlayerLifecycleSystem.ConnectionTransition connected =
  PlayerLifecycleSystem.SetConnectionState(
    identity,
    PlayerConnectionState.Active,
    PlayerLifecycleSystem.ConnectionStateChangeSource.NetworkActiveState);
PlayerLifecycleSystem.ConnectionTransition repeatedConnection =
  PlayerLifecycleSystem.SetConnectionState(
    identity,
    PlayerConnectionState.Active,
    PlayerLifecycleSystem.ConnectionStateChangeSource.NetworkActiveState);
PlayerLifecycleSystem.ConnectionTransition disconnected =
  PlayerLifecycleSystem.SetConnectionState(
    identity,
    PlayerConnectionState.Inactive,
    PlayerLifecycleSystem.ConnectionStateChangeSource.NetworkActiveState);
PlayerLifecycleSystem.ConnectionTransition spawned =
  PlayerLifecycleSystem.SetConnectionState(
    identity,
    PlayerConnectionState.Active,
    PlayerLifecycleSystem.ConnectionStateChangeSource.Spawn);
Assert(connected.HookIntent == PlayerLifecycleSystem.ConnectionHookIntent.Connected &&
  !repeatedConnection.Changed &&
  repeatedConnection.HookIntent == PlayerLifecycleSystem.ConnectionHookIntent.None &&
  disconnected.HookIntent == PlayerLifecycleSystem.ConnectionHookIntent.Disconnected &&
  spawned.HookIntent == PlayerLifecycleSystem.ConnectionHookIntent.None && identity.IsActive,
  "Only real network connection transitions request hooks; spawn activation does not.");

var lifecycle = new PlayerLifecycleComponent(PlayerLifecyclePhase.Alive, 0);
PlayerLifecycleSystem.SpectatingTargetResult startedSpectating =
  PlayerLifecycleSystem.RequestSpectatingTarget(
    ref lifecycle,
    new PlayerLifecycleSystem.SpectatingTargetRequestInput(
      RequestedTargetPlayerSlot: 3,
      ObserverPlayerSlot: 1,
      NetworkMode: PlayerLifecycleSystem.SpectatingNetworkMode.Server,
      IsLocalPlayer: false,
      TargetWhoAmI: 3,
      TargetIsActive: true,
      TargetIsDead: false,
      TargetDeadElapsedTicks: 0));
var deathRecord = new PlayerDeathRecordComponent();
var deathRest = new PlayerRestComponent();
var deathSitting = new PlayerSittingComponent();
var deathSleeping = new PlayerSleepingComponent();
PlayerRestInteractionSystem.BeginSitting(
  deathRest,
  deathSitting,
  new PlayerRestInteractionSystem.SittingEntryDetails(
    new TileCoordinate(4, 5),
    DirectionKind.Right,
    RestSeatFeatures.None,
    default,
    0));
var deathInput = new PlayerLifecycleSystem.DeathResolutionInput(
  IsCreativeGodMode: false,
  PracticeModeResetReturnsTrue: false,
  IsPvpRequest: true,
  DeathPosition: new WorldPosition(12f, 34f),
  DeathTime: new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc),
  RespawnRemainingTicks: 600,
  IsLocalPlayer: false,
  MultiplayerBroadcast: false,
  NetworkMode: PlayerLifecycleSystem.SpectatingNetworkMode.Server);
PlayerLifecycleSystem.DeathResolutionResult death = PlayerLifecycleSystem.ResolveDeath(
  ref lifecycle,
  deathRecord,
  deathRest,
  deathSitting,
  deathSleeping,
  deathInput);
PlayerLifecycleSystem.DeathResolutionResult duplicateDeath =
  PlayerLifecycleSystem.ResolveDeath(
    ref lifecycle,
    deathRecord,
    deathRest,
    deathSitting,
    deathSleeping,
    deathInput);
bool deathCommittedBeforeSpawn = death.DidCommit && lifecycle.IsDead &&
  lifecycle.SpectatingTargetSlot is null && deathRecord.PvpDeathCount == 1 &&
  deathRest.Activities == PlayerRestActivity.None &&
  duplicateDeath.Status == PlayerLifecycleSystem.DeathResolutionStatus.AlreadyDead;
PlayerLifecycleSystem.SpawnCommitResult deathWorldJoin = PlayerLifecycleSystem.CommitSpawn(
  new PlayerIdentityComponent(),
  ref lifecycle,
  deathRecord,
  deathRest,
  deathSitting,
  deathSleeping,
  new PlayerLifecycleSystem.SpawnCommitInput(
    IsSpawningIntoWorld: true,
    LastTimePlayerWasSavedBinary: savedAt.ToBinary(),
    CurrentUtcTime: savedAt,
    IsLocalPlayer: true,
    MultiplayerBroadcast: false));
bool deathPreservedAfterWorldJoin = deathWorldJoin.PreservedDeathState &&
  !deathWorldJoin.ShouldApplyPvpDeathRecovery && deathRecord.WasPvpDeath && lifecycle.IsDead;
PlayerLifecycleSystem.SpawnCommitResult deathSpawn = PlayerLifecycleSystem.CommitSpawn(
  new PlayerIdentityComponent(),
  ref lifecycle,
  deathRecord,
  deathRest,
  deathSitting,
  deathSleeping,
  new PlayerLifecycleSystem.SpawnCommitInput(
    IsSpawningIntoWorld: false,
    LastTimePlayerWasSavedBinary: 0L,
    CurrentUtcTime: new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc),
    IsLocalPlayer: true,
    MultiplayerBroadcast: false));
Assert(startedSpectating.Action == PlayerLifecycleSystem.SpectatingTargetAction.BroadcastServerTarget &&
  deathCommittedBeforeSpawn && deathPreservedAfterWorldJoin &&
  death.SpectatingResult.Action ==
    PlayerLifecycleSystem.SpectatingTargetAction.BroadcastServerTarget &&
  deathSpawn.ShouldApplyPvpDeathRecovery && !deathRecord.WasPvpDeath &&
  lifecycle.Phase == PlayerLifecyclePhase.Alive,
  "Death commits once; spawn returns the PvP recovery intent before clearing its lifecycle marker.");

Console.WriteLine("PASS: player lifecycle/rest core invariants");
