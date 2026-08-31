using System;
using Terraria.Dome.Simulation.WorldGeneration;

DungeonRoomType[] roomTypes = Enum.GetValues<DungeonRoomType>();
if (roomTypes.Length != 12 ||
    (int)DungeonRoomType.Legacy != 0 ||
    (int)DungeonRoomType.BiomeStructured != 6 ||
    (int)DungeonRoomType.GenShapeQuadCircle != 11)
{
  throw new InvalidOperationException("Dungeon room type contract diverged.");
}

Console.WriteLine("PASS: Dungeon room type contract");
