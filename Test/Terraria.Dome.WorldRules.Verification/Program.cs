using System;
using System.Collections.Generic;
using System.IO;
using Terraria.Dome.Protocol.V1456.Compatibility;
using Terraria.Dome.Protocol.V1456.Dispatch;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Protocol.V1456.Protocol;
using Terraria.Dome.Protocol.V1456.Session;
using Terraria.Dome.Server;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Npc.Commands;
using Terraria.Dome.Simulation.World.Events;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Systems;

using DomeSimulation first = new(new WorldGrid(400, 300));
using DomeSimulation second = new(new WorldGrid(400, 300));
for (int index = 0; index < 120; index++)
{
  first.Tick(new SimulationInputBatch());
  second.Tick(new SimulationInputBatch());
}

if (first.CreateWorldRuleSnapshot() != second.CreateWorldRuleSnapshot() ||
    first.CreateWorldRuleSnapshot().TimeOfDay != 120 || !first.CreateWorldRuleSnapshot().IsDayTime)
{
  throw new InvalidOperationException("World time was not a deterministic simulation-owned rule.");
}

Console.WriteLine("PASS: deterministic world time advances only from simulation ticks");

WorldClock overflowClock = new(tickNumber: long.MaxValue - 1, timeOfDay: 0);
try
{
  overflowClock.Advance(2);
  throw new InvalidOperationException(
    "World clock accepted an advance that would overflow TickNumber.");
}
catch (ArgumentOutOfRangeException)
{
  if (overflowClock.TickNumber != long.MaxValue - 1)
  {
    throw new InvalidOperationException(
      "World clock partially mutated before rejecting TickNumber overflow.");
  }
}

Console.WriteLine("PASS: world clock rejects TickNumber overflow before mutation");

WorldClock moonPhaseClock = new(
  tickNumber: 0,
  timeOfDay: WorldClock.DefaultNightLengthTicks - 1,
  isDayTime: false,
  moonPhase: 7);
new WorldClockSystem().Tick(moonPhaseClock);
WorldClockSnapshot dawnSnapshot = moonPhaseClock.CreateSnapshot();
if (!dawnSnapshot.IsDayTime || dawnSnapshot.TimeOfDay != 0 || dawnSnapshot.MoonPhase != 0)
{
  throw new InvalidOperationException(
    "A V1456 dawn did not increment and wrap the authoritative moon phase.");
}

WorldClock pausedMoonPhaseClock = new(isPaused: true, moonPhase: 4);
new WorldClockSystem().Tick(pausedMoonPhaseClock);
if (pausedMoonPhaseClock.CreateSnapshot().MoonPhase != 4)
{
  throw new InvalidOperationException("A paused world clock changed its moon phase.");
}

try
{
  _ = new WorldClock(moonPhase: 8);
  throw new InvalidOperationException("An invalid moon phase was accepted by the world clock.");
}
catch (ArgumentOutOfRangeException)
{
}

TerrariaFrame frame = TerrariaFrameCodec.Decode(TerrariaPacketCodec.EncodeWorldTime(
  first.CreateWorldRuleSnapshot()));
if (frame.MessageId != TerrariaMessageId.SetTime ||
    !frame.Payload.Span.SequenceEqual(new byte[] {
      1,
      120,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0,
      0
    }))
{
  throw new InvalidOperationException(
    "World time did not retain the original V1456 SetTime field widths.");
}

TerrariaPacketDispatcher dispatcher = new();
TerrariaSession session = new(1);
try
{
  _ = dispatcher.Dispatch(
    session,
    TerrariaPacketCodec.EncodeWorldTime(first.CreateWorldRuleSnapshot()));
  throw new InvalidOperationException("Client SetTime was accepted as authority.");
}
catch (InvalidDataException)
{
}

Console.WriteLine("PASS: client SetTime assertions are rejected before world mutation");

WorldGrid bloodMoonWorld = new(width: 400, height: 300);
WorldMetadata bloodMoonMetadata = new(
  "blood-moon",
  new WorldSeed(19),
  400,
  300,
  spawnX: 200,
  spawnY: 75);
DomeSimulationSnapshot bloodMoonSnapshot = new(
  bloodMoonWorld.CreateSnapshot(bloodMoonMetadata),
  [],
  [],
  0,
  worldClock: new WorldClockSnapshot(0, 1, false, false, 1),
  progression: new WorldProgressionState(isBloodMoon: true));
using DomeServer bloodMoonServer = new(bloodMoonSnapshot);
LegacyWorldDataContext projectedWorldData = bloodMoonServer.CreateWorldDataContext();
TerrariaFrame projectedWorldDataFrame = TerrariaFrameCodec.Decode(
  TerrariaV1456Compatibility.EncodeWorldData(projectedWorldData));
if (projectedWorldDataFrame.Payload.Length < 5 || projectedWorldDataFrame.Payload.Span[4] != 2)
{
  throw new InvalidOperationException(
    "WorldData did not project the authoritative blood-moon flag.");
}

Console.WriteLine("PASS: authoritative blood-moon state projects through WorldData");

WorldGrid moonPhaseWorld = new(width: 400, height: 300);
WorldMetadata moonPhaseMetadata = new("Moon phase", new WorldSeed(9), 400, 300);
DomeSimulationSnapshot moonPhaseSnapshot = new(
  moonPhaseWorld.CreateSnapshot(moonPhaseMetadata),
  [],
  [],
  tickNumber: 0,
  worldClock: new WorldClockSnapshot(0, 0, true, false, 1, MoonPhase: 4));
using DomeServer moonPhaseServer = new(moonPhaseSnapshot);
if (moonPhaseServer.CreateWorldDataContext().MoonPhase != 4)
{
  throw new InvalidOperationException(
    "WorldData did not project the authoritative moon phase from the simulation snapshot.");
}

VerifyBloodMoonStateMachine();
Console.WriteLine("PASS: blood-moon progression is deterministic, bounded and persistent");

VerifyEclipseStateMachine();
Console.WriteLine("PASS: eclipse progression is deterministic, qualified and persistent");

VerifyInvasionStateMachine();
VerifyInvasionSpawnEligibility();
VerifyInvasionPositionGuard();
VerifyInvasionTownFallbackGuard();
VerifyInvasionDelayPolicy();
VerifyInvasionDelayTickIntegration();
VerifyInvasionProgressProjection();
Console.WriteLine("PASS: invasion progression is deterministic, bounded and persistent");

WorldInvasionStartEligibilitySystem invasionEligibility = new();
if (invasionEligibility.CanStart(2, [
      new WorldInvasionPlayerSnapshot(IsActive: false, MaximumHealth: 400),
      new WorldInvasionPlayerSnapshot(IsActive: true, MaximumHealth: 199)]) ||
    !invasionEligibility.CanStart(2, [
      new WorldInvasionPlayerSnapshot(IsActive: true, MaximumHealth: 200)]) ||
    invasionEligibility.CanStart(0, [
      new WorldInvasionPlayerSnapshot(IsActive: true, MaximumHealth: 400)]))
{
  throw new InvalidOperationException(
    "Invasion start eligibility did not preserve the active 200-health source threshold.");
}
Console.WriteLine("PASS: invasion start eligibility retains the qualified-player threshold");

using DomeSimulation gatedInvasion = new(new WorldGrid(400, 300));
if (gatedInvasion.TryQueueWorldInvasion(
      new WorldInvasionStartCommand(2, 40, 20),
      [new WorldInvasionPlayerSnapshot(IsActive: true, MaximumHealth: 199)]) ||
    !gatedInvasion.TryQueueWorldInvasion(
      new WorldInvasionStartCommand(2, 40, 21),
      [new WorldInvasionPlayerSnapshot(IsActive: true, MaximumHealth: 200)]))
{
  throw new InvalidOperationException(
    "The snapshot-gated invasion start route ignored qualified player authority.");
}
Console.WriteLine("PASS: snapshot-gated invasion start uses server player facts");

using DomeSimulation playerSnapshotSimulation = new(new WorldGrid(400, 300));
_ = playerSnapshotSimulation.CreatePlayer(new SimulationVector(10.0f, 0.0f));
IReadOnlyList<WorldInvasionPlayerSnapshot> producedPlayerSnapshots =
  playerSnapshotSimulation.CreateWorldInvasionPlayerSnapshots();
if (producedPlayerSnapshots.Count != 1 ||
    !producedPlayerSnapshots[0].IsActive ||
    producedPlayerSnapshots[0].MaximumHealth != 100)
{
  throw new InvalidOperationException(
    "Simulation did not produce the authoritative invasion player snapshot.");
}
Console.WriteLine("PASS: simulation produces invasion player readiness snapshots");

WorldInvasionSizeSystem invasionSize = new();
if (invasionSize.Resolve(1, 1) != 120 ||
    invasionSize.Resolve(3, 2) != 240 ||
    invasionSize.Resolve(4, 2) != 240)
{
  throw new InvalidOperationException(
    "Invasion size policy did not preserve the source type-specific formulas.");
}
try
{
  _ = invasionSize.Resolve(2, 0);
  throw new InvalidOperationException("Invasion size policy accepted zero qualified players.");
}
catch (ArgumentOutOfRangeException)
{
}
try
{
  _ = invasionSize.Resolve(3, int.MaxValue);
  throw new InvalidOperationException(
    "Invasion size policy accepted a player count that overflows its Int32 size.");
}
catch (ArgumentOutOfRangeException)
{
}
Console.WriteLine("PASS: invasion size policy retains source type formulas");

