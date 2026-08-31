using System;
using Terraria.Dome.Simulation.WorldGeneration;

DungeonStyleDefinition style = new(
  brickTileType: 10,
  brickGrassTileType: 11,
  brickCrackedTileType: 12,
  brickWallType: 20,
  windowGlassWallType: 21,
  windowClosedGlassWallType: 22,
  windowEdgeWallType: 23);

if (!style.TileIsInStyle(10) ||
    !style.TileIsInStyle(11) ||
    !style.TileIsInStyle(12) ||
    style.TileIsInStyle(12, includeCracked: false) ||
    style.TileIsInStyle(13) ||
    !style.WallIsInStyle(20) ||
    !style.WallIsInStyle(21, includeWindows: true) ||
    !style.WallIsInStyle(22, includeWindows: true) ||
    !style.WallIsInStyle(23, includeWindows: true) ||
    style.WallIsInStyle(21) ||
    style.WallIsInStyle(24, includeWindows: true))
{
  throw new InvalidOperationException("Dungeon style tile/wall predicates diverged.");
}

Console.WriteLine("PASS: Dungeon style definition contract");
