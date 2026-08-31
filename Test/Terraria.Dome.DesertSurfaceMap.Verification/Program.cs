using System;
using Terraria.Dome.Simulation.WorldGeneration;

LegacyDesertSurfaceMap map = LegacyDesertSurfaceMap.FromHeights(
  new short[] { 70, 83, 90, 76 },
  x: 120,
  worldSurface: 95.0);
if (map.X != 120 || map.Width != 4 || map[120] != 70 || map[123] != 76 ||
    map.Top != 70 || map.Bottom != 85 || map.Average != 79.75)
{
  throw new InvalidOperationException("SurfaceMap statistics or absolute indexing diverged.");
}

AssertThrows<ArgumentException>(() =>
  LegacyDesertSurfaceMap.FromHeights(Array.Empty<short>(), x: 0, worldSurface: 100.0));
AssertThrows<ArgumentOutOfRangeException>(() => _ = map[124]);
AssertThrows<ArgumentOutOfRangeException>(() =>
  LegacyDesertSurfaceMap.FromHeights(new short[] { 1 }, x: 0, worldSurface: double.NaN));

Console.WriteLine("PASS: Desert SurfaceMap value contract");

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
