using System;
using Terraria.Dome.Simulation.WorldGeneration;

if (!DungeonGenerationStyleSelection.IsValidStyleId(DungeonGenerationStyleId.Dungeon) ||
    !DungeonGenerationStyleSelection.IsValidStyleId(DungeonGenerationStyleId.Crystal) ||
    DungeonGenerationStyleSelection.IsValidStyleId(-1) ||
    DungeonGenerationStyleSelection.IsValidStyleId(DungeonGenerationStyleId.Count))
{
  throw new InvalidOperationException("Dungeon style ID range diverged.");
}

DungeonGenerationStyleSelection.ValidateStyleId(DungeonGenerationStyleId.Snow);
AssertThrows<ArgumentOutOfRangeException>(() => DungeonGenerationStyleSelection.ValidateStyleId(-1));
AssertThrows<ArgumentOutOfRangeException>(() => DungeonGenerationStyleSelection.ValidateStyleId(
  DungeonGenerationStyleId.Count));

Console.WriteLine("PASS: Dungeon generation style selection contract");

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