using DomeSimulation derivedStart = new(new WorldGrid(400, 300));
if (!derivedStart.TryQueueWorldInvasion(
      3,
      30,
      [
        new WorldInvasionPlayerSnapshot(IsActive: true, MaximumHealth: 200),
        new WorldInvasionPlayerSnapshot(IsActive: true, MaximumHealth: 250),
        new WorldInvasionPlayerSnapshot(IsActive: false, MaximumHealth: 500)
      ]))
{
  throw new InvalidOperationException("Derived invasion start route rejected qualified players.");
}
derivedStart.Tick(new SimulationInputBatch());
if (derivedStart.CreateWorldProgressionSnapshot().InvasionType != 3 ||
    derivedStart.CreateWorldProgressionSnapshot().InvasionSize != 240)
{
  throw new InvalidOperationException(
    "Derived invasion start route did not compute the source size from qualified players.");
}
Console.WriteLine("PASS: snapshot-gated invasion start derives source size");

WorldInvasionTravelSystem invasionTravel = new();
WorldInvasionTravelResult rightward = invasionTravel.Advance(10.0, 15, 2.0f);
WorldInvasionTravelResult leftward = invasionTravel.Advance(20.0, 15, 0.25f);
WorldInvasionTravelResult clamped = invasionTravel.Advance(14.0, 15, 2.0f);
if (rightward.Position != 12.0 || rightward.Arrived ||
    leftward.Position != 19.0 || leftward.Arrived ||
    clamped.Position != 15.0 || !clamped.Arrived)
{
  throw new InvalidOperationException(
    "Invasion travel did not preserve the source step and clamp rules.");
}
Console.WriteLine("PASS: invasion travel preserves source step and arrival clamp");

WorldInvasionWarningSystem invasionWarning = new();
WorldInvasionWarningResult countdown = invasionWarning.Advance(2, arrived: false, moved: true);
WorldInvasionWarningResult reset = invasionWarning.Advance(1, arrived: false, moved: true);
WorldInvasionWarningResult arrival = invasionWarning.Advance(17, arrived: true, moved: true);
if (countdown.WarningTicks != 1 || countdown.ShouldWarn ||
    reset.WarningTicks != 3600 || !reset.ShouldWarn ||
    arrival.WarningTicks != 17 || !arrival.ShouldWarn)
{
  throw new InvalidOperationException(
    "Invasion warning policy did not preserve countdown, reset and arrival rules.");
}
Console.WriteLine("PASS: invasion warning policy preserves countdown and arrival rules");

WorldInvasionWarningMessageSystem invasionWarningMessage = new();
if (invasionWarningMessage.Resolve(2, 0, 100.0, 120.0) !=
      WorldInvasionWarningKind.CompletedFrost ||
    invasionWarningMessage.Resolve(3, 5, 100.0, 120.0) !=
      WorldInvasionWarningKind.ApproachingPirates ||
    invasionWarningMessage.Resolve(1, 5, 130.0, 120.0) !=
      WorldInvasionWarningKind.RecedingGoblins ||
    invasionWarningMessage.Resolve(4, 5, 100.0, 120.0) != WorldInvasionWarningKind.None ||
    invasionWarningMessage.Resolve(4, 5, 120.0, 120.0) !=
      WorldInvasionWarningKind.ArrivedMartians)
{
  throw new InvalidOperationException(
    "Invasion warning message branches did not preserve the source type and travel rules.");
}
Console.WriteLine("PASS: invasion warning message branches preserve source semantics");

WorldInvasionClearFlagSystem invasionClearFlags = new();
if (invasionClearFlags.Resolve(1) != WorldInvasionClearFlag.Goblins ||
    invasionClearFlags.Resolve(2) != WorldInvasionClearFlag.Frost ||
    invasionClearFlags.Resolve(3) != WorldInvasionClearFlag.Pirates ||
    invasionClearFlags.Resolve(4) != WorldInvasionClearFlag.Martians)
{
  throw new InvalidOperationException(
    "Invasion completion did not preserve the source type-to-clear-flag mapping.");
}
try
{
  _ = invasionClearFlags.Resolve(0);
  throw new InvalidOperationException("Invasion clear flags accepted an invalid type.");
}
catch (ArgumentOutOfRangeException)
{
}
Console.WriteLine("PASS: invasion completion maps source clear flags by type");

WorldInvasionStartPositionSystem invasionStartPosition = new();
WorldInvasionStartPositionResult martianStart = invasionStartPosition.Resolve(4, 200);
WorldInvasionStartPositionResult randomStart = invasionStartPosition.Resolve(2, 200);
if (!martianStart.IsResolved || martianStart.Position != 199.0 || randomStart.IsResolved)
{
  throw new InvalidOperationException(
    "Invasion start position did not fail closed around the source random branch.");
}
Console.WriteLine("PASS: deterministic Martian invasion start position is isolated");

VerifyRainStateMachine();
Console.WriteLine("PASS: rain state is deterministic, bounded and persistent");

VerifyRawRainStateModel();
Console.WriteLine("PASS: raw weather state preserves independent rain activity and maximum strength");

  VerifyWorldEventRandomState();
  VerifyWorldEventRandomStateSnapshotContinuity();
Console.WriteLine("PASS: world-event random state is deterministic and explicit");

VerifySlimeRainStateMachine();
Console.WriteLine("PASS: slime rain state is deterministic, bounded and persistent");

VerifySlimeRainCooldownStateMachine();
Console.WriteLine("PASS: slime rain cooldown is deterministic and persistent");

VerifySlimeRainWarningStateMachine();
Console.WriteLine("PASS: slime rain warning countdown emits one authoritative event");

VerifySlimeRainEligibilityMetadata();

VerifyMeteorImpactStateMachine();
Console.WriteLine("PASS: meteor impact is deterministic, protected and world-mutating");

VerifyWindStateMachine();
Console.WriteLine("PASS: wind state is deterministic, bounded and projected");

VerifyWindRainCoupling();
Console.WriteLine("PASS: wind current uses the authoritative rain-adjusted target");

VerifyLanternNightStateMachine();
Console.WriteLine("PASS: lantern night is deterministic, bounded and projected");

VerifyLanternNightRainSuppression();
Console.WriteLine("PASS: lantern night suppresses rain through server authority");

VerifyLanternNightSameTickRainOrdering();
Console.WriteLine("PASS: pending Lantern Night suppresses rain in the same tick");

VerifyLanternNightScheduleStateMachine();
Console.WriteLine("PASS: Lantern Night schedule is server-owned and consumes once at night");

VerifyNpcFirstEventClearSchedulesLanternNight();
Console.WriteLine("PASS: NPC first-event authority schedules eligible Lantern Nights");

VerifyLanternNightEligibilityContract();
Console.WriteLine("PASS: Lantern Night eligibility retains every legacy server guard");

VerifyNormalEventStartEligibilityContract();
Console.WriteLine("PASS: normal event blocking retains the legacy world-event guards");

VerifyKingSlimeReadinessContract();
Console.WriteLine("PASS: King Slime readiness retains the legacy player thresholds");

VerifyMeteorScheduleStateMachine();
Console.WriteLine("PASS: meteor schedule is deterministic, qualified and bounded");

VerifyScheduledMeteorResolution();
Console.WriteLine("PASS: scheduled meteor resolution is authoritative and deterministic");

static void VerifyBloodMoonStateMachine()
{
  WorldMetadata metadata = new(
    "blood-moon-state",
    new WorldSeed(20),
    400,
    300,
    spawnX: 200,
    spawnY: 75,
    worldSurface: 100,
    isRemixWorld: false);
  using DomeSimulation daySimulation = new(new WorldGrid(400, 300));
  WorldProgressionState initialProgression = daySimulation.CreateWorldProgressionSnapshot();
  if (daySimulation.TryQueueWorldEvent(new WorldEventStartCommand(WorldEventKind.BloodMoon, 1)))
  {
    throw new InvalidOperationException("Blood-moon input was accepted during daytime.");
  }

  daySimulation.Tick(new SimulationInputBatch());
  if (daySimulation.CreateWorldProgressionSnapshot() != initialProgression)
  {
    throw new InvalidOperationException("Rejected daytime input changed progression state.");
  }

  WorldGrid nightWorld = new(400, 300);
  DomeSimulationSnapshot nightSnapshot = new(
    nightWorld.CreateSnapshot(metadata),
    [],
    [],
    0,
    worldClock: new WorldClockSnapshot(0, 0, false, false, 1));
  using DomeSimulation nightSimulation = new(nightSnapshot);
  if (!nightSimulation.TryQueueWorldEvent(
        new WorldEventStartCommand(WorldEventKind.BloodMoon, 2)) ||
      nightSimulation.TryQueueWorldEvent(
        new WorldEventStartCommand(WorldEventKind.BloodMoon, 3)) ||
      nightSimulation.TryQueueWorldEvent(
        new WorldEventStartCommand((WorldEventKind)99, 4)))
  {
    throw new InvalidOperationException("Blood-moon input validation was not deterministic.");
  }

  nightSimulation.Tick(new SimulationInputBatch());
  if (!nightSimulation.CreateWorldProgressionSnapshot().IsBloodMoon)
  {
    throw new InvalidOperationException("Accepted nighttime input did not start a blood moon.");
  }

  DomeSimulationSnapshot persisted = nightSimulation.CreatePersistenceSnapshot(metadata);
  using DomeSimulation restored = new(persisted);
  restored.Tick(new SimulationInputBatch());
  if (!restored.CreateWorldProgressionSnapshot().IsBloodMoon)
  {
    throw new InvalidOperationException("Blood-moon state did not survive snapshot continuation.");
  }

  WorldGrid dawnWorld = new(400, 300);
  DomeSimulationSnapshot dawnSnapshot = new(
    dawnWorld.CreateSnapshot(metadata),
    [],
    [],
    0,
    worldClock: new WorldClockSnapshot(
      0,
      WorldClock.DefaultNightLengthTicks - 1,
      false,
      false,
      1),
    progression: new WorldProgressionState(isBloodMoon: true));
  using DomeSimulation dawnSimulation = new(dawnSnapshot);
  dawnSimulation.Tick(new SimulationInputBatch());
  if (!dawnSimulation.IsDayTime || dawnSimulation.CreateWorldProgressionSnapshot().IsBloodMoon)
  {
    throw new InvalidOperationException("Blood moon did not end at the day boundary.");
  }
}

