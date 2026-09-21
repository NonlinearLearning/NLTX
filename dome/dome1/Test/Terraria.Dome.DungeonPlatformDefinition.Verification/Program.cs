using System;
using Terraria.Dome.Simulation.WorldGeneration;

if (!new DungeonPlatformDefinition(0.1, 0, 0, 0).IsAShelf ||
    !new DungeonPlatformDefinition(0, 0.1, 0, 0).IsAShelf ||
    !new DungeonPlatformDefinition(0, 0, 0.1, 0).IsAShelf ||
    !new DungeonPlatformDefinition(0, 0, 0, 0.1).IsAShelf ||
    new DungeonPlatformDefinition(0, 0, 0, 0).IsAShelf ||
    new DungeonPlatformDefinition(-0.1, 0, 0, 0).IsAShelf)
{
  throw new InvalidOperationException("Dungeon platform shelf predicate diverged.");
}

AssertThrows<ArgumentOutOfRangeException>(() => new DungeonPlatformDefinition(
  double.NaN,
  0,
  0,
  0));

Console.WriteLine("PASS: Dungeon platform definition contract");

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
