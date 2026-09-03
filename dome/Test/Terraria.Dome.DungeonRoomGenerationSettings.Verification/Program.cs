using System;
using Terraria.Dome.Simulation.WorldGeneration;

DungeonRoomGenerationSettings settings = new(
  DungeonRoomType.BiomeRugged,
  randomSeed: 17,
  progressionStage: 2,
  startingRoom: true,
  overridePaintTile: 3,
  overridePaintWall: 4,
  forceStyleForDoorsAndPlatforms: true,
  onCurvedLine: true,
  orientation: DungeonSnakeOrientation.Top,
  hallwayPointAdjuster: 5);

if (settings.RoomType != DungeonRoomType.BiomeRugged ||
    settings.RandomSeed != 17 ||
    settings.ProgressionStage != 2 ||
    !settings.StartingRoom ||
    settings.OverridePaintTile != 3 ||
    settings.OverridePaintWall != 4 ||
    !settings.ForceStyleForDoorsAndPlatforms ||
    !settings.OnCurvedLine ||
    settings.Orientation != DungeonSnakeOrientation.Top ||
    settings.HallwayPointAdjuster != 5)
{
  throw new InvalidOperationException("Dungeon room generation settings diverged.");
}

AssertThrows<ArgumentOutOfRangeException>(() =>
  new DungeonRoomGenerationSettings(DungeonRoomType.Regular, 0, -1, false));
AssertThrows<ArgumentOutOfRangeException>(() =>
  new DungeonRoomGenerationSettings(DungeonRoomType.Regular, 0, 0, false, -2));
AssertThrows<ArgumentOutOfRangeException>(() =>
  new DungeonRoomGenerationSettings((DungeonRoomType)99, 0, 0, false));

Console.WriteLine("PASS: Dungeon room generation settings contract");

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
