using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria.DataStructures;
using Terraria.GameContent.Generation.Dungeon.Features;
using Terraria.GameContent.Generation.Dungeon.Rooms;
using Terraria.ID;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace Terraria.GameContent.Generation.Dungeon;

public class DungeonUtils
{
	public delegate ConnectionPointQuality GetHallwayConnectionPoint(DungeonRoom room, Vector2D otherRoomPos, out Vector2D connectionPoint);

	public const int DOORSTYLE_WOODEN = 13;

	public const int DOORSTYLE_BLUEBRICK = 16;

	public const int DOORSTYLE_GREENBRICK = 17;

	public const int DOORSTYLE_PINKBRICK = 18;

	public const int POTSTYLE_NORMAL_1 = 0;

	public const int POTSTYLE_NORMAL_2 = 1;

	public const int POTSTYLE_NORMAL_3 = 2;

	public const int POTSTYLE_NORMAL_4 = 3;

	public const int POTSTYLE_SKULL_1 = 10;

	public const int POTSTYLE_SKULL_2 = 11;

	public const int POTSTYLE_SKULL_3 = 12;

	public const int CHANDELIERSTYLE_BLUEBRICK = 27;

	public const int CHANDELIERSTYLE_GREENBRICK = 28;

	public const int CHANDELIERSTYLE_PINKBRICK = 29;

	public const int PLATFORMSTYLE_BLUEBRICK = 6;

	public const int PLATFORMSTYLE_GREENBRICK = 8;

	public const int PLATFORMSTYLE_PINKBRICK = 7;

	public const int PLATFORMSTYLE_METALSHELF = 9;

	public const int PLATFORMSTYLE_BRASSSHELF = 10;

	public const int PLATFORMSTYLE_WOODSHELF = 11;

	public const int PLATFORMSTYLE_DUNGEONSHELF = 12;

	public const int BANNERSTYLE_BRICK_MARCHINGBONES = 10;

	public const int BANNERSTYLE_BRICK_NECROMANTICSIGN = 11;

	public const int BANNERSTYLE_SLAB_RUGGEDCOMPANY = 12;

	public const int BANNERSTYLE_SLAB_RAGGEDBROTHERHOOD = 13;

	public const int BANNERSTYLE_TILES_MOLTENLEGION = 14;

	public const int BANNERSTYLE_TILES_DIABOLICSIGIL = 15;

	public const int TRAPTYPE_DART = 0;

	public const double HALLWAY_DOOR_PLACEMENT_VARIANCE = 0.25;

	public const int DUNGEONHALL_DEFAULT_INNER_AREA_DEPTH = 3;

	public const int DUNGEONHALL_DEFAULT_OUTER_WALL_DEPTH = 8;

	public const int DUNGEONROOM_DEFAULT_INNER_AREA_DEPTH = 6;

	public const int DUNGEONROOM_DEFAULT_OUTER_WALL_DEPTH = 8;

	public const int MOSAIC_NONE = 0;

	public const int MOSAIC_SKELETRON = 1;

