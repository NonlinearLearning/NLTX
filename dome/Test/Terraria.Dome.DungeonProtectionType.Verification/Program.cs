using System;
using Terraria.Dome.Simulation.WorldGeneration;

if (DungeonProtectionTypeQuery.ProtectsTiles(DungeonProtectionType.None) ||
    DungeonProtectionTypeQuery.ProtectsWalls(DungeonProtectionType.None) ||
    !DungeonProtectionTypeQuery.ProtectsTiles(DungeonProtectionType.Tiles) ||
    DungeonProtectionTypeQuery.ProtectsWalls(DungeonProtectionType.Tiles) ||
    DungeonProtectionTypeQuery.ProtectsTiles(DungeonProtectionType.Walls) ||
    !DungeonProtectionTypeQuery.ProtectsWalls(DungeonProtectionType.Walls) ||
    !DungeonProtectionTypeQuery.ProtectsTiles(DungeonProtectionType.TilesAndWalls) ||
    !DungeonProtectionTypeQuery.ProtectsWalls(DungeonProtectionType.TilesAndWalls))
{
  throw new InvalidOperationException("Dungeon protection type mapping diverged.");
}

Console.WriteLine("PASS: Dungeon protection type contract");
