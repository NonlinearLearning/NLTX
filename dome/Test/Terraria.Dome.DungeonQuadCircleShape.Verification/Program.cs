using System;
using Terraria.Dome.Simulation.WorldGeneration;

DungeonQuadCircleShape shape = new(radius: 3, distanceBetweenSpheres: 8);
if (!shape.Contains(0, 0) ||
    !shape.Contains(0, 5) ||
    !shape.Contains(5, 0) ||
    !shape.Contains(-5, 0) ||
    !shape.Contains(6, 0) ||
    !shape.Contains(8, 0) ||
    shape.Contains(0, 9) ||
    shape.Contains(9, 0))
{
  throw new InvalidOperationException("QuadCircleRoom union footprint diverged.");
}

DungeonQuadCircleShape coincident = new(radius: 0, distanceBetweenSpheres: 3);
if (!coincident.Contains(0, 0) || coincident.Contains(1, 0))
{
  throw new InvalidOperationException("QuadCircleRoom zero-radius footprint diverged.");
}

AssertThrows<ArgumentOutOfRangeException>(() => new DungeonQuadCircleShape(-1, 3));

Console.WriteLine("PASS: Dungeon QuadCircleRoom shape contract");

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
