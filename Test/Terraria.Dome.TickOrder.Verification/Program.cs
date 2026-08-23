using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.Tick;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Systems;

SimulationTickPhase[] expectedPhases =
[
  SimulationTickPhase.BeginTick,
  SimulationTickPhase.ApplyWorldClock,
  SimulationTickPhase.ApplyPlayerInputs,
  SimulationTickPhase.ApplyPlayerControl,
  SimulationTickPhase.ResolveTileCollision,
  SimulationTickPhase.SelectNpcTargets,
  SimulationTickPhase.ApplyNpcAi,
  SimulationTickPhase.MoveEntities,
  SimulationTickPhase.AdvanceProjectiles,
  SimulationTickPhase.ResolveCombat,
  SimulationTickPhase.CommitDomainCommands,
  SimulationTickPhase.PublishSnapshot,
  SimulationTickPhase.EndTick
];

AssertSequence(
  SimulationTickSchedule.ActivePhases,
  expectedPhases,
  "The named active tick schedule changed.");
Console.WriteLine("PASS: named active phase schedule");

using DomeSimulation simulation = new(new WorldGrid(400, 300));
simulation.Tick(new SimulationInputBatch());
AssertSequence(
  simulation.LastTickPhases,
  expectedPhases,
  "DomeSimulation did not execute the named phase schedule.");
Assert(
  simulation.LastPublishedSnapshot is SimulationSnapshot snapshot &&
  snapshot.Tick == simulation.TickNumber,
  "The tick did not publish the completed authoritative snapshot.");
Console.WriteLine("PASS: tick executes and publishes the named phase schedule");

using DomeSimulation pausedSimulation = new(new WorldGrid(400, 300));
pausedSimulation.SetWorldTimePaused(true);
pausedSimulation.Tick(new SimulationInputBatch());
AssertSequence(
  pausedSimulation.LastTickPhases,
  [
    SimulationTickPhase.BeginTick,
    SimulationTickPhase.ApplyWorldClock,
    SimulationTickPhase.EndTick
  ],
  "A paused simulation tick reached a gameplay phase.");
Assert(
  pausedSimulation.LastPublishedSnapshot is null,
  "A paused simulation tick published a gameplay snapshot.");
Console.WriteLine("PASS: paused tick ends before gameplay phases");

SimulationCommandQueue commandQueue = new();
commandQueue.Enqueue(new MechanismActivationCommand(
  Sequence: 100,
  MechanismId: 20,
  Kind: MechanismActivationKind.Activate,
  SourceId: 4));
commandQueue.Enqueue(new MechanismActivationCommand(
  Sequence: 100,
  MechanismId: 10,
  Kind: MechanismActivationKind.Activate,
  SourceId: 9));
commandQueue.Enqueue(new MechanismActivationCommand(
  Sequence: 100,
  MechanismId: 10,
  Kind: MechanismActivationKind.Activate,
  SourceId: 3));

IReadOnlyList<MechanismActivationCommand> ordered = commandQueue.MechanismActivationCommands;
Assert(
  ordered[0].MechanismId == 10 && ordered[0].SourceId == 3 &&
  ordered[1].MechanismId == 10 && ordered[1].SourceId == 9 &&
  ordered[2].MechanismId == 20 && ordered[2].SourceId == 4,
  "Equal-sequence mechanism commands did not use the documented deterministic key.");
Console.WriteLine("PASS: equal-sequence mechanism command order is deterministic");

SimulationCommandQueue reversedCommandQueue = new();
reversedCommandQueue.Enqueue(new MechanismActivationCommand(
  Sequence: 100,
  MechanismId: 10,
  Kind: MechanismActivationKind.Activate,
  SourceId: 3));
reversedCommandQueue.Enqueue(new MechanismActivationCommand(
  Sequence: 100,
  MechanismId: 20,
  Kind: MechanismActivationKind.Activate,
  SourceId: 4));
reversedCommandQueue.Enqueue(new MechanismActivationCommand(
  Sequence: 100,
  MechanismId: 10,
  Kind: MechanismActivationKind.Activate,
  SourceId: 9));
string orderedCommandHash = HashMechanismCommands(ordered);
string reversedCommandHash = HashMechanismCommands(reversedCommandQueue.MechanismActivationCommands);
Assert(
  StringComparer.Ordinal.Equals(orderedCommandHash, reversedCommandHash),
  "Same-tick competing mechanism commands produced different ordered hashes.");
