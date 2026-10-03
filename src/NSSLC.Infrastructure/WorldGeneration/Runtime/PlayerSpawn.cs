using NSSLC.WorldGeneration.GameContent.Generation.Dungeon;
using System;
using NSSLC.WorldGeneration.Geometry;
using NSSLC.WorldGeneration.ID;
namespace NSSLC.WorldGeneration;
public partial class Player {
	public static bool Spawn_IsAreaValidSpawn(int floorX, int floorY, bool generatingSpawn = false)
{
	
		for (int i = floorX - 1; i < floorX + 2; i++)
		{
			for (int j = floorY - 3; j < floorY; j++)
			{
				if (WorldGen.InWorld(i, j) && Main.tile[i, j] != null)
				{
					Tile tile = Main.tile[i, j];
					if (tile.nactive() && Main.tileSolid[tile.type] && !Main.tileSolidTop[tile.type])
					{
						return false;
					}
					if (tile.liquid > 0)
					{
						return false;
					}
					if (generatingSpawn && Main.dualDungeonsSeed && ((tile.active() && DungeonUtils.IsConsideredDungeonTile(tile.type, allDungeons: true)) || DungeonUtils.IsConsideredDungeonWall(tile.wall, allDungeons: true)))
					{
						return false;
					}
				}
			}
		}
		return true;
	
	}
	public static void Spawn_ForceClearArea(int floorX, int floorY)
{
	
		for (int i = floorX - 1; i < floorX + 2; i++)
		{
			for (int j = floorY - 3; j < floorY; j++)
			{
				if (WorldGen.InWorld(i, j) && Main.tile[i, j] != null)
				{
					if (Main.tile[i, j].nactive() && Main.tileSolid[Main.tile[i, j].type] && !Main.tileSolidTop[Main.tile[i, j].type])
					{
						WorldGen.KillTile(i, j);
					}
					if (Main.tile[i, j].liquid > 0)
					{
						Main.tile[i, j].lava(lava: false);
						Main.tile[i, j].liquid = 0;
						WorldGen.SquareTileFrame(i, j);
					}
				}
			}
		}
	
	}
}
