using System;
using Terraria.Dome.Simulation.WorldGeneration;

DungeonCircleShape circle = new(horizontalRadius: 5, verticalRadius: 3);
if (circle.GetHorizontalExtent(0) != 5 ||
    circle.GetHorizontalExtent(3) != 3 ||
    !circle.Contains(5, 0) ||
    !circle.Contains(0, -3) ||
    !circle.Contains(3, 3) ||
    circle.Contains(4, 3) ||
    circle.Contains(0, 4) ||
    circle.Contains(6, 0))
{
  throw new InvalidOperationException("CircleRoom discrete footprint diverged.");
}

AssertThrows<ArgumentOutOfRangeException>(() => new DungeonCircleShape(-1, 2));
AssertThrows<ArgumentOutOfRangeException>(() => new DungeonCircleShape(1, 0));
AssertThrows<ArgumentOutOfRangeException>(() => circle.GetHorizontalExtent(4));

Console.WriteLine("PASS: Dungeon CircleRoom shape contract");

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
