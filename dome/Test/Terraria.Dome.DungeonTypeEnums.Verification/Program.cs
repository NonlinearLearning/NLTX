using System;
using Terraria.Dome.Simulation.WorldGeneration;

DungeonHallType[] hallTypes = Enum.GetValues<DungeonHallType>();
DungeonGenShapeType shapeTypes = DungeonGenShapeType.QuadCircle;
if (hallTypes.Length != 5 ||
    (int)DungeonHallType.Legacy != 0 ||
    (int)DungeonHallType.Sine != 4 ||
    (int)shapeTypes != 4 ||
    Enum.GetValues<DungeonGenShapeType>().Length != 5)
{
  throw new InvalidOperationException("Dungeon hall or shape type contract diverged.");
}

Console.WriteLine("PASS: Dungeon hall and shape type contracts");
