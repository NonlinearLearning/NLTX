using System;
using Terraria.Dome.Simulation.WorldGeneration;

DungeonRoomCandidate candidate = new("candidate", 10, 10, 3);
DungeonRoomCandidate excluded = new("excluded", 13, 14, 2);
if (!DungeonRoomSearchQuery.CanChoose(
      candidate,
      excluded,
      new DungeonRoomSearchSettingsSnapshot(
        progressionStage: 3,
        progressionStageCheck: DungeonProgressionStageCheck.Equals,
        maximumDistance: 10,
        fluff: 2)) ||
    DungeonRoomSearchQuery.CanChoose(
      candidate,
      excluded,
      new DungeonRoomSearchSettingsSnapshot(
        progressionStage: 2,
        progressionStageCheck: DungeonProgressionStageCheck.Equals,
        maximumDistance: null)) ||
    DungeonRoomSearchQuery.CanChoose(
      new DungeonRoomCandidate("same", 0, 0, 3),
      new DungeonRoomCandidate("same", 10, 10, 3),
      new DungeonRoomSearchSettingsSnapshot(null, DungeonProgressionStageCheck.Equals, null)))
{
  throw new InvalidOperationException("Room search predicate diverged.");
}

if (!DungeonRoomSearchQuery.CanChoose(
      candidate,
      excluded,
      new DungeonRoomSearchSettingsSnapshot(
        progressionStage: 4,
        progressionStageCheck: DungeonProgressionStageCheck.LessThanOrEqualTo,
        maximumDistance: null)) ||
    !DungeonRoomSearchQuery.CanChoose(
      candidate,
      excluded,
      new DungeonRoomSearchSettingsSnapshot(
        progressionStage: 2,
        progressionStageCheck: DungeonProgressionStageCheck.GreaterThanOrEqualTo,
        maximumDistance: null)))
{
  throw new InvalidOperationException("Progression stage comparisons diverged.");
}

AssertThrows<ArgumentOutOfRangeException>(() =>
  new DungeonRoomSearchSettingsSnapshot(-1, DungeonProgressionStageCheck.Equals, null));
AssertThrows<ArgumentOutOfRangeException>(() =>
  new DungeonRoomSearchSettingsSnapshot(null, DungeonProgressionStageCheck.Equals, -1));
AssertThrows<ArgumentOutOfRangeException>(() =>
  new DungeonRoomSearchSettingsSnapshot(null, DungeonProgressionStageCheck.Equals, null, -1));

if (new DungeonRoomSearchSettingsSnapshot(null, DungeonProgressionStageCheck.Equals, null, 3).Fluff != 3)
{
  throw new InvalidOperationException("Room search fluff did not round-trip.");
}

Console.WriteLine("PASS: Dungeon room search predicate contract");

static void AssertThrows<TException>(Action action)
  where TException : Exception
{
  try
  {
    action();
  }
  catch (TException)
  {
    return;
  }

  throw new InvalidOperationException($"Expected {typeof(TException).Name}.");
}
