using System;
using Terraria.Dome.Simulation.WorldGeneration;

DungeonMoundShape mound = new(halfWidth: 4, height: 10);
if (mound.GetColumnHeight(0) != 10 ||
    mound.GetColumnHeight(-4) != 0 ||
    mound.GetColumnHeight(3) != 4 ||
    !mound.Contains(0, -5) ||
    !mound.Contains(0, 4) ||
    mound.Contains(0, 5) ||
    mound.Contains(4, 0) ||
    mound.Contains(5, 0))
{
  throw new InvalidOperationException("MoundRoom discrete footprint diverged.");
}

AssertThrows<ArgumentOutOfRangeException>(() => new DungeonMoundShape(0, 1));
AssertThrows<ArgumentOutOfRangeException>(() => new DungeonMoundShape(1, -1));
AssertThrows<ArgumentOutOfRangeException>(() => mound.GetColumnHeight(5));

Console.WriteLine("PASS: Dungeon MoundRoom shape contract");

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