Console.WriteLine("PASS: same-tick competing mechanism commands have an ordered replay hash");

using DomeSimulation firstSimulation = new(new WorldGrid(400, 300));
using DomeSimulation secondSimulation = new(new WorldGrid(400, 300));
PlayerHandle firstPlayerOne = firstSimulation.CreatePlayer(new SimulationVector(20.0f, 20.0f));
PlayerHandle firstPlayerTwo = firstSimulation.CreatePlayer(new SimulationVector(40.0f, 20.0f));
PlayerHandle secondPlayerOne = secondSimulation.CreatePlayer(new SimulationVector(20.0f, 20.0f));
PlayerHandle secondPlayerTwo = secondSimulation.CreatePlayer(new SimulationVector(40.0f, 20.0f));

PlayerInput firstInput = new(firstPlayerOne, false, true, false, false);
PlayerInput secondInput = new(firstPlayerTwo, true, false, false, false);
firstSimulation.Tick(new SimulationInputBatch(firstInput, secondInput));
secondSimulation.Tick(new SimulationInputBatch(
  new PlayerInput(secondPlayerTwo, true, false, false, false),
  new PlayerInput(secondPlayerOne, false, true, false, false)));

SimulationSnapshot firstSnapshot = firstSimulation.LastPublishedSnapshot
  ?? throw new InvalidOperationException("The first deterministic simulation did not publish.");
SimulationSnapshot secondSnapshot = secondSimulation.LastPublishedSnapshot
  ?? throw new InvalidOperationException("The second deterministic simulation did not publish.");
AssertSequence(
  firstSimulation.LastTickPhases,
  secondSimulation.LastTickPhases,
  "Equivalent simulations produced different phase traces.");
Assert(
  firstSnapshot.FindPlayer(firstPlayerOne) == secondSnapshot.FindPlayer(secondPlayerOne) &&
  firstSnapshot.FindPlayer(firstPlayerTwo) == secondSnapshot.FindPlayer(secondPlayerTwo) &&
  firstSnapshot.Tick == secondSnapshot.Tick,
  "Reversing independent input collection changed the authoritative snapshot.");
string firstSnapshotHash = HashSnapshot(firstSnapshot);
string secondSnapshotHash = HashSnapshot(secondSnapshot);
Assert(
  StringComparer.Ordinal.Equals(firstSnapshotHash, secondSnapshotHash),
  "Reversing same-tick input collection changed the ordered snapshot hash.");
Console.WriteLine("PASS: reversed independent inputs keep phase trace and snapshot deterministic");
Console.WriteLine("PASS: same-tick competing input replay has an ordered snapshot hash");

using DomeSimulation invasionSimulation = new(new WorldGrid(400, 300));
Assert(
  invasionSimulation.TryQueueWorldInvasion(new WorldInvasionStartCommand(2, 1, 1)),
  "The tick-order invasion fixture could not queue its start.");
invasionSimulation.Tick(new SimulationInputBatch());
Assert(
  invasionSimulation.TryQueueWorldInvasionProgress(new WorldInvasionProgressCommand(1, 2)),
  "The tick-order invasion fixture could not queue completion.");
invasionSimulation.Tick(new SimulationInputBatch());
Assert(
  invasionSimulation.CreateWorldInvasionCompletedEvents().Count == 1,
  "World invasion completion was not visible after the authoritative world phase.");
int worldPhaseIndex = IndexOf(invasionSimulation.LastTickPhases, SimulationTickPhase.ApplyWorldClock);
int projectilePhaseIndex = IndexOf(
  invasionSimulation.LastTickPhases,
  SimulationTickPhase.AdvanceProjectiles);
Assert(
  worldPhaseIndex >= 0 && projectilePhaseIndex > worldPhaseIndex,
  "World invasion completion did not precede projectile advancement.");
Console.WriteLine("PASS: world invasion completion precedes projectile advancement");

WorldTimeRatePolicy timeRatePolicy = new();
WorldTimeRateSystem timeRateSystem = new();
Assert(
  timeRatePolicy.Resolve(new WorldTimeRateInput(targetRate: 3)).Rate == 3,
  "The configured time rate was not retained.");
Assert(
  timeRatePolicy.Resolve(new WorldTimeRateInput(isFastForwarding: true, targetRate: 3)).Rate == 60,
  "Fast-forward did not use the source-backed rate.");
