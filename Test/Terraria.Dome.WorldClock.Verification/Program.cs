using System;
using System.IO;
using Terraria.Dome.Server.Persistence;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.WorldModel;

Test("fixed tick rate", () =>
{
  WorldClock clock = new(ticksPerUpdate: 2);
  clock.Advance();
  Assert(clock.TickNumber == 2 && clock.TimeOfDay == 2, "configured rate was not applied");
});

Test("global time wraps on the authoritative hourly boundary", () =>
{
  WorldClock clock = new(tickNumber: WorldClock.TicksPerSecond * 3599 + 30);
  Assert(clock.GlobalTimeWrappedHourly == 3599.5d,
    "global hourly time did not derive from the monotonic tick clock");

  WorldClock wrapped = new(tickNumber: WorldClock.TicksPerSecond * 3600);
  Assert(wrapped.GlobalTimeWrappedHourly == 0.0d,
    "global hourly time did not wrap after one hour");
});

Test("day and night boundaries", () =>
{
  WorldClock dayClock = new(timeOfDay: WorldClock.DefaultDayLengthTicks - 1);
  dayClock.Advance();
  Assert(!dayClock.IsDayTime && dayClock.TimeOfDay == 0, "day boundary was not crossed");

  WorldClock nightClock = new(
    timeOfDay: WorldClock.DefaultNightLengthTicks - 1,
    isDayTime: false);
  nightClock.Advance();
  Assert(nightClock.IsDayTime && nightClock.TimeOfDay == 0, "night boundary was not crossed");
});

Test("full cycle", () =>
{
  WorldClock clock = new();
  for (int index = 0; index < WorldClock.DefaultDayLengthTicks +
    WorldClock.DefaultNightLengthTicks; index++)
  {
    clock.Advance();
  }

  Assert(clock.TickNumber == 86400 && clock.IsDayTime && clock.TimeOfDay == 0,
    "a full day/night cycle did not return to the day boundary");
});

Test("paused time and continuation", () =>
{
  WorldClock clock = new();
  clock.Advance();
  WorldClockSnapshot beforePause = clock.CreateSnapshot();
  clock.SetPaused(true);
  clock.Advance();
  Assert(clock.CreateSnapshot() == beforePause with { IsPaused = true },
    "paused clock advanced");

  clock.SetPaused(false);
  clock.Advance();
  WorldClock resumed = new();
  resumed.Restore(beforePause);
  resumed.Advance();
  Assert(clock.CreateSnapshot() == resumed.CreateSnapshot(),
    "clock did not continue deterministically from an equal snapshot");
});

Test("simulation tick phase", () =>
{
  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  simulation.Tick(new SimulationInputBatch());
  Assert(simulation.TickNumber == 1 && simulation.TimeOfDay == 1,
    "DomeSimulation did not run the clock phase");
  simulation.SetWorldTimePaused(true);
  simulation.Tick(new SimulationInputBatch());
  Assert(simulation.TickNumber == 1 && simulation.TimeOfDay == 1,
    "paused simulation advanced the clock");
});

Test("game update count starts at zero and advances once per active tick", () =>
{
  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  Assert(simulation.GameUpdateCount == 0u, "game update count did not start at zero");

  simulation.Tick(new SimulationInputBatch());
  Assert(simulation.GameUpdateCount == 1u,
    "game update count did not advance once for an active tick");
});

Test("paused simulation does not advance game update count", () =>
{
  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  simulation.Tick(new SimulationInputBatch());
  simulation.SetWorldTimePaused(true);
  simulation.Tick(new SimulationInputBatch());
  Assert(simulation.GameUpdateCount == 1u,
    "paused simulation advanced the game update count");

  simulation.SetWorldTimePaused(false);
  simulation.Tick(new SimulationInputBatch());
  Assert(simulation.GameUpdateCount == 2u,
    "game update count did not resume after unpausing");
});

Test("game update count is independent from world clock rate", () =>
{
  using DomeSimulation frozenClock = new(new WorldGrid(400, 300));
  frozenClock.ConfigureWorldTimeRate(new WorldTimeRateInput(isTimeFrozen: true));
  frozenClock.Tick(new SimulationInputBatch());
  Assert(frozenClock.GameUpdateCount == 1u && frozenClock.TickNumber == 0,
    "rate zero did not advance the active game update count exactly once");

  using DomeSimulation acceleratedClock = new(new WorldGrid(400, 300));
  acceleratedClock.ConfigureWorldTimeRate(new WorldTimeRateInput(targetRate: 3));
  acceleratedClock.Tick(new SimulationInputBatch());
  Assert(acceleratedClock.GameUpdateCount == 1u && acceleratedClock.TickNumber == 3,
    "rate three coupled the game update count to clock ticks");
});

Test("game update count is transient across persistence snapshots", () =>
{
  using DomeSimulation simulation = new(new WorldGrid(400, 300));
  simulation.Tick(new SimulationInputBatch());
  WorldMetadata metadata = new("game-update-count", new WorldSeed(12), 400, 300);
  DomeSimulationSnapshot snapshot = simulation.CreatePersistenceSnapshot(metadata);

  using DomeSimulation restored = new(snapshot);
  Assert(restored.GameUpdateCount == 0u,
    "persistence snapshot unexpectedly serialized the transient game update count");
  restored.Tick(new SimulationInputBatch());
  Assert(restored.GameUpdateCount == 1u,
    "restored simulation did not restart its transient game update cursor");
});

