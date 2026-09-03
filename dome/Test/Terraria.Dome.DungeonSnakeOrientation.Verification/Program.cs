using System;
using Terraria.Dome.Simulation.WorldGeneration;

if ((int)DungeonSnakeOrientation.Unknown != 0 ||
    (int)DungeonSnakeOrientation.Top != 1 ||
    (int)DungeonSnakeOrientation.Center != 2 ||
    (int)DungeonSnakeOrientation.Bottom != 3)
{
  throw new InvalidOperationException("Dungeon snake orientation values diverged.");
}

Console.WriteLine("PASS: Dungeon snake orientation contract");
