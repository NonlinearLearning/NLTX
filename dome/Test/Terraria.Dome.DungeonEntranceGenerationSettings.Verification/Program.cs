using System;
using Terraria.Dome.Simulation.WorldGeneration;

DungeonStyleDefinition definition = new(1, null, 2, 3, 4, 5, 6);
DungeonStyleLookupEntry style = new(definition);
DungeonEntranceGenerationSettings settings = new(
  DungeonEntranceType.Tower,
  randomSeed: 11,
  style,
  precalculateEntrancePosition: true);

if (settings.EntranceType != DungeonEntranceType.Tower ||
    settings.RandomSeed != 11 ||
    settings.Style != style ||
    !settings.PrecalculateEntrancePosition)
{
  throw new InvalidOperationException("Dungeon entrance generation settings diverged.");
}

AssertThrows<ArgumentOutOfRangeException>(() =>
  new DungeonEntranceGenerationSettings((DungeonEntranceType)99, 0, style));
AssertThrows<ArgumentNullException>(() =>
  new DungeonEntranceGenerationSettings(DungeonEntranceType.Legacy, 0, null!));

Console.WriteLine("PASS: Dungeon entrance generation settings contract");

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
