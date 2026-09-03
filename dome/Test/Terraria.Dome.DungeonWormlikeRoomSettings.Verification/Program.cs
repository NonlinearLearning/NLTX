using System;
using Terraria.Dome.Simulation.WorldGeneration;

DungeonWormlikeRoomSettings settings = new(4, 6);
if (settings.FirstSideIterations != 4 ||
    settings.SecondSideIterations != 6 ||
    settings.GetBoundingRadius() != DungeonRoomBoundingRadiusQuery.Wormlike(4, 6))
{
  throw new InvalidOperationException("Wormlike dungeon room settings diverged.");
}

AssertThrows<ArgumentOutOfRangeException>(() => new DungeonWormlikeRoomSettings(-1, 0));
AssertThrows<ArgumentOutOfRangeException>(() => new DungeonWormlikeRoomSettings(0, -1));

Console.WriteLine("PASS: Wormlike dungeon room settings contract");

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