static void VerifyEclipseStateMachine()
{
  WorldMetadata metadata = new(
    "eclipse-state",
    new WorldSeed(22),
    400,
    300,
    spawnX: 200,
    spawnY: 75);
  WorldGrid dayWorld = new(400, 300);
  DomeSimulationSnapshot daySnapshot = new(
    dayWorld.CreateSnapshot(metadata),
    [],
    [],
    0,
    worldClock: new WorldClockSnapshot(0, 0, true, false, 1),
    progression: new WorldProgressionState(
      isHardMode: true,
      defeatedMechanicalBoss: true));
  using DomeSimulation daySimulation = new(daySnapshot);
  if (!daySimulation.TryQueueWorldEvent(new WorldEventStartCommand(WorldEventKind.Eclipse, 5)))
  {
    throw new InvalidOperationException("Qualified daytime eclipse input was rejected.");
  }

  daySimulation.Tick(new SimulationInputBatch());
  if (!daySimulation.CreateWorldProgressionSnapshot().IsEclipse)
  {
    throw new InvalidOperationException("Qualified daytime input did not start an eclipse.");
  }

  DomeSimulationSnapshot persisted = daySimulation.CreatePersistenceSnapshot(metadata);
  using DomeSimulation restored = new(persisted);
  restored.Tick(new SimulationInputBatch());
  if (!restored.CreateWorldProgressionSnapshot().IsEclipse)
  {
    throw new InvalidOperationException("Eclipse state did not survive snapshot continuation.");
  }

  using DomeServer eclipseServer = new(persisted);
  LegacyWorldDataContext eclipseWorldData = eclipseServer.CreateWorldDataContext();
  TerrariaFrame eclipseWorldDataFrame = TerrariaFrameCodec.Decode(
    TerrariaV1456Compatibility.EncodeWorldData(eclipseWorldData));
  if (eclipseWorldDataFrame.Payload.Length < 5 || eclipseWorldDataFrame.Payload.Span[4] != 5)
  {
    throw new InvalidOperationException("WorldData did not project the authoritative eclipse flag.");
  }

  WorldGrid unqualifiedWorld = new(400, 300);
  DomeSimulationSnapshot unqualifiedSnapshot = new(
    unqualifiedWorld.CreateSnapshot(metadata),
    [],
    [],
    0,
    worldClock: new WorldClockSnapshot(0, 0, true, false, 1));
  using DomeSimulation unqualifiedSimulation = new(unqualifiedSnapshot);
  if (unqualifiedSimulation.TryQueueWorldEvent(
        new WorldEventStartCommand(WorldEventKind.Eclipse, 6)))
  {
    throw new InvalidOperationException("Unqualified eclipse input was accepted.");
  }

  WorldGrid nightWorld = new(400, 300);
  DomeSimulationSnapshot nightSnapshot = new(
    nightWorld.CreateSnapshot(metadata),
    [],
    [],
    0,
    worldClock: new WorldClockSnapshot(0, 0, false, false, 1),
    progression: new WorldProgressionState(
      isHardMode: true,
      defeatedMechanicalBoss: true,
      isEclipse: true));
  using DomeSimulation nightSimulation = new(nightSnapshot);
  nightSimulation.Tick(new SimulationInputBatch());
  if (nightSimulation.CreateWorldProgressionSnapshot().IsEclipse)
  {
    throw new InvalidOperationException("Eclipse did not end at the night boundary.");
  }
}

static void VerifyInvasionStateMachine()
{
  try
  {
    _ = new WorldProgressionState(invasionType: 2, invasionSize: 10, invasionSizeStart: 9);
    throw new InvalidOperationException("An inconsistent invasion original size was accepted.");
  }
  catch (ArgumentException)
  {
  }

  WorldMetadata metadata = new(
    "invasion-state",
    new WorldSeed(23),
    400,
    300,
    spawnX: 200,
    spawnY: 75);
  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  if (!simulation.TryQueueWorldInvasion(new WorldInvasionStartCommand(2, 40, 7)) ||
      simulation.TryQueueWorldInvasion(new WorldInvasionStartCommand(3, 40, 8)) ||
      simulation.TryQueueWorldInvasion(new WorldInvasionStartCommand(0, 40, 9)))
  {
    throw new InvalidOperationException("Invasion start validation was not deterministic.");
  }

  simulation.Tick(new SimulationInputBatch());
  WorldProgressionState started = simulation.CreateWorldProgressionSnapshot();
  if (started.InvasionType != 2 || started.InvasionSize != 40 ||
      started.InvasionSizeStart != 40 ||
      !simulation.TryQueueWorldInvasionProgress(new WorldInvasionProgressCommand(15, 10)) ||
      simulation.TryQueueWorldInvasionProgress(new WorldInvasionProgressCommand(5, 10)))
  {
    throw new InvalidOperationException(
      "Invasion progress did not reject a duplicate pending sequence.");
  }

  simulation.Tick(new SimulationInputBatch());
  WorldProgressionState reduced = simulation.CreateWorldProgressionSnapshot();
  if (reduced.InvasionType != 2 || reduced.InvasionSize != 25 || reduced.InvasionSizeStart != 40)
  {
    throw new InvalidOperationException("Invasion progress did not commit deterministically.");
  }

  DomeSimulationSnapshot persisted = simulation.CreatePersistenceSnapshot(metadata);
  using DomeSimulation restored = new(persisted);
  if (restored.CreateWorldProgressionSnapshot() != reduced)
  {
    throw new InvalidOperationException("Invasion state did not survive snapshot continuation.");
  }

  using DomeServer invasionServer = new(persisted);
  if (invasionServer.CreateWorldDataContext().Progression.InvasionType != 2)
  {
    throw new InvalidOperationException("WorldData did not project the authoritative invasion type.");
  }

  if (!simulation.TryQueueWorldInvasionProgress(new WorldInvasionProgressCommand(25, 11)))
  {
    throw new InvalidOperationException("Valid invasion completion progress was rejected.");
  }

  simulation.Tick(new SimulationInputBatch());
  WorldProgressionState completed = simulation.CreateWorldProgressionSnapshot();
  if (completed.InvasionType != 0 || completed.InvasionSize != 0 ||
      completed.InvasionSizeStart != 0 ||
      !completed.DefeatedFrost || completed.DefeatedGoblins ||
      completed.DefeatedPirates || completed.DefeatedMartians ||
      simulation.TryQueueWorldInvasionProgress(new WorldInvasionProgressCommand(1, 12)))
  {
    throw new InvalidOperationException("Completed invasion state was not normalized or rejected.");
  }

  if (simulation.CreateWorldInvasionCompletedEvents().Count != 1 ||
      simulation.CreateWorldInvasionCompletedEvents()[0].InvasionType != 2 ||
      simulation.CreateWorldInvasionCompletedEvents()[0].ClearFlag != WorldInvasionClearFlag.Frost)
  {
    throw new InvalidOperationException(
      "Invasion completion did not publish its authoritative source type.");
  }
}

static void VerifyInvasionSpawnEligibility()
{
  WorldInvasionSpawnEligibilitySystem system = new();
  if (!system.CanSpawn(1, 10, 0) ||
      system.CanSpawn(0, 10, 0) ||
      system.CanSpawn(1, 0, 0) ||
      system.CanSpawn(1, 10, 1) ||
      system.CanSpawn(1, 10, -1))
  {
    throw new InvalidOperationException(
      "Invasion spawn eligibility did not preserve the source guard.");
  }

  Console.WriteLine("PASS: invasion spawn eligibility preserves the source delay guard");
}

static void VerifyInvasionPositionGuard()
{
  WorldInvasionPositionGuardSystem system = new();
  if (!system.IsWithinSpawnWindow(1000.0, 100.0, 100.0, 80.0, 100) ||
      !system.IsWithinSpawnWindow(1000.0, 2000.0, 100.0, 80.0, 100) ||
      system.IsWithinSpawnWindow(1000.0, 3000.0, 100.0, 80.0, 80) ||
      system.IsWithinSpawnWindow(5000.0, 100.0, 100.0, 80.0, 100) ||
      system.IsWithinSpawnWindow(double.NaN, 100.0, 100.0, 80.0, 100))
  {
    throw new InvalidOperationException(
      "Invasion position guard did not preserve the source surface and window rules.");
  }

  Console.WriteLine("PASS: invasion position guard preserves source surface and window rules");
}

