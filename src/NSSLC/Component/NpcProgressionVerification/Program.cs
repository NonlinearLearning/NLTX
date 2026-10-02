using Terraria.WorldSession.NpcProgression;
using Terraria.WorldSession.NpcProgression.Boss;
using Terraria.WorldSession.NpcProgression.Events;
using Terraria.WorldSession.NpcProgression.Invasion;
using Terraria.WorldSession.NpcProgression.LunarTower;
using Terraria.WorldSession.NpcProgression.MoonLord;

var bossProgression = new BossDefeatProgressionStateComponent();
var eventProgression = new EventDefeatProgressionStateComponent();
var system = new NpcProgressionCommitSystem(bossProgression, eventProgression);

var firstBoss = system.MarkBossDefeated(BossDefeatProgressionKind.MechBoss1);
Assert(
  firstBoss.Status == NpcProgressionCommitStatus.Committed,
  "The first boss defeat must commit.");
Assert(firstBoss.IsFirstClear, "The first boss defeat must be marked as a first clear.");
Assert(bossProgression.DownedMechBoss1, "The committed boss flag must be set.");
Assert(bossProgression.DownedMechBossAny, "A mech defeat must set the aggregate mech flag.");

var duplicateBoss = system.MarkBossDefeated(BossDefeatProgressionKind.MechBoss1);
Assert(
  duplicateBoss.Status == NpcProgressionCommitStatus.AlreadyCleared,
  "A duplicate boss defeat must be idempotent.");

var firstEvent = system.MarkEventDefeated(EventDefeatProgressionKind.TowerSolar);
Assert(
  firstEvent.Status == NpcProgressionCommitStatus.Committed,
  "The first event defeat must commit.");
Assert(eventProgression.DownedTowerSolar, "The committed event flag must be set.");

var duplicateEvent = system.MarkEventDefeated(EventDefeatProgressionKind.TowerSolar);
Assert(
  duplicateEvent.Status == NpcProgressionCommitStatus.AlreadyCleared,
  "A duplicate event defeat must be idempotent.");

var rejectedBoss = system.MarkBossDefeated((BossDefeatProgressionKind)255);
Assert(
  rejectedBoss.Status == NpcProgressionCommitStatus.RejectedUnknownKind,
  "An unknown boss kind must be rejected without a state write.");

system.Reset();
Assert(!bossProgression.DownedMechBoss1, "Reset must clear boss progression.");
Assert(!bossProgression.DownedMechBossAny, "Reset must clear aggregate boss progression.");
Assert(!eventProgression.DownedTowerSolar, "Reset must clear event progression.");

var moonLordDefinition = new MoonLordEncounterDefinition(
  new int[1, 1, 1, 1],
  new int[1, 1],
  4500,
  3600,
  3600,
  720);
var moonLordState = new MoonLordEncounterStateComponent(moonLordDefinition.MaxMoonLordCountdown);
var moonLordSystem = new MoonLordEncounterSystem(moonLordDefinition, moonLordState);

var itemCountdown = moonLordSystem.StartItemCountdown();
Assert(itemCountdown.MaxMoonLordCountdown == 720, "Item countdown must update the active maximum.");
Assert(
  itemCountdown.MoonLordCountdown == 720,
  "Item countdown must start at its configured value.");

moonLordSystem.StartCountdown(2);
var firstTick = moonLordSystem.TickCountdown(hasServerSpawnAuthority: true);
Assert(
  firstTick.Status == MoonLordCountdownTickStatus.Ticked,
  "A non-zero tick must only decrement.");
var zeroTick = moonLordSystem.TickCountdown(hasServerSpawnAuthority: true);
Assert(zeroTick.SpawnRequested, "The authoritative zero crossing must request one spawn.");
var duplicateZeroTick = moonLordSystem.TickCountdown(hasServerSpawnAuthority: true);
Assert(
  duplicateZeroTick.Status == MoonLordCountdownTickStatus.NoCountdown,
  "A zero countdown must not request a second spawn.");
moonLordSystem.StartCountdown(1);
var clientZeroTick = moonLordSystem.TickCountdown(hasServerSpawnAuthority: false);
Assert(
  clientZeroTick.Status == MoonLordCountdownTickStatus.ReachedZero
    && !clientZeroTick.SpawnRequested,
  "A non-authoritative zero crossing must not request a spawn.");
moonLordSystem.Reset();
Assert(moonLordState.MoonLordCountdown == 0, "Moon Lord reset must clear the countdown.");

var invasionState = new InvasionWaveProgressStateComponent();
var invasionSystem = new InvasionWaveProgressSystem(invasionState);
var startedWave = invasionSystem.BeginWave();
Assert(
  startedWave.View == new InvasionWaveProgressSyncView(0.0f, 0.0f, 1),
  "Wave start tuple is incorrect.");
var committedWave = invasionSystem.CommitProgress(42.0f, 7.0f, 2);
Assert(committedWave.IsCommitted, "A changed invasion tuple must commit.");
Assert(
  committedWave.View == new InvasionWaveProgressSyncView(42.0f, 7.0f, 2),
  "Wave tuple commit is incorrect.");
var duplicateWave = invasionSystem.CommitProgress(42.0f, 7.0f, 2);
Assert(
  duplicateWave.Status == InvasionWaveProgressCommitStatus.AlreadyCommitted,
  "An identical invasion tuple must be idempotent.");
var stoppedWave = invasionSystem.StopWave();
Assert(
  stoppedWave.View == new InvasionWaveProgressSyncView(0.0f, 0.0f, 0),
  "Wave stop must reset the tuple.");

var lunarTowerState = new LunarTowerEncounterStateComponent();
var lunarTowerSystem = new LunarTowerEncounterSystem(lunarTowerState);
lunarTowerSystem.BeginApocalypse();
Assert(lunarTowerState.LunarApocalypseIsUp, "Apocalypse start must set the active state.");
Assert(
  lunarTowerState.ShieldStrengthTowerSolar == 100,
  "Apocalypse start must restore tower shields.");
Assert(
  lunarTowerSystem.ApplyShieldDamage(LunarTowerKind.Solar, 150) == 0,
  "Tower shield damage must clamp at zero.");
var apocalypseResult = lunarTowerSystem.RecomputeApocalypse(
  new LunarTowerPresenceSnapshot(false, false, false, false, false),
  3600);
Assert(
  apocalypseResult.StartedImpendingDoom,
  "The last missing tower must request impending doom.");
Assert(!lunarTowerState.LunarApocalypseIsUp, "Impending doom must clear the apocalypse state.");

Console.WriteLine("PASS: P13 core progression/encounter smoke (10% scope)");

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}