Assert(
  timeRatePolicy.Resolve(new WorldTimeRateInput(isTimeFrozen: true, targetRate: 3)).Rate == 0,
  "Freeze-time did not stop the rate.");
Assert(
  timeRatePolicy.Resolve(
    new WorldTimeRateInput(targetRate: 3, activePlayerCount: 2, sleepingPlayerCount: 2)).Rate == 15,
  "All sleeping active players did not accelerate time.");
Assert(
  timeRatePolicy.Resolve(
    new WorldTimeRateInput(targetRate: 3, activePlayerCount: 2, sleepingPlayerCount: 1)).Rate == 3,
  "A partial sleeping set accelerated time.");
try
{
  _ = timeRatePolicy.Resolve(new WorldTimeRateInput(
    targetRate: int.MaxValue,
    activePlayerCount: 1,
    sleepingPlayerCount: 1));
  throw new InvalidOperationException(
    "Sleeping time-rate policy accepted a target that overflows its Int32 rate.");
}
catch (ArgumentOutOfRangeException)
{
}
Assert(
  timeRatePolicy.Resolve(new WorldTimeRateInput(isGameMenu: true, targetRate: 3)).Rate == 1,
  "Game-menu fallback did not override the rate.");
Assert(
  timeRatePolicy.Resolve(
    new WorldTimeRateInput(
      isFastForwarding: true,
      isTimeFrozen: true,
      isGameMenu: true)).Rate == 60,
  "Fast-forward did not retain its source early-return precedence.");
Assert(
  timeRatePolicy.Resolve(
    new WorldTimeRateInput(isTimeFrozen: true, isGameMenu: true, targetRate: 3)).Rate == 1,
  "Game-menu fallback did not override the frozen normal branch.");
Console.WriteLine("PASS: world time-rate policy preserves source precedence");

using DomeSimulation clockRateSimulation = new(new WorldGrid(400, 300));
clockRateSimulation.ConfigureWorldTimeRate(new WorldTimeRateInput(targetRate: 3));
clockRateSimulation.Tick(new SimulationInputBatch());
Assert(
  clockRateSimulation.TickNumber == 3 && clockRateSimulation.TimeOfDay == 3,
  "The resolved world time rate was not applied before clock advancement.");
Assert(
  timeRateSystem.Resolve(new WorldTimeRateInput(targetRate: 3)).Rate == 3,
  "The production world time-rate system did not expose the validated policy result.");
WorldMetadata clockRateMetadata = new(
  "Time-rate persistence",
  new WorldSeed(21),
  400,
  300);
DomeSimulationSnapshot persistedRate = clockRateSimulation.CreatePersistenceSnapshot(
  clockRateMetadata);
Assert(
  persistedRate.WorldTimeRate.IsAvailable && persistedRate.WorldTimeRate.Rate == 3,
  "The resolved world time rate was not present in the persistence snapshot.");
using DomeSimulation restoredRateSimulation = new(persistedRate);
restoredRateSimulation.Tick(new SimulationInputBatch());
Assert(
  restoredRateSimulation.TickNumber == 6 && restoredRateSimulation.TimeOfDay == 6 &&
  restoredRateSimulation.CreateWorldTimeRateSnapshot().Rate == 3,
  "The persisted time rate did not drive deterministic restart continuation.");
Console.WriteLine("PASS: resolved world time rate advances the authoritative clock");

WorldMetadata travelMetadata = new(
  "Time-rate travel",
  new WorldSeed(9),
  400,
  300,
  spawnX: 200,
  spawnY: 75);
DomeSimulationSnapshot travelSnapshot = new(
  new WorldGrid(400, 300).CreateSnapshot(travelMetadata),
  [],
  [],
  0,
  progression: new WorldProgressionState(invasionType: 1, invasionSize: 5, invasionX: 202));
using DomeSimulation travelSimulation = new(travelSnapshot);
travelSimulation.ConfigureWorldTimeRate(new WorldTimeRateInput(isTimeFrozen: true));
travelSimulation.Tick(new SimulationInputBatch());
Assert(
  travelSimulation.CreateWorldProgressionSnapshot().InvasionX == 201,
  "Invasion travel did not consume the resolved rate in the world-clock phase.");
Assert(
  travelSimulation.CreateWorldTimeRateSnapshot().Rate == 0,
  "Invasion travel ran before the frozen time rate was resolved.");