static void VerifyInvasionTownFallbackGuard()
{
  WorldInvasionTownFallbackGuardSystem system = new();
  if (!system.IsEligible(200.0, 400, 205.0, 1000.0, true) ||
      system.IsEligible(190.0, 400, 205.0, 1000.0, true) ||
      system.IsEligible(200.0, 400, 205.0, 4001.0, true) ||
      system.IsEligible(200.0, 400, 205.0, 1000.0, false) ||
      system.IsEligible(double.NaN, 400, 205.0, 1000.0, true))
  {
    throw new InvalidOperationException(
      "Invasion town-NPC fallback guard did not preserve source center and distance rules.");
  }

  Console.WriteLine("PASS: invasion town-NPC fallback guard preserves source rules");
}

static void VerifyInvasionDelayPolicy()
{
  WorldInvasionDelaySystem system = new();
  if (system.AdvanceAtDayStart(3) != 2 ||
      system.AdvanceAtDayStart(1) != 0 ||
      system.AdvanceAtDayStart(0) != 0)
  {
    throw new InvalidOperationException(
      "Invasion delay did not decrement exactly once at day start.");
  }

  try
  {
    _ = system.AdvanceAtDayStart(-1);
    throw new InvalidOperationException("A negative invasion delay was accepted.");
  }
  catch (ArgumentOutOfRangeException)
  {
  }

  Console.WriteLine("PASS: invasion delay policy preserves the source day-start decrement");
}

static void VerifyInvasionDelayTickIntegration()
{
  WorldProgressionSystem system = new();
  WorldProgressionState delayed = new(invasionDelayTicks: 3);
  WorldProgressionState dawn = system.Advance(
    new WorldClockSnapshot(10, 0, true, false, 1),
    delayed,
    [],
    [],
    [],
    [],
    [],
    []);
  WorldProgressionState midday = system.Advance(
    new WorldClockSnapshot(11, 1, true, false, 1),
    delayed,
    [],
    [],
    [],
    [],
    [],
    []);
  if (dawn.InvasionDelayTicks != 2 || midday.InvasionDelayTicks != 3)
  {
    throw new InvalidOperationException(
      "Invasion delay was not tied to the authoritative dawn boundary.");
  }

  Console.WriteLine("PASS: invasion delay integrates only at the authoritative dawn boundary");
}

static void VerifyInvasionProgressProjection()
{
  WorldInvasionProgressProjectionSystem system = new();
  WorldInvasionProgressResult result = system.Resolve(
    new WorldProgressionState(invasionType: 2, invasionSize: 25, invasionSizeStart: 40));
  if (!result.IsAvailable || result.Progress != 15 || result.ProgressMax != 40 || result.Icon != 5)
  {
    throw new InvalidOperationException("Invasion progress projection did not preserve source values.");
  }

  WorldInvasionProgressResult unknown = system.Resolve(
    new WorldProgressionState(invasionType: 2, invasionSize: 25));
  if (unknown.IsAvailable)
  {
    throw new InvalidOperationException(
      "Invasion progress projection fabricated an unavailable original size.");
  }

  Console.WriteLine("PASS: invasion progress projection preserves source values and unknown state");
}

static void VerifyRainStateMachine()
{
  WorldMetadata metadata = new(
    "rain-state",
    new WorldSeed(24),
    400,
    300,
    spawnX: 200,
    spawnY: 75);
  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  WorldRuleState initial = simulation.CreateWorldRuleState();
  if (simulation.TryQueueWorldRain(new WorldRainStartCommand(0, 0.5f, 13)) ||
      simulation.TryQueueWorldRain(new WorldRainStartCommand(3, 0.0f, 14)) ||
      !simulation.TryQueueWorldRain(new WorldRainStartCommand(3, 0.5f, 15)) ||
      simulation.TryQueueWorldRain(new WorldRainStartCommand(3, 0.5f, 16)))
  {
    throw new InvalidOperationException("Rain start validation was not deterministic.");
  }

  simulation.Tick(new SimulationInputBatch());
  WorldRuleState started = simulation.CreateWorldRuleState();
  if (!started.IsRaining || started.RainTimeTicks != 3 || started.RainStrength != 0.5f)
  {
    throw new InvalidOperationException("Accepted rain input did not commit authoritative state.");
  }

  DomeSimulationSnapshot startedSnapshot = simulation.CreatePersistenceSnapshot(metadata);
  using DomeServer rainServer = new(startedSnapshot);
  if (rainServer.CreateWorldDataContext().Background.MaximumRaining != 0.5f)
  {
    throw new InvalidOperationException("WorldData did not project authoritative rain strength.");
  }

  simulation.Tick(new SimulationInputBatch());
  WorldRuleState advanced = simulation.CreateWorldRuleState();
  if (!advanced.IsRaining || advanced.RainTimeTicks != 2)
  {
    throw new InvalidOperationException("Rain duration did not advance with simulation time.");
  }

  DomeSimulationSnapshot persisted = simulation.CreatePersistenceSnapshot(metadata);
  using DomeSimulation restored = new(persisted);
  restored.Tick(new SimulationInputBatch());
  WorldRuleState restoredState = restored.CreateWorldRuleState();
  if (!restoredState.IsRaining || restoredState.RainTimeTicks != 1)
  {
    throw new InvalidOperationException("Rain state did not survive snapshot continuation.");
  }

  restored.Tick(new SimulationInputBatch());
  WorldRuleState completed = restored.CreateWorldRuleState();
  if (completed.IsRaining || completed.RainTimeTicks != 0 || completed.RainStrength != 0.0f ||
      completed != initial)
  {
    throw new InvalidOperationException("Expired rain was not normalized to a clear weather state.");
  }
}

static void VerifyRawRainStateModel()
{
  WorldRuleState inactiveSavedRain = new(
    rainTimeTicks: 120,
    rainStrength: 0.25f,
    isRaining: false,
    maximumRainStrength: 0.75f);
  if (inactiveSavedRain.IsRaining || inactiveSavedRain.RainTimeTicks != 120 ||
      inactiveSavedRain.RainStrength != 0.25f || inactiveSavedRain.MaximumRainStrength != 0.75f)
  {
    throw new InvalidOperationException(
      "Raw weather state did not preserve independent WLD rain activity, time and maximum strength.");
  }

  WorldRuleState continuedRain = inactiveSavedRain.WithRain(60, 0.1f);
  if (!continuedRain.IsRaining || continuedRain.RainStrength != 0.1f ||
      continuedRain.RainTimeTicks != 60 || continuedRain.MaximumRainStrength != 0.75f)
  {
    throw new InvalidOperationException(
      "Rain continuation overwrote the independent maximum rain strength.");
  }

  Console.WriteLine("PASS: rain continuation preserves independent maximum strength");
}

static void VerifyWorldEventRandomState()
{
  WorldEventRandomState first = new(17U);
  WorldEventRandomState second = new(17U);
  (WorldEventRandomState firstNext, int firstValue) = first.NextExclusive(100);
  (WorldEventRandomState secondNext, int secondValue) = second.NextExclusive(100);
  if (firstValue != secondValue || firstNext != secondNext || firstNext == first)
  {
    throw new InvalidOperationException(
      "World-event random state did not produce a reproducible advancing sequence.");
  }

  (_, int fullRangeValue) = first.NextInclusive(int.MinValue, int.MaxValue);
  if (fullRangeValue < int.MinValue || fullRangeValue > int.MaxValue)
  {
    throw new InvalidOperationException(
      "World-event random state rejected the full Int32 inclusive range.");
  }
}

static void VerifyWorldEventRandomStateSnapshotContinuity()
{
  WorldMetadata metadata = new("random-snapshot", new WorldSeed(29), 400, 300);
  WorldGrid world = new(width: 400, height: 300);
  WorldEventRandomState expected = new(987654321U);
  DomeSimulationSnapshot source = new(
    world.CreateSnapshot(metadata),
    [],
    [],
    tickNumber: 0,
    worldEventRandomState: expected);
  using DomeSimulation simulation = new(source);
  DomeSimulationSnapshot restored = simulation.CreatePersistenceSnapshot(metadata);
  if (restored.WorldEventRandomState != expected)
  {
    throw new InvalidOperationException(
      "Simulation persistence snapshot did not retain the world-event random state.");
  }
}

static void VerifySlimeRainStateMachine()
{
  WorldMetadata metadata = new(
    "slime-rain-state",
    new WorldSeed(25),
    400,
    300,
    spawnX: 200,
    spawnY: 75);
  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  if (simulation.TryQueueWorldSlimeRain(new WorldSlimeRainStartCommand(0, 17)) ||
      !simulation.TryQueueWorldSlimeRain(new WorldSlimeRainStartCommand(3, 18)) ||
      simulation.TryQueueWorldSlimeRain(new WorldSlimeRainStartCommand(3, 19)))
  {
    throw new InvalidOperationException("Slime-rain start validation was not deterministic.");
  }

  simulation.Tick(new SimulationInputBatch());
  WorldProgressionState started = simulation.CreateWorldProgressionSnapshot();
  if (!started.IsSlimeRaining || started.SlimeRainTimeTicks != 3)
  {
    throw new InvalidOperationException("Accepted slime-rain input did not commit authoritative state.");
  }

  DomeSimulationSnapshot persisted = simulation.CreatePersistenceSnapshot(metadata);
  using DomeSimulation restored = new(persisted);
  restored.Tick(new SimulationInputBatch());
  WorldProgressionState continued = restored.CreateWorldProgressionSnapshot();
  if (!continued.IsSlimeRaining || continued.SlimeRainTimeTicks != 2)
  {
    throw new InvalidOperationException("Slime-rain state did not survive snapshot continuation.");
  }

  using DomeServer slimeRainServer = new(persisted);
  if ((slimeRainServer.CreateWorldDataContext().Progression.EventFlags3 & 4) == 0)
  {
    throw new InvalidOperationException("WorldData did not project the authoritative slime-rain flag.");
  }

  restored.Tick(new SimulationInputBatch());
  restored.Tick(new SimulationInputBatch());
  WorldProgressionState completed = restored.CreateWorldProgressionSnapshot();
  if (completed.IsSlimeRaining || completed.SlimeRainTimeTicks != 0)
  {
    throw new InvalidOperationException("Expired slime rain was not normalized to a clear state.");
  }

  using DomeSimulation rainingSimulation = new(new WorldGrid(400, 300));
  if (!rainingSimulation.TryQueueWorldRain(new WorldRainStartCommand(3, 0.5f, 20)))
  {
    throw new InvalidOperationException("Rain conflict fixture could not start rain.");
  }

  rainingSimulation.Tick(new SimulationInputBatch());
  if (rainingSimulation.TryQueueWorldSlimeRain(new WorldSlimeRainStartCommand(3, 21)))
  {
    throw new InvalidOperationException("Slime rain was accepted while ordinary rain was active.");
  }
}

