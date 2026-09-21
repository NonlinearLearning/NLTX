using System;
using Terraria.Dome.Simulation.WorldGeneration;

DungeonSpawnPoint unset = DungeonSpawnPoint.Unset;
DungeonSpawnPoint point = new(120, 340);
if (unset.IsSet || unset.X != -1 || unset.Y != -1 || !point.IsSet ||
    point.X != 120 || point.Y != 340)
{
  throw new InvalidOperationException("Dungeon spawn-point state diverged.");
}

if (DungeonSpawnPoint.FromLegacy(-1, 340).IsSet ||
    DungeonSpawnPoint.FromLegacy(120, -1).IsSet ||
    DungeonSpawnPoint.FromLegacy(120, 340) != point)
{
  throw new InvalidOperationException("Legacy dungeon spawn-point projection diverged.");
}

AssertThrows(() => new DungeonSpawnPoint(-1, 10));
AssertThrows(() => new DungeonSpawnPoint(10, -1));
Console.WriteLine("PASS: dungeon spawn-point unset=-1/-1 and non-negative coordinates");
Console.WriteLine("DEFERRED: dungeon generation, persistence, and Demolitionist spawn integration remain outside this slice");

static void AssertThrows(Action action)
{
  try
  {
    action();
  }
  catch (ArgumentOutOfRangeException)
  {
    return;
  }

  throw new InvalidOperationException("Expected argument validation to reject unset coordinates.");
}