	public const int MOSAIC_MOONLORD = 2;
	public static void UpdateDungeonProgress(GenerationProgress progress, float percentile, string debugString, bool noFormatting = false)
{
	using (new global::Terraria.CallTracker("Terraria.GameContent.Generation.Dungeon.DungeonUtils.UpdateDungeonProgress"))
	{
		Main.statusText = debugString;
		if (progress != null)
		{
			if (noFormatting)
			{
				progress.MessageNoFormatting = debugString;
			}
			else
			{
				progress.Message = debugString;
			}
			progress.Set(percentile);
		}
	}
	}
	public static bool IsConsideredDungeonTile(int tileType, bool allDungeons = false)
{
	using (new global::Terraria.CallTracker("Terraria.GameContent.Generation.Dungeon.DungeonUtils.IsConsideredDungeonTile"))
	{
		if (tileType > 0 && Main.tileDungeon[tileType])
		{
			return true;
		}
		if (allDungeons)
		{
			for (int i = 0; i < GenVars.dungeonGenVars.Count; i++)
			{
				if (GenVars.dungeonGenVars[i].isDungeonTile[tileType])
				{
					return true;
				}
			}
		}
		else if (GenVars.CurrentDungeonGenVars.isDungeonTile[tileType])
		{
			return true;
		}
		return false;
	}
	}
	public static bool IsConsideredDungeonWall(int wallType, bool allDungeons = false)
{
	using (new global::Terraria.CallTracker("Terraria.GameContent.Generation.Dungeon.DungeonUtils.IsConsideredDungeonWall"))
	{
		if (wallType > 0 && Main.wallDungeon[wallType])
		{
			return true;
		}
		if (allDungeons)
		{
			for (int i = 0; i < GenVars.dungeonGenVars.Count; i++)
			{
				if (GenVars.dungeonGenVars[i].isDungeonWall[wallType])
				{
					return true;
				}
			}
		}
		else if (GenVars.CurrentDungeonGenVars.isDungeonWall[wallType])
		{
			return true;
		}
		return false;
	}
	}
	public static void CreatePotentialDungeonBounds(out DungeonBounds innerBounds, out DungeonBounds outerBounds, bool leftDungeon, double percentInMiddle = 0.02, double percentOnEdges = 0.02, double percentOnTop = -1.0, double percentOnBottom = -1.0, int innerBuffer = 10)
{
	using (new global::Terraria.CallTracker("Terraria.GameContent.Generation.Dungeon.DungeonUtils.CreatePotentialDungeonBounds"))
	{
		if (percentOnTop == -1.0)
		{
			percentOnTop = ((!SpecialSeedFeatures.DungeonEntranceIsUnderground) ? ((Main.worldSurface + 10.0) / (double)Main.maxTilesY) : ((GenVars.worldSurfaceHigh + 10.0) / (double)Main.maxTilesY));
		}
		if (percentOnBottom == -1.0)
		{
			percentOnBottom = ((double)Main.UnderworldLayer - 10.0) / (double)Main.maxTilesY;
		}
		double num = percentInMiddle / 2.0;
		_ = (double)Main.maxTilesX / 4200.0;
		int num2 = (leftDungeon ? ((int)((double)Main.maxTilesX * percentOnEdges)) : ((int)((double)Main.maxTilesX * (0.5 + num))));
		int num3 = (leftDungeon ? ((int)((double)Main.maxTilesX * (0.5 - num))) : (Main.maxTilesX - (int)((double)Main.maxTilesX * percentOnEdges)));
		int num4 = (int)((double)Main.maxTilesY * percentOnTop);
		int num5 = (int)((double)Main.maxTilesY * percentOnBottom);
		outerBounds = new DungeonBounds();
		outerBounds.SetBounds(num2, num4, num3, num5);
		innerBounds = new DungeonBounds();
		innerBounds.SetBounds(num2 + innerBuffer, num4 + innerBuffer, num3 - innerBuffer, num5 - innerBuffer);
	}
	}
	public static bool InAnyPotentialDungeonBounds(int x, int y, int fluff = 0, bool inner = false)
{
	using (new global::Terraria.CallTracker("Terraria.GameContent.Generation.Dungeon.DungeonUtils.InAnyPotentialDungeonBounds"))
	{
		int iteration;
		return InAnyPotentialDungeonBounds(out iteration, x, y, fluff, inner);
	}
	}
	public static bool InAnyPotentialDungeonBounds(out int iteration, int x, int y, int fluff = 0, bool inner = false)
{
	using (new global::Terraria.CallTracker("Terraria.GameContent.Generation.Dungeon.DungeonUtils.InAnyPotentialDungeonBounds"))
	{
		iteration = -1;
		for (int i = 0; i < GenVars.dungeonGenVars.Count; i++)
		{
			DungeonGenVars dungeonGenVars = GenVars.dungeonGenVars[i];
			if ((inner && dungeonGenVars.innerPotentialDungeonBounds.ContainsWithFluff(x, y, fluff)) || (!inner && dungeonGenVars.outerPotentialDungeonBounds.ContainsWithFluff(x, y, fluff)))
			{
				iteration = i;
				return true;
			}
		}
		return false;
	}
	}
	public static bool IntersectsAnyPotentialDungeonBounds(Rectangle rect, bool inner = false)
{
	using (new global::Terraria.CallTracker("Terraria.GameContent.Generation.Dungeon.DungeonUtils.IntersectsAnyPotentialDungeonBounds"))
	{
		int iteration;
		return IntersectsAnyPotentialDungeonBounds(out iteration, rect, inner);
	}
	}
	public static bool IntersectsAnyPotentialDungeonBounds(out int iteration, Rectangle rect, bool inner = false)
{
	using (new global::Terraria.CallTracker("Terraria.GameContent.Generation.Dungeon.DungeonUtils.IntersectsAnyPotentialDungeonBounds"))
	{
		iteration = -1;
		for (int i = 0; i < GenVars.dungeonGenVars.Count; i++)
		{
			DungeonGenVars dungeonGenVars = GenVars.dungeonGenVars[i];
			if ((inner && dungeonGenVars.innerPotentialDungeonBounds.Intersects(rect)) || (!inner && dungeonGenVars.outerPotentialDungeonBounds.Intersects(rect)))
			{
				iteration = i;
				return true;
			}
		}
		return false;
	}
	}
}
