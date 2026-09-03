using System;
using Terraria.Dome.Simulation.WorldGeneration;

DungeonHourglassShape shape = new(width: 10, height: 10, percentileAddon: 0f);
if (shape.GetHalfWidth(-5) != 5 ||
    shape.GetHalfWidth(0) != 1 ||
    shape.GetHalfWidth(5) != 5 ||
    !shape.Contains(1, 0) ||
    shape.Contains(2, 0) ||
    !shape.Contains(5, 5) ||
    shape.Contains(6, 5) ||
    shape.Contains(0, 6))
{
  throw new InvalidOperationException("HourglassRoom discrete footprint diverged.");
}

DungeonHourglassShape expanded = new(width: 10, height: 10, percentileAddon: 1f);
if (expanded.GetHalfWidth(0) != 5 || expanded.GetHalfWidth(5) != 5)
{
  throw new InvalidOperationException("HourglassRoom percentile clamp diverged.");
}

AssertThrows<ArgumentOutOfRangeException>(() => new DungeonHourglassShape(0, 1, 0f));
AssertThrows<ArgumentOutOfRangeException>(() => new DungeonHourglassShape(1, 0, 0f));
AssertThrows<ArgumentOutOfRangeException>(() => new DungeonHourglassShape(1, 1, float.NaN));
AssertThrows<ArgumentOutOfRangeException>(() => shape.GetHalfWidth(6));

Console.WriteLine("PASS: Dungeon HourglassRoom shape contract");

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