Test("game update count wraps at uint maximum", () =>
{
  WorldGameUpdateCountProjection projection =
    new(initialValue: uint.MaxValue);
  projection.Advance();
  Assert(projection.Value == 0u,
    "game update count did not use the legacy uint wraparound boundary");
});

Test("dawn and dusk transitions are deterministic", () =>
{
  WorldMetadata metadata = new("clock-transition", new WorldSeed(8), 400, 300);
  WorldGrid world = new(400, 300);
  DomeSimulationSnapshot duskSource = new(
    world.CreateSnapshot(metadata),
    [],
    [],
    0,
    worldClock: new WorldClockSnapshot(
      0,
      WorldClock.DefaultDayLengthTicks - 1,
      true,
      false,
      1));
  using DomeSimulation dusk = new(duskSource);
  dusk.Tick(new SimulationInputBatch());
  Assert(
    dusk.LastWorldClockTransition?.Kind == WorldClockTransitionKind.Dusk,
    "The day boundary did not publish a dusk transition.");

  using DomeSimulation restoredDusk = new(duskSource);
  restoredDusk.Tick(new SimulationInputBatch());
  Assert(
    restoredDusk.LastWorldClockTransition?.Kind == WorldClockTransitionKind.Dusk &&
    restoredDusk.TickNumber == 1,
    "Restoring the same boundary did not reproduce the same single transition.");

  WorldGrid dawnWorld = new(400, 300);
  DomeSimulationSnapshot dawnSource = new(
    dawnWorld.CreateSnapshot(metadata),
    [],
    [],
    0,
    worldClock: new WorldClockSnapshot(
      0,
      WorldClock.DefaultNightLengthTicks - 1,
      false,
      false,
      1));
  using DomeSimulation dawn = new(dawnSource);
  dawn.Tick(new SimulationInputBatch());
  Assert(
    dawn.LastWorldClockTransition?.Kind == WorldClockTransitionKind.Dawn,
    "The night boundary did not publish a dawn transition.");

  dawn.SetWorldTimePaused(true);
  dawn.Tick(new SimulationInputBatch());
  Assert(
    dawn.LastWorldClockTransition is null,
    "A paused tick published a clock transition.");
});

Test("snapshot continuation", () =>
{
  using DomeSimulation first = new(new WorldGrid(400, 300));
  first.Tick(new SimulationInputBatch());
  DomeSimulationSnapshot snapshot = first.CreatePersistenceSnapshot(
    new WorldMetadata("clock", new WorldSeed(1), 400, 300));
  using DomeSimulation second = new(snapshot);
  first.Tick(new SimulationInputBatch());
  second.Tick(new SimulationInputBatch());
  Assert(first.CreateWorldRuleSnapshot() == second.CreateWorldRuleSnapshot(),
    "equal snapshots did not produce equal clock state");
});

Test("fractional WLD time survives snapshot persistence", () =>
{
  const double savedWldTime = 1.5d;
  WorldClock clock = new(timeOfDay: savedWldTime);
  Assert(clock.TimeOfDay == savedWldTime,
    "The authoritative clock did not retain the fractional WLD time.");
  WorldClock restored = new();
  restored.Restore(clock.CreateSnapshot());
  Assert(restored.TimeOfDay == savedWldTime,
    "The clock snapshot did not retain the fractional WLD time.");

  WorldGrid world = new(400, 300);
  WorldMetadata metadata = new("fractional", new WorldSeed(45), 400, 300);
  DomeSimulationSnapshot source = new(
    world.CreateSnapshot(metadata),
    [],
    [],
    0,
    worldClock: clock.CreateSnapshot());
  using MemoryStream storage = new();
  DomeStatePersistenceFormat.Write(storage, source);
  storage.Position = 0;
  DomeSimulationSnapshot persisted = DomeStatePersistenceFormat.Read(storage);
  Assert(persisted.Clock.TimeOfDay == savedWldTime,
    "The persisted clock truncated fractional WLD time.");
});

Test("restore rejects an incompatible update rate before mutation", () =>
{
  WorldClock clock = new(ticksPerUpdate: 2);
  WorldClockSnapshot before = clock.CreateSnapshot();
  WorldClockSnapshot incompatible = before with { TicksPerUpdate = 1, TickNumber = 4 };

  bool rejected = false;
  try
  {
    clock.Restore(incompatible);
  }
  catch (ArgumentException)
  {
    rejected = true;
  }

  Assert(rejected, "an incompatible update rate was accepted");
  Assert(clock.CreateSnapshot() == before,
    "incompatible restore mutated the clock before rejection");
});

Console.WriteLine("PASS: world clock verification scenarios");

static void Test(string name, Action action)
{
  action();
  Console.WriteLine($"PASS: {name}");
}

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}
