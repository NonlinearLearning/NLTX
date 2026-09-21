using System;
using Terraria.Dome.Simulation.WorldGeneration;

DungeonStyleDefinition definition = new(1, null, 2, 3, 4, 5, 6);
DungeonEntranceGenerationSettings entrance = new(
  DungeonEntranceType.Legacy,
  3,
  new DungeonStyleLookupEntry(definition));
DungeonPreGenerationEntranceSettings settings = new(entrance, 1, 2, 3, true);
if (settings.EntranceSettings != entrance ||
    settings.BuriedEntranceYOffset != 1 ||
    settings.BuriedEntranceSandDugoutYOffset != 2 ||
    settings.RoughHeight != 3 ||
    !settings.BuryEntrance)
{
  throw new InvalidOperationException("Pre-generation entrance settings diverged.");
}

AssertThrows<ArgumentOutOfRangeException>(() => new DungeonPreGenerationEntranceSettings(entrance, -1, 0, 0, false));
AssertThrows<ArgumentOutOfRangeException>(() => new DungeonPreGenerationEntranceSettings(entrance, 0, -1, 0, false));
AssertThrows<ArgumentOutOfRangeException>(() => new DungeonPreGenerationEntranceSettings(entrance, 0, 0, -1, false));

Console.WriteLine("PASS: Pre-generation entrance settings contract");

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
