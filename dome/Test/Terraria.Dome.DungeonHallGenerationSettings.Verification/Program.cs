using System;
using Terraria.Dome.Simulation.WorldGeneration;

DungeonHallGenerationSettings settings = new(
  DungeonHallType.Regular,
  randomSeed: 19,
  overridePaintTile: 2,
  overridePaintWall: 3,
  crackedBrickChance: 0.25,
  placeOverProtectedBricks: true,
  zigzagChance: 0.75,
  forceStyleForDoorsAndPlatforms: true,
  carveOnly: true,
  overrideInnerBoundsSize: 4,
  overrideOuterBoundsSize: 8);

if (settings.HallType != DungeonHallType.Regular ||
    settings.RandomSeed != 19 ||
    settings.OverridePaintTile != 2 ||
    settings.OverridePaintWall != 3 ||
    settings.CrackedBrickChance != 0.25 ||
    !settings.PlaceOverProtectedBricks ||
    settings.ZigzagChance != 0.75 ||
    !settings.ForceStyleForDoorsAndPlatforms ||
    !settings.CarveOnly ||
    settings.OverrideInnerBoundsSize != 4 ||
    settings.OverrideOuterBoundsSize != 8)
{
  throw new InvalidOperationException("Dungeon hall generation settings diverged.");
}

AssertThrows<ArgumentOutOfRangeException>(() =>
  new DungeonHallGenerationSettings(DungeonHallType.Regular, 0, crackedBrickChance: 1.1));
AssertThrows<ArgumentOutOfRangeException>(() =>
  new DungeonHallGenerationSettings(DungeonHallType.Regular, 0, zigzagChance: double.NaN));
AssertThrows<ArgumentOutOfRangeException>(() =>
  new DungeonHallGenerationSettings((DungeonHallType)99, 0));

Console.WriteLine("PASS: Dungeon hall generation settings contract");

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
