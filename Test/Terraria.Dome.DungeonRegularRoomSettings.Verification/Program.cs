using System;
using Terraria.Dome.Simulation.WorldGeneration;

DungeonRegularRoomSettings settings = new(10, 20);
if (settings.OverrideInnerBoundsSize != 10 ||
    settings.OverrideOuterBoundsSize != 20 ||
    settings.GetBoundingRadius() != DungeonRoomBoundingRadiusQuery.Regular(10, 20))
{
  throw new InvalidOperationException("Regular dungeon room settings diverged.");
}

AssertThrows<ArgumentOutOfRangeException>(() => new DungeonRegularRoomSettings(-1, 0));
AssertThrows<ArgumentOutOfRangeException>(() => new DungeonRegularRoomSettings(0, -1));

Console.WriteLine("PASS: Regular dungeon room settings contract");

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
