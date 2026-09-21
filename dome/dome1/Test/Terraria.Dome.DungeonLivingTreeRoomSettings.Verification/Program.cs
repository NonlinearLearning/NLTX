using System;
using Terraria.Dome.Simulation.WorldGeneration;

DungeonLivingTreeRoomSettings settings = new(12, 20, 7, 30);
if (settings.InnerWidth != 12 ||
    settings.InnerHeight != 20 ||
    settings.Depth != 7 ||
    settings.BoundingRadius != 30 ||
    settings.GetBoundingRadius() != 30)
{
  throw new InvalidOperationException("Living tree dungeon room settings diverged.");
}

AssertThrows<ArgumentOutOfRangeException>(() => new DungeonLivingTreeRoomSettings(-1, 0, 0, 0));
AssertThrows<ArgumentOutOfRangeException>(() => new DungeonLivingTreeRoomSettings(0, -1, 0, 0));
AssertThrows<ArgumentOutOfRangeException>(() => new DungeonLivingTreeRoomSettings(0, 0, -1, 0));
AssertThrows<ArgumentOutOfRangeException>(() => new DungeonLivingTreeRoomSettings(0, 0, 0, -1));

Console.WriteLine("PASS: Living tree dungeon room settings contract");

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
