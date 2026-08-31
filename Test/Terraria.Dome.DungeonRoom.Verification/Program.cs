using System;
using Terraria.Dome.Simulation.WorldGeneration;

if (DungeonRoomBoundingRadiusQuery.Regular(100, 50) != 213 ||
    DungeonRoomBoundingRadiusQuery.LivingTree(27) != 27 ||
    DungeonRoomBoundingRadiusQuery.Wormlike(10, 20) != 18 ||
    DungeonRoomBoundingRadiusQuery.StepBased(10, 20) != 27)
{
  throw new InvalidOperationException("Dungeon room radius formulas diverged.");
}

AssertThrows<ArgumentOutOfRangeException>(() => DungeonRoomBoundingRadiusQuery.Regular(-1, 1));
AssertThrows<ArgumentOutOfRangeException>(() => DungeonRoomBoundingRadiusQuery.Wormlike(1, -1));
AssertThrows<ArgumentOutOfRangeException>(() => DungeonRoomBoundingRadiusQuery.StepBased(1, -1));
AssertThrows<OverflowException>(() =>
  DungeonRoomBoundingRadiusQuery.Regular(int.MaxValue, int.MaxValue));

Console.WriteLine("PASS: Dungeon room bounding-radius contracts");

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
