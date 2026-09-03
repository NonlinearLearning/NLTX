using System;
using Terraria.Dome.Simulation.WorldGeneration;

DungeonBookshelfSize normal = DungeonBookshelfSizeQuery.GetDefault(2, 6);
DungeonBookshelfSize unrestricted = DungeonBookshelfSizeQuery.GetDefault(-4, -9);
DungeonBookshelfSize reversed = DungeonBookshelfSizeQuery.GetDefault(10, 3);

if (normal.Minimum != 2 ||
    normal.Maximum != 6 ||
    unrestricted.Minimum != -4 ||
    unrestricted.Maximum != -9 ||
    reversed.Minimum != 10 ||
    reversed.Maximum != 3)
{
  throw new InvalidOperationException("Dungeon bookshelf default bounds diverged.");
}

Console.WriteLine("PASS: Dungeon bookshelf size contract");