static void VerifySlimeRainCooldownStateMachine()
{
  WorldMetadata metadata = new(
    "slime-rain-cooldown",
    new WorldSeed(30),
    400,
    300,
    spawnX: 200,
    spawnY: 75,
    worldSurface: 100,
    isRemixWorld: false);
  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  if (!simulation.TryQueueWorldSlimeRain(new WorldSlimeRainStartCommand(4, 30)))
  {
    throw new InvalidOperationException("Cooldown fixture could not start Slime Rain.");
  }

  simulation.Tick(new SimulationInputBatch());
  if (!simulation.TryQueueWorldSlimeRainStop(new WorldSlimeRainStopCommand(3, 31)) ||
      simulation.TryQueueWorldSlimeRainStop(new WorldSlimeRainStopCommand(2, 32)))
  {
    throw new InvalidOperationException("Slime-rain stop validation was not deterministic.");
  }

  simulation.Tick(new SimulationInputBatch());
  WorldProgressionState stopped = simulation.CreateWorldProgressionSnapshot();
  if (stopped.IsSlimeRaining || stopped.SlimeRainTimeTicks != 0 ||
      !stopped.IsSlimeRainCoolingDown || stopped.SlimeRainCooldownTicks != 3 ||
      simulation.TryQueueWorldSlimeRain(new WorldSlimeRainStartCommand(3, 33)))
  {
    throw new InvalidOperationException("Slime-rain cooldown did not block immediate restart.");
  }

  DomeSimulationSnapshot persisted = simulation.CreatePersistenceSnapshot(metadata);
  using DomeSimulation restored = new(persisted);
  if (restored.CreateWorldProgressionSnapshot().SlimeRainCooldownTicks != 3)
  {
    throw new InvalidOperationException("Slime-rain cooldown did not survive persistence.");
  }

  restored.Tick(new SimulationInputBatch());
  restored.Tick(new SimulationInputBatch());
  restored.Tick(new SimulationInputBatch());
  WorldProgressionState ready = restored.CreateWorldProgressionSnapshot();
  if (ready.IsSlimeRainCoolingDown || ready.SlimeRainCooldownTicks != 0 ||
      !restored.TryQueueWorldSlimeRain(new WorldSlimeRainStartCommand(3, 34)))
  {
    throw new InvalidOperationException("Slime-rain cooldown did not expire deterministically.");
  }

  using DomeSimulation idle = new(new WorldGrid(400, 300));
  if (idle.TryQueueWorldSlimeRainStop(new WorldSlimeRainStopCommand(3, 35)))
  {
    throw new InvalidOperationException("Slime-rain stop was accepted without active rain.");
  }
}

static void VerifySlimeRainEligibilityMetadata()
{
  WorldSlimeRainEligibilitySystem system = new();
  WorldSlimeRainEligibilityResult normal = system.Evaluate(
    new WorldMetadata(
      "normal",
      new WorldSeed(1),
      400,
      300,
      worldSurface: 100,
      isRemixWorld: false));
  if (!normal.IsEligible || normal.Rejection != WorldSlimeRainStartRejection.None)
  {
    throw new InvalidOperationException("A normal world with a surface was rejected.");
  }

  WorldSlimeRainEligibilityResult remix = system.Evaluate(
    new WorldMetadata(
      "remix",
      new WorldSeed(2),
      400,
      300,
      worldSurface: 100,
      isRemixWorld: true));
  if (remix.IsEligible || remix.Rejection != WorldSlimeRainStartRejection.RemixWorld)
  {
    throw new InvalidOperationException("A remix world was not rejected.");
  }

  WorldSlimeRainEligibilityResult missingSurface = system.Evaluate(
    new WorldMetadata(
      "underground",
      new WorldSeed(3),
      400,
      300,
      worldSurface: 50,
      isRemixWorld: false));
  if (missingSurface.IsEligible ||
      missingSurface.Rejection != WorldSlimeRainStartRejection.NoWorldSurface)
  {
    throw new InvalidOperationException("A world without a usable surface was accepted.");
  }

  WorldSlimeRainEligibilityResult unknown = system.Evaluate(
    new WorldMetadata("unknown", new WorldSeed(4), 400, 300, worldSurface: 100));
  if (unknown.IsEligible ||
      unknown.Rejection != WorldSlimeRainStartRejection.UnknownWorldVariant)
  {
    throw new InvalidOperationException("Unknown world variant was treated as a normal world.");
  }

  using DomeSimulation remixSimulation = new(new WorldGrid(400, 300));
  _ = remixSimulation.CreatePersistenceSnapshot(
    new WorldMetadata(
      "remix-runtime",
      new WorldSeed(5),
      400,
      300,
      worldSurface: 100,
      isRemixWorld: true));
  if (remixSimulation.TryQueueWorldSlimeRain(new WorldSlimeRainStartCommand(3, 5)))
  {
    throw new InvalidOperationException(
      "The authoritative Slime Rain command route accepted a Remix world.");
  }

  Console.WriteLine("PASS: Slime Rain metadata eligibility is explicit and fail-closed");
}

static void VerifySlimeRainWarningStateMachine()
{
  using DomeSimulation starting = new(new WorldGrid(400, 300));
  if (!starting.TryQueueWorldSlimeRain(new WorldSlimeRainStartCommand(500, 40, true)))
  {
    throw new InvalidOperationException("Warning fixture could not start Slime Rain.");
  }

  starting.Tick(new SimulationInputBatch());
  WorldProgressionState started = starting.CreateWorldProgressionSnapshot();
  if (started.SlimeRainWarningTicks != 419 ||
      starting.CreateWorldSlimeRainWarningEvents().Count != 0)
  {
    throw new InvalidOperationException(
      "Announced Slime Rain did not start its 420-tick warning countdown.");
  }

  for (int index = 0; index < 418; index++)
  {
    starting.Tick(new SimulationInputBatch());
  }

  if (starting.CreateWorldProgressionSnapshot().SlimeRainWarningTicks != 1)
  {
    throw new InvalidOperationException("Slime Rain warning countdown did not advance deterministically.");
  }

  starting.Tick(new SimulationInputBatch());
  IReadOnlyList<WorldSlimeRainWarningEvent> startedEvents =
    starting.CreateWorldSlimeRainWarningEvents();
  if (starting.CreateWorldProgressionSnapshot().SlimeRainWarningTicks != 0 ||
      startedEvents.Count != 1 || !startedEvents[0].IsSlimeRaining)
  {
    throw new InvalidOperationException(
      "Slime Rain warning did not emit exactly one active-state event at expiry.");
  }

  starting.Tick(new SimulationInputBatch());
  if (starting.CreateWorldSlimeRainWarningEvents().Count != 0)
  {
    throw new InvalidOperationException("Expired Slime Rain warning was emitted more than once.");
  }

  using DomeSimulation stopping = new(new WorldGrid(400, 300));
  if (!stopping.TryQueueWorldSlimeRain(new WorldSlimeRainStartCommand(500, 42, true)))
  {
    throw new InvalidOperationException("Stop-warning fixture could not start Slime Rain.");
  }

  stopping.Tick(new SimulationInputBatch());
  if (!stopping.TryQueueWorldSlimeRainStop(new WorldSlimeRainStopCommand(3, 43, true)))
  {
    throw new InvalidOperationException("Stop-warning fixture could not stop Slime Rain.");
  }

  stopping.Tick(new SimulationInputBatch());
  for (int index = 0; index < 418; index++)
  {
    stopping.Tick(new SimulationInputBatch());
  }

  stopping.Tick(new SimulationInputBatch());
  IReadOnlyList<WorldSlimeRainWarningEvent> stoppedEvents =
    stopping.CreateWorldSlimeRainWarningEvents();
  if (stoppedEvents.Count != 1 || stoppedEvents[0].IsSlimeRaining)
  {
    throw new InvalidOperationException(
      "Stopped Slime Rain warning did not emit exactly one inactive-state event.");
  }

  using DomeSimulation silent = new(new WorldGrid(400, 300));
  if (!silent.TryQueueWorldSlimeRain(new WorldSlimeRainStartCommand(5, 41, false)))
  {
    throw new InvalidOperationException("Silent Slime Rain start was rejected.");
  }

  silent.Tick(new SimulationInputBatch());
  if (silent.CreateWorldProgressionSnapshot().SlimeRainWarningTicks != 0 ||
      silent.CreateWorldSlimeRainWarningEvents().Count != 0)
  {
    throw new InvalidOperationException("announce=false unexpectedly created a Slime Rain warning.");
  }
}

