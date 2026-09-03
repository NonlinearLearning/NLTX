using System;
using Terraria.Dome.Simulation.WorldGeneration;

if ((int)DungeonWindowType.RegularWindows != 0 ||
    (int)DungeonWindowType.SkeletronMosaic != 1 ||
    (int)DungeonWindowType.MoonLordMosaic != 2 ||
    (int)DungeonType.Default != 0 ||
    (int)DungeonType.DualDungeon != 1)
{
  throw new InvalidOperationException("Dungeon type enum values diverged.");
}

Console.WriteLine("PASS: Dungeon type enum contracts");
