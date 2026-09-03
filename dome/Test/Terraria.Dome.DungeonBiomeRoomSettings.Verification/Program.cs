using System;
using Terraria.Dome.Simulation.WorldGeneration;

DungeonBiomeRoomSettings settings = new(4200, DungeonGenerationStyleId.Temple);
if (settings.WorldWidth != 4200 ||
    settings.StyleId != DungeonGenerationStyleId.Temple ||
    settings.GetBoundingRadius() != 58)
{
  throw new InvalidOperationException("Biome dungeon room settings diverged.");
}

AssertThrows<ArgumentOutOfRangeException>(() => new DungeonBiomeRoomSettings(0, 0));
AssertThrows<ArgumentOutOfRangeException>(() => new DungeonBiomeRoomSettings(4200, -1));

Console.WriteLine("PASS: Biome dungeon room settings contract");

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