static void VerifyMeteorImpactStateMachine()
{
  WorldMetadata metadata = new(
    "meteor-impact-state",
    new WorldSeed(24),
    400,
    300,
    spawnX: 200,
    spawnY: 75);

  using DomeSimulation first = new(new WorldGrid(400, 300));
  using DomeSimulation second = new(new WorldGrid(400, 300));
  WorldSectionCoordinates section = new(1, 1);
  long firstVersion = first.WorldGrid.GetSectionVersion(section);
  WorldMeteorImpactCommand command = new(200, 150, 11);
  if (!first.TryQueueWorldMeteorImpact(command) ||
      first.TryQueueWorldMeteorImpact(command))
  {
    throw new InvalidOperationException("Meteor impact request validation was not deterministic.");
  }

  if (!second.TryQueueWorldMeteorImpact(command))
  {
    throw new InvalidOperationException("The same valid meteor impact was not accepted in a peer simulation.");
  }

  first.Tick(new SimulationInputBatch());
  second.Tick(new SimulationInputBatch());
  int firstMeteoriteTiles = CountTiles(first.WorldGrid, 37);
  int secondMeteoriteTiles = CountTiles(second.WorldGrid, 37);
  if (firstMeteoriteTiles == 0 || firstMeteoriteTiles != secondMeteoriteTiles ||
      first.WorldGrid.GetSectionVersion(section) <= firstVersion)
  {
    throw new InvalidOperationException(
      "Meteor impact did not produce a deterministic tile set and section-version change.");
  }

  for (int y = 110; y < 190; y++)
  {
    for (int x = 160; x < 240; x++)
    {
      if (first.WorldGrid.GetTile(x, y) != second.WorldGrid.GetTile(x, y))
      {
        throw new InvalidOperationException("Equivalent meteor commands produced different tiles.");
      }
    }
  }

  DomeSimulationSnapshot persisted = first.CreatePersistenceSnapshot(metadata);
  using DomeSimulation restored = new(persisted);
  if (CountTiles(restored.WorldGrid, 37) != firstMeteoriteTiles)
  {
    throw new InvalidOperationException("Meteor impact tiles did not survive snapshot continuation.");
  }

  using DomeSimulation protectedSimulation = new(new WorldGrid(400, 300));
  if (!protectedSimulation.WorldGrid.TrySetTile(
        200,
        150,
        new WorldTile(IsActive: true, Type: 26)) ||
      protectedSimulation.TryQueueWorldMeteorImpact(new WorldMeteorImpactCommand(200, 150, 12)))
  {
    throw new InvalidOperationException("Protected meteor impact area was accepted.");
  }

  using DomeSimulation occupiedSimulation = new(new WorldGrid(400, 300));
  _ = occupiedSimulation.CreatePlayer(new SimulationVector(200.0f, 150.0f));
  if (occupiedSimulation.TryQueueWorldMeteorImpact(new WorldMeteorImpactCommand(200, 150, 13)))
  {
    throw new InvalidOperationException("Meteor impact intersecting a player safety area was accepted.");
  }

  if (first.TryQueueWorldMeteorImpact(new WorldMeteorImpactCommand(35, 150, 14)))
  {
    throw new InvalidOperationException("Out-of-bounds meteor impact was accepted.");
  }
}

static int CountTiles(WorldGrid world, ushort tileType)
{
  int count = 0;
  for (int y = 0; y < world.Height; y++)
  {
    for (int x = 0; x < world.Width; x++)
    {
      if (world.GetTile(x, y).IsActive && world.GetTile(x, y).Type == tileType)
      {
        count++;
      }
    }
  }

  return count;
}

static void VerifyWindStateMachine()
{
  WorldMetadata metadata = new(
    "wind-state",
    new WorldSeed(25),
    400,
    300,
    spawnX: 200,
    spawnY: 75);
  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  if (!simulation.TryQueueWorldWind(new WorldWindChangeCommand(0.4f, 1)) ||
      simulation.TryQueueWorldWind(new WorldWindChangeCommand(0.8f, 2)) ||
      simulation.TryQueueWorldWind(new WorldWindChangeCommand(float.NaN, 3)) ||
      simulation.TryQueueWorldWind(new WorldWindChangeCommand(0.9f, 4)))
  {
    throw new InvalidOperationException("Wind target validation was not deterministic.");
  }

  simulation.Tick(new SimulationInputBatch());
  WorldRuleState rules = simulation.CreateWorldRuleState();
  if (rules.WindSpeedTarget != 0.4f || rules.WindSpeedCurrent <= 0.0f ||
      rules.WindSpeedCurrent >= rules.WindSpeedTarget)
  {
    throw new InvalidOperationException("Accepted wind target did not advance deterministically.");
  }

  using DomeServer server = new(simulation.CreatePersistenceSnapshot(metadata));
  LegacyWorldDataContext context = server.CreateWorldDataContext();
  if (context.Background.WindSpeedTarget != 0.4f)
  {
    throw new InvalidOperationException("WorldData did not project authoritative wind target.");
  }

  DomeSimulationSnapshot persisted = simulation.CreatePersistenceSnapshot(metadata);
  using DomeSimulation restored = new(persisted);
  if (restored.CreateWorldRuleState() != rules)
  {
    throw new InvalidOperationException("Wind state did not survive snapshot continuation.");
  }
}

static void VerifyWindRainCoupling()
{
  WorldRuleState clearRules = new(windSpeedTarget: 0.4f, windSpeedCurrent: 0.0f);
  WorldRuleState rainingRules = new(
    rainTimeTicks: 3,
    rainStrength: 0.5f,
    windSpeedTarget: 0.4f,
    windSpeedCurrent: 0.0f);
  WorldWeatherSystem weather = new();
  WorldClockSnapshot clock = new(0, 0, true, false, 1);
  WorldRuleState clearNext = weather.Advance(clock, clearRules, [], []);
  WorldRuleState rainingNext = weather.Advance(clock, rainingRules, [], []);
  float expectedRainingCurrent = 0.0003f + (0.4f * (1.0f + 5.0f / 9.0f * 0.5f)) * 0.0015f;
  if (rainingNext.WindSpeedCurrent <= clearNext.WindSpeedCurrent ||
      MathF.Abs(rainingNext.WindSpeedCurrent - expectedRainingCurrent) > 0.000001f ||
      rainingNext.RainTimeTicks != 2)
  {
    throw new InvalidOperationException(
      "Wind current did not use the Version4 rain-adjusted smoothing target.");
  }
}

static void VerifyLanternNightStateMachine()
{
  WorldMetadata metadata = new(
    "lantern-night-state",
    new WorldSeed(26),
    400,
    300,
    spawnX: 200,
    spawnY: 75);
  WorldGrid world = new(400, 300);
  DomeSimulationSnapshot nightSnapshot = new(
    world.CreateSnapshot(metadata),
    [],
    [],
    0,
    worldClock: new WorldClockSnapshot(0, 1, false, false, 1));
  using DomeSimulation nightSimulation = new(nightSnapshot);
  if (!nightSimulation.TryQueueWorldEvent(new WorldEventStartCommand(WorldEventKind.LanternNight, 1)) ||
      nightSimulation.TryQueueWorldEvent(new WorldEventStartCommand(WorldEventKind.LanternNight, 2)))
  {
    throw new InvalidOperationException("Lantern-night request validation was not deterministic.");
  }

  nightSimulation.Tick(new SimulationInputBatch());
  if (!nightSimulation.CreateWorldProgressionSnapshot().IsLanternNight)
  {
    throw new InvalidOperationException("Accepted nighttime request did not start Lantern Night.");
  }

  using DomeServer server = new(nightSimulation.CreatePersistenceSnapshot(metadata));
  if ((server.CreateWorldDataContext().Progression.EventFlags11 & 2) == 0)
  {
    throw new InvalidOperationException("WorldData did not project Lantern Night into EventFlags11 bit 1.");
  }

  WorldGrid dayWorld = new(400, 300);
  DomeSimulationSnapshot daySnapshot = new(
    dayWorld.CreateSnapshot(metadata),
    [],
    [],
    0,
    worldClock: new WorldClockSnapshot(0, 0, true, false, 1),
    progression: new WorldProgressionState(isLanternNight: true));
  using DomeSimulation daySimulation = new(daySnapshot);
  daySimulation.Tick(new SimulationInputBatch());
  if (daySimulation.CreateWorldProgressionSnapshot().IsLanternNight ||
      daySimulation.TryQueueWorldEvent(new WorldEventStartCommand(WorldEventKind.LanternNight, 3)))
  {
    throw new InvalidOperationException("Lantern Night did not clear at the daytime boundary.");
  }
}

