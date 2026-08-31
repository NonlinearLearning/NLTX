using System;
using Terraria.Dome.Simulation.WorldGeneration;

int[] ids =
[
  DungeonGenerationStyleId.Dungeon,
  DungeonGenerationStyleId.Cavern,
  DungeonGenerationStyleId.Snow,
  DungeonGenerationStyleId.Desert,
  DungeonGenerationStyleId.Corruption,
  DungeonGenerationStyleId.Crimson,
  DungeonGenerationStyleId.Hallow,
  DungeonGenerationStyleId.GlowingMushroom,
  DungeonGenerationStyleId.Jungle,
  DungeonGenerationStyleId.Beehive,
  DungeonGenerationStyleId.Temple,
  DungeonGenerationStyleId.Shimmer,
  DungeonGenerationStyleId.Spider,
  DungeonGenerationStyleId.LivingWood,
  DungeonGenerationStyleId.LivingMahogany,
  DungeonGenerationStyleId.Crystal
];

for (int i = 0; i < ids.Length; i++)
{
  if (ids[i] != i)
  {
    throw new InvalidOperationException("Dungeon generation style IDs diverged.");
  }
}

if (DungeonGenerationStyleId.Count != ids.Length)
{
  throw new InvalidOperationException("Dungeon generation style count diverged.");
}

Console.WriteLine("PASS: Dungeon generation style ID contract");
