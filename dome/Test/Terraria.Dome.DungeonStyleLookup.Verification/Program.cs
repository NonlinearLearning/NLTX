using System;
using Terraria.Dome.Simulation.WorldGeneration;

DungeonStyleDefinition primary = new(10, null, 11, 20, 21, 22, 23);
DungeonStyleDefinition nested = new(30, null, 31, 40, 41, 42, 43);
DungeonStyleDefinition later = new(10, null, 12, 50, 51, 52, 53);
DungeonStyleDefinition[] nestedStyles = { nested };
DungeonStyleLookupEntry[] styles =
{
  new DungeonStyleLookupEntry(primary, nestedStyles),
  new DungeonStyleLookupEntry(later)
};
nestedStyles[0] = later;

if (!DungeonStyleLookupQuery.TryFindByTile(styles, 30, out DungeonStyleDefinition nestedTile) ||
    nestedTile.BrickTileType != 30 ||
    !DungeonStyleLookupQuery.TryFindByWall(styles, 40, out DungeonStyleDefinition nestedWall) ||
    nestedWall.BrickWallType != 40 ||
    !DungeonStyleLookupQuery.TryFindByTile(styles, 10, out DungeonStyleDefinition firstTile) ||
    firstTile.BrickTileType != 10 ||
    DungeonStyleLookupQuery.TryFindByTile(styles, 99, out _) ||
    DungeonStyleLookupQuery.TryFindByWall(styles, 99, out _))
{
  throw new InvalidOperationException("Dungeon style lookup contract diverged.");
}

Console.WriteLine("PASS: Dungeon style lookup contract");