static void VerifyLanternNightRainSuppression()
{
  WorldMetadata metadata = new(
    "lantern-rain-suppression",
    new WorldSeed(29),
    400,
    300,
    spawnX: 200,
    spawnY: 75);
  WorldGrid world = new(400, 300);
  DomeSimulationSnapshot snapshot = new(
    world.CreateSnapshot(metadata),
    [],
    [],
    0,
    worldClock: new WorldClockSnapshot(0, 1, false, false, 1),
    worldRules: new WorldRuleState(rainTimeTicks: 3, rainStrength: 0.5f),
    progression: new WorldProgressionState(isLanternNight: true));
  using DomeSimulation simulation = new(snapshot);
  if (simulation.TryQueueWorldRain(new WorldRainStartCommand(3, 0.5f, 1)))
  {
    throw new InvalidOperationException("Rain was accepted while Lantern Night was active.");
  }

  simulation.Tick(new SimulationInputBatch());
  WorldRuleState rules = simulation.CreateWorldRuleState();
  if (rules.IsRaining || rules.RainTimeTicks != 0 || rules.RainStrength != 0.0f)
  {
    throw new InvalidOperationException("Lantern Night did not stop active rain.");
  }
}

static void VerifyLanternNightSameTickRainOrdering()
{
  WorldMetadata metadata = new(
    "lantern-same-tick-ordering",
    new WorldSeed(31),
    400,
    300,
    spawnX: 200,
    spawnY: 75);
  WorldGrid world = new(400, 300);
  DomeSimulationSnapshot snapshot = new(
    world.CreateSnapshot(metadata),
    [],
    [],
    0,
    worldClock: new WorldClockSnapshot(0, 1, false, false, 1),
    worldRules: new WorldRuleState(rainTimeTicks: 3, rainStrength: 0.5f));
  using DomeSimulation simulation = new(snapshot);
  if (!simulation.TryQueueWorldEvent(
        new WorldEventStartCommand(WorldEventKind.LanternNight, 1)))
  {
    throw new InvalidOperationException("Same-tick Lantern Night request was rejected.");
  }

  simulation.Tick(new SimulationInputBatch());
  if (!simulation.CreateWorldProgressionSnapshot().IsLanternNight ||
      simulation.CreateWorldRuleState().IsRaining)
  {
    throw new InvalidOperationException(
      "Lantern Night did not suppress existing Rain in the same simulation tick.");
  }

  DomeSimulationSnapshot conflictSnapshot = new(
    new WorldGrid(400, 300).CreateSnapshot(metadata),
    [],
    [],
    0,
    worldClock: new WorldClockSnapshot(0, 1, false, false, 1));
  using DomeSimulation startConflict = new(conflictSnapshot);
  if (!startConflict.TryQueueWorldRain(new WorldRainStartCommand(3, 0.5f, 2)) ||
      !startConflict.TryQueueWorldEvent(
        new WorldEventStartCommand(WorldEventKind.LanternNight, 3)))
  {
    throw new InvalidOperationException("Same-tick Rain/Lantern Night conflict was not queueable.");
  }

  startConflict.Tick(new SimulationInputBatch());
  if (startConflict.CreateWorldRuleState().IsRaining ||
      !startConflict.CreateWorldProgressionSnapshot().IsLanternNight)
  {
    throw new InvalidOperationException(
      "Lantern Night did not win the same-tick Rain start conflict.");
  }
}

static void VerifyLanternNightScheduleStateMachine()
{
  WorldMetadata metadata = new(
    "lantern-schedule-state",
    new WorldSeed(32),
    400,
    300,
    spawnX: 200,
    spawnY: 75);
  WorldGrid world = new(400, 300);
  DomeSimulationSnapshot snapshot = new(
    world.CreateSnapshot(metadata),
    [],
    [],
    0,
    worldClock: new WorldClockSnapshot(
      0,
      WorldClock.DefaultDayLengthTicks - 2,
      true,
      false,
      1));
  using DomeSimulation simulation = new(snapshot);
  NpcGameEventFirstClearCommand schedule = new(1, 1);
  if (!simulation.TryQueueNpcGameEventFirstClear(schedule) ||
      simulation.TryQueueNpcGameEventFirstClear(schedule) ||
      simulation.TryQueueNpcGameEventFirstClear(new NpcGameEventFirstClearCommand(1, -1)))
  {
    throw new InvalidOperationException(
      "Lantern Night schedule authority did not reject duplicate or invalid requests.");
  }

  simulation.Tick(new SimulationInputBatch());
  WorldProgressionState daytime = simulation.CreateWorldProgressionSnapshot();
  if (daytime.IsLanternNight || !daytime.IsNextNightLanternNight)
  {
    throw new InvalidOperationException(
      "A daytime Lantern Night schedule was consumed before the night phase.");
  }

  simulation.Tick(new SimulationInputBatch());
  WorldProgressionState started = simulation.CreateWorldProgressionSnapshot();
  if (!started.IsLanternNight || started.IsNextNightLanternNight)
  {
    throw new InvalidOperationException(
      "The scheduled Lantern Night did not consume exactly once at night.");
  }

  DomeSimulationSnapshot nextNightSnapshot = new(
    new WorldGrid(400, 300).CreateSnapshot(metadata),
    [],
    [],
    0,
    worldClock: new WorldClockSnapshot(0, 0, false, false, 1),
    progression: started.WithLanternNight(false));
  using DomeSimulation nextNightSimulation = new(nextNightSnapshot);
  nextNightSimulation.Tick(new SimulationInputBatch());
  WorldProgressionState nextNight = nextNightSimulation.CreateWorldProgressionSnapshot();
  if (nextNight.IsLanternNight || nextNight.IsNextNightLanternNight)
  {
    throw new InvalidOperationException(
      "A consumed Lantern Night schedule restarted on a later night.");
  }
}

static void VerifyNpcFirstEventClearSchedulesLanternNight()
{
  WorldMetadata metadata = new(
    "npc-first-event-lantern-schedule",
    new WorldSeed(33),
    400,
    300,
    spawnX: 200,
    spawnY: 75);
  DomeSimulationSnapshot snapshot = new(
    new WorldGrid(400, 300).CreateSnapshot(metadata),
    [],
    [],
    0,
    worldClock: new WorldClockSnapshot(0, 0, true, false, 1));
  using DomeSimulation simulation = new(snapshot);
  NpcGameEventFirstClearCommand eligible = new(1, 1);
  if (!simulation.TryQueueNpcGameEventFirstClear(eligible) ||
      simulation.TryQueueNpcGameEventFirstClear(eligible) ||
      simulation.TryQueueNpcGameEventFirstClear(new NpcGameEventFirstClearCommand(-1, 2)))
  {
    throw new InvalidOperationException(
      "NPC first-event authority did not reject duplicate or invalid commands.");
  }

  simulation.Tick(new SimulationInputBatch());
  if (!simulation.CreateWorldProgressionSnapshot().IsNextNightLanternNight)
  {
    throw new InvalidOperationException(
      "An eligible NPC first-event clear did not schedule Lantern Night.");
  }

  using DomeSimulation excluded = new(snapshot);
  if (excluded.TryQueueNpcGameEventFirstClear(new NpcGameEventFirstClearCommand(4, 3)))
  {
    throw new InvalidOperationException(
      "NPC game event 4 incorrectly scheduled Lantern Night.");
  }

  excluded.Tick(new SimulationInputBatch());
  if (excluded.CreateWorldProgressionSnapshot().IsNextNightLanternNight)
  {
    throw new InvalidOperationException(
      "An excluded NPC first-event clear mutated the Lantern Night schedule.");
  }
}

static void VerifyLanternNightEligibilityContract()
{
  LanternNightEligibilitySystem system = new();
  WorldProgressionState clearProgression = new();
  LanternNightEligibilitySnapshot clearEligibility = new(
    isPumpkinMoon: false,
    isSnowMoon: false,
    moonLordCountdown: 0,
    npcs: []);
  if (!system.CanStart(clearProgression, clearEligibility) ||
      !system.CanPersist(
        new WorldClockSnapshot(0, 1, false, false, 1),
        clearProgression,
        clearEligibility) ||
      system.CanPersist(
        new WorldClockSnapshot(0, 1, true, false, 1),
        clearProgression,
        clearEligibility))
  {
    throw new InvalidOperationException(
      "Clear nighttime Lantern Night eligibility did not match the archive contract.");
  }

  if (system.CanStart(
        clearProgression.WithMeteorScheduled(true),
        clearEligibility) ||
      system.CanStart(clearProgression.WithBloodMoon(true), clearEligibility) ||
      system.CanStart(clearProgression.WithInvasion(1, 1), clearEligibility) ||
      system.CanStart(
        clearProgression,
        new LanternNightEligibilitySnapshot(true, false, 0, [])) ||
      system.CanStart(
        clearProgression,
        new LanternNightEligibilitySnapshot(false, true, 0, [])) ||
      system.CanStart(
        clearProgression,
        new LanternNightEligibilitySnapshot(false, false, 1, [])) ||
      system.CanStart(
        clearProgression,
        new LanternNightEligibilitySnapshot(
          false,
          false,
          0,
          [new LanternNightNpcSnapshot(true, true, 1)])) ||
      system.CanStart(
        clearProgression,
        new LanternNightEligibilitySnapshot(
          false,
          false,
          0,
          [new LanternNightNpcSnapshot(true, false, 13)])))
  {
    throw new InvalidOperationException(
      "A legacy Lantern Night eligibility guard was not enforced.");
  }

  if (!system.CanStart(
        clearProgression,
        new LanternNightEligibilitySnapshot(
          false,
          false,
          0,
          [new LanternNightNpcSnapshot(false, true, 1)])))
  {
    throw new InvalidOperationException(
      "An inactive boss incorrectly blocked Lantern Night eligibility.");
  }
}

