using System;
using Terraria.Dome.Simulation.WorldGeneration;

DungeonGenShapeRoomSettings settings = new(DungeonGenShapeType.Mound, 17);
if (settings.ShapeType != DungeonGenShapeType.Mound ||
    settings.BoundingRadius != 17 ||
    settings.GetBoundingRadius() != 17)
{
  throw new InvalidOperationException("Gen-shape dungeon room settings diverged.");
}

AssertThrows<ArgumentOutOfRangeException>(() =>
  new DungeonGenShapeRoomSettings((DungeonGenShapeType)99, 1));
AssertThrows<ArgumentOutOfRangeException>(() =>
  new DungeonGenShapeRoomSettings(DungeonGenShapeType.Circle, -1));

Console.WriteLine("PASS: Gen-shape dungeon room settings contract");

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
