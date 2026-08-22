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