static void VerifyNormalEventStartEligibilityContract()
{
  NormalEventEligibilitySystem system = new();
  NormalEventEligibilitySnapshot clear = new(
    isLunarApocalypseActive: false,
    hasActiveLegacyNpc398: false,
    moonLordCountdown: 0);
  if (system.ShouldBlockNormalEvents(isLanternNight: false, clear) ||
      !system.ShouldBlockNormalEvents(isLanternNight: true, clear) ||
      !system.ShouldBlockNormalEvents(
        isLanternNight: false,
        new NormalEventEligibilitySnapshot(true, false, 0)) ||
      !system.ShouldBlockNormalEvents(
        isLanternNight: false,
        new NormalEventEligibilitySnapshot(false, true, 0)) ||
      !system.ShouldBlockNormalEvents(
        isLanternNight: false,
        new NormalEventEligibilitySnapshot(false, false, 1)))
  {
    throw new InvalidOperationException(
      "Normal-event blocking did not preserve the legacy world-event guards.");
  }

  try
  {
    _ = new NormalEventEligibilitySnapshot(false, false, -1);
    throw new InvalidOperationException(
      "Normal-event eligibility accepted a negative Moon Lord countdown.");
  }
  catch (ArgumentOutOfRangeException)
  {
  }
}

static void VerifyKingSlimeReadinessContract()
{
  KingSlimeReadinessSystem system = new();
  if (system.HasReadyPlayer([]) ||
      system.HasReadyPlayer([new KingSlimePlayerSnapshot(false, 400, 80)]) ||
      system.HasReadyPlayer([new KingSlimePlayerSnapshot(true, 140, 9)]) ||
      system.HasReadyPlayer([new KingSlimePlayerSnapshot(true, 141, 8)]) ||
      !system.HasReadyPlayer([new KingSlimePlayerSnapshot(true, 141, 9)]) ||
      !system.HasReadyPlayer([
        new KingSlimePlayerSnapshot(true, 140, 9),
        new KingSlimePlayerSnapshot(true, 141, 9)
      ]))
  {
    throw new InvalidOperationException(
      "King Slime readiness did not preserve the legacy player thresholds.");
  }

  try
  {
    _ = new KingSlimePlayerSnapshot(true, -1, 9);
    throw new InvalidOperationException(
      "King Slime readiness accepted a negative maximum health.");
  }
  catch (ArgumentOutOfRangeException)
  {
  }

  try
  {
    _ = new KingSlimePlayerSnapshot(true, 141, -1);
    throw new InvalidOperationException(
      "King Slime readiness accepted a negative defense value.");
  }
  catch (ArgumentOutOfRangeException)
  {
  }
}

static void VerifyMeteorScheduleStateMachine()
{
  WorldMetadata metadata = new(
    "meteor-schedule-state",
    new WorldSeed(27),
    400,
    300,
    spawnX: 200,
    spawnY: 75);
  WorldGrid world = new(400, 300);
  WorldProgressionState eligibleProgression = new(defeatedEaterOrBrain: true);
  DomeSimulationSnapshot nightSnapshot = new(
    world.CreateSnapshot(metadata),
    [],
    [],
    0,
    worldClock: new WorldClockSnapshot(0, 1, false, false, 1),
    progression: eligibleProgression);
  using DomeSimulation nightSimulation = new(nightSnapshot);
  if (!nightSimulation.TryQueueWorldMeteorSchedule(new WorldMeteorScheduleCommand(1)) ||
      nightSimulation.TryQueueWorldMeteorSchedule(new WorldMeteorScheduleCommand(2)) ||
      nightSimulation.TryQueueWorldMeteorSchedule(new WorldMeteorScheduleCommand(-1)))
  {
    throw new InvalidOperationException("Meteor schedule request validation was not deterministic.");
  }

  nightSimulation.Tick(new SimulationInputBatch());
  if (!nightSimulation.CreateWorldProgressionSnapshot().IsMeteorScheduled)
  {
    throw new InvalidOperationException("Accepted nighttime request did not schedule a meteor.");
  }

  DomeSimulationSnapshot persisted = nightSimulation.CreatePersistenceSnapshot(metadata);
  using DomeSimulation restored = new(persisted);
  if (!restored.CreateWorldProgressionSnapshot().IsMeteorScheduled)
  {
    throw new InvalidOperationException("Meteor schedule did not survive snapshot continuation.");
  }

  DomeSimulationSnapshot dayBoundarySnapshot = new(
    world.CreateSnapshot(metadata),
    [],
    [],
    0,
    worldClock: new WorldClockSnapshot(0, 16199, true, false, 1),
    progression: new WorldProgressionState(
      defeatedEaterOrBrain: true,
      isMeteorScheduled: true));
  using DomeSimulation daySimulation = new(dayBoundarySnapshot);
  daySimulation.Tick(new SimulationInputBatch());
  if (!daySimulation.CreateWorldProgressionSnapshot().IsMeteorScheduled ||
      daySimulation.TimeOfDay != 16200)
  {
    throw new InvalidOperationException("Meteor schedule cleared before the source cutoff.");
  }

  daySimulation.Tick(new SimulationInputBatch());
  if (daySimulation.CreateWorldProgressionSnapshot().IsMeteorScheduled ||
      daySimulation.TimeOfDay != 16201)
  {
    throw new InvalidOperationException("Meteor schedule did not clear after the source cutoff.");
  }

  DomeSimulationSnapshot unqualifiedSnapshot = new(
    world.CreateSnapshot(metadata),
    [],
    [],
    0,
    worldClock: new WorldClockSnapshot(0, 1, false, false, 1));
  using DomeSimulation unqualifiedSimulation = new(unqualifiedSnapshot);
  if (unqualifiedSimulation.TryQueueWorldMeteorSchedule(
        new WorldMeteorScheduleCommand(3)))
  {
    throw new InvalidOperationException("Meteor schedule ignored the source boss qualification.");
  }

  DomeSimulationSnapshot dayRequestSnapshot = new(
    world.CreateSnapshot(metadata),
    [],
    [],
    0,
    worldClock: new WorldClockSnapshot(0, 1, true, false, 1),
    progression: eligibleProgression);
  using DomeSimulation dayRequestSimulation = new(dayRequestSnapshot);
  if (dayRequestSimulation.TryQueueWorldMeteorSchedule(
        new WorldMeteorScheduleCommand(4)))
  {
    throw new InvalidOperationException("Meteor schedule was accepted during daytime.");
  }
}

static void VerifyScheduledMeteorResolution()
{
  WorldMetadata metadata = new(
    "meteor-resolution-state",
    new WorldSeed(28),
    400,
    300,
    spawnX: 200,
    spawnY: 75);
  WorldGrid world = new(400, 300);
  DomeSimulationSnapshot beforeCutoffSnapshot = new(
    world.CreateSnapshot(metadata),
    [],
    [],
    0,
    worldClock: new WorldClockSnapshot(0, 16199, true, false, 1),
    progression: new WorldProgressionState(
      defeatedEaterOrBrain: true,
      isMeteorScheduled: true));
  using DomeSimulation beforeCutoff = new(beforeCutoffSnapshot);
  if (beforeCutoff.TryQueueScheduledWorldMeteorImpact(
        new WorldMeteorImpactCommand(200, 150, 1)))
  {
    throw new InvalidOperationException("Meteor resolution was accepted before the source cutoff.");
  }

  beforeCutoff.Tick(new SimulationInputBatch());
  if (beforeCutoff.TimeOfDay != 16200 || beforeCutoff.TryQueueScheduledWorldMeteorImpact(
        new WorldMeteorImpactCommand(200, 150, 6)))
  {
    throw new InvalidOperationException("Meteor resolution was accepted at the source cutoff.");
  }

  DomeSimulationSnapshot resolutionSnapshot = new(
    world.CreateSnapshot(metadata),
    [],
    [],
    0,
    worldClock: new WorldClockSnapshot(0, 16201, true, false, 1),
    progression: new WorldProgressionState(
      defeatedEaterOrBrain: true,
      isMeteorScheduled: true));
  using DomeSimulation resolution = new(resolutionSnapshot);
  WorldMeteorImpactCommand impact = new(200, 150, 2);
  if (!resolution.TryQueueScheduledWorldMeteorImpact(impact) ||
      resolution.TryQueueScheduledWorldMeteorImpact(new WorldMeteorImpactCommand(201, 150, 3)) ||
      resolution.TryQueueWorldMeteorImpact(new WorldMeteorImpactCommand(202, 150, 4)))
  {
    throw new InvalidOperationException("Scheduled meteor resolution authority was not exclusive.");
  }

  resolution.Tick(new SimulationInputBatch());
  if (resolution.CreateWorldProgressionSnapshot().IsMeteorScheduled ||
      resolution.TimeOfDay != 16202 ||
      CountTiles(resolution.WorldGrid, 37) == 0)
  {
    throw new InvalidOperationException(
      "Scheduled meteor resolution did not clear pending state and commit its impact.");
  }

  DomeSimulationSnapshot unscheduledSnapshot = new(
    world.CreateSnapshot(metadata),
    [],
    [],
    0,
    worldClock: new WorldClockSnapshot(0, 16200, true, false, 1),
    progression: new WorldProgressionState(defeatedEaterOrBrain: true));
  using DomeSimulation unscheduled = new(unscheduledSnapshot);
  if (unscheduled.TryQueueScheduledWorldMeteorImpact(
        new WorldMeteorImpactCommand(200, 150, 5)))
  {
    throw new InvalidOperationException("Unscheduled meteor resolution bypassed pending state.");
  }
}