Assert(
  travelSimulation.TickNumber == 0 && travelSimulation.TimeOfDay == 0,
  "Frozen time advanced the authoritative world clock.");
Console.WriteLine("PASS: invasion travel consumes resolved time rate before player phases");

using DomeSimulation deathOrderSimulation = new(new WorldGrid(400, 300));
NpcHandle deathOrderNpc = deathOrderSimulation.CreateNpc(new SimulationVector(30.0f, 0.0f));
deathOrderSimulation.Tick(new SimulationInputBatch());
deathOrderSimulation.QueueNpcDamage(deathOrderNpc, 100);
deathOrderSimulation.Tick(new SimulationInputBatch());
IReadOnlyList<SimulationTickPhase> deathPhases = deathOrderSimulation.LastTickPhases;
int combatPhaseIndex = IndexOf(deathPhases, SimulationTickPhase.ResolveCombat);
int commitPhaseIndex = IndexOf(deathPhases, SimulationTickPhase.CommitDomainCommands);
int publishPhaseIndex = IndexOf(deathPhases, SimulationTickPhase.PublishSnapshot);
NpcReplicationSnapshot deathOrderReplication = deathOrderSimulation
  .CreateNpcReplicationSnapshots()
  .Single(snapshot => snapshot.ReplicationId == deathOrderNpc.Value);
Assert(
  combatPhaseIndex < commitPhaseIndex && commitPhaseIndex < publishPhaseIndex,
  "NPC death commit did not precede authoritative snapshot publication.");
Assert(
  !deathOrderReplication.IsActive && deathOrderSimulation.CreateWorldItemSnapshots().Count == 1,
  "NPC death loot was not visible with the inactive replication in the same completed tick.");
Console.WriteLine("PASS: NPC death commit and loot precede authoritative snapshot publication");

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

static string HashMechanismCommands(IReadOnlyList<MechanismActivationCommand> commands)
{
  StringBuilder canonical = new();
  for (int index = 0; index < commands.Count; index++)
  {
    MechanismActivationCommand command = commands[index];
    canonical.Append(command.Sequence).Append(':')
      .Append(command.MechanismId).Append(':')
      .Append((int)command.Kind).Append(':')
      .Append(command.SourceId).Append(';');
  }

  return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical.ToString())));
}

static string HashSnapshot(SimulationSnapshot snapshot)
{
  StringBuilder canonical = new();
  canonical.Append("tick=").Append(snapshot.Tick).Append('|');
  foreach (PlayerSnapshot player in snapshot.Players.OrderBy(value => value.Player.Value))
  {
    canonical.Append("p=").Append(player.Player.Value).Append(':')
      .Append(player.Position.X).Append(',').Append(player.Position.Y).Append(':')
      .Append(player.Velocity.X).Append(',').Append(player.Velocity.Y).Append(':')
      .Append(player.Facing).Append(':').Append(player.Health).Append(':')
      .Append(player.IsActive).Append(':').Append(player.RespawnTicks).Append(';');
  }

  foreach (NpcSnapshot npc in snapshot.Npcs.OrderBy(value => value.Npc.Value))
  {
    canonical.Append("n=").Append(npc.Npc.Value).Append(':')
      .Append(npc.Position.X).Append(',').Append(npc.Position.Y).Append(':')
      .Append(npc.Health).Append(':').Append(npc.HasTarget).Append(';');
  }

  for (int index = 0; index < snapshot.Projectiles.Count; index++)
  {
    ProjectileSnapshot projectile = snapshot.Projectiles[index];
    canonical.Append("x=").Append(index).Append(':')
      .Append(projectile.Position.X).Append(',').Append(projectile.Position.Y).Append(':')
      .Append(projectile.RemainingLifetime).Append(';');
  }

  return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical.ToString())));
}

static void AssertSequence(
  IReadOnlyList<SimulationTickPhase> actual,
  IReadOnlyList<SimulationTickPhase> expected,
  string message)
{
  if (actual.Count != expected.Count)
  {
    throw new InvalidOperationException(message);
  }

  for (int index = 0; index < expected.Count; index++)
  {
    if (actual[index] != expected[index])
    {
      throw new InvalidOperationException(message);
    }
  }
}

static int IndexOf(IReadOnlyList<SimulationTickPhase> phases, SimulationTickPhase phase)
{
  for (int index = 0; index < phases.Count; index++)
  {
    if (phases[index] == phase)
    {
      return index;
    }
  }

  return -1;
}
