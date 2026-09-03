using System;
using Terraria.Dome.Simulation.WorldGeneration;

if (DungeonUnbreakableWallTier.EarlyGame != 0 ||
    DungeonUnbreakableWallTier.EvilBoss != 1 ||
    DungeonUnbreakableWallTier.JungleBoss != 2 ||
    DungeonUnbreakableWallTier.Dungeon != 3 ||
    DungeonUnbreakableWallTier.Hallow != 4 ||
    DungeonUnbreakableWallTier.Temple != 5 ||
    (int)DungeonColor.Blue != 0 ||
    (int)DungeonColor.Green != 1 ||
    (int)DungeonColor.Pink != 2)
{
  throw new InvalidOperationException("Dungeon static definitions diverged.");
}

Console.WriteLine("PASS: Dungeon static definitions contract");
