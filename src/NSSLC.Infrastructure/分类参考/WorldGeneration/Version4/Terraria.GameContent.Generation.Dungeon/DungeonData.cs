using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria.GameContent.Generation.Dungeon.Entrances;
using Terraria.GameContent.Generation.Dungeon.Features;
using Terraria.GameContent.Generation.Dungeon.Halls;
using Terraria.GameContent.Generation.Dungeon.Rooms;
using Terraria.WorldBuilding;

namespace Terraria.GameContent.Generation.Dungeon;

public class DungeonData
{
	public DungeonType Type;

	public int Iteration;

	public DungeonEntrance dungeonEntrance;

	public List<DungeonRoom> dungeonRooms = new List<DungeonRoom>();

	public List<DungeonHall> dungeonHalls = new List<DungeonHall>();

	public List<IDungeonFeature> dungeonFeatures = new List<IDungeonFeature>();

	public List<DungeonDoorData> dungeonDoorData = new List<DungeonDoorData>();

	public List<DungeonPlatformData> dungeonPlatformData = new List<DungeonPlatformData>();

	public List<DungeonBounds> protectedDungeonBounds = new List<DungeonBounds>();

	public bool makeNextPitTrapFlooded;

	public bool useSkewedDungeonEntranceHalls;

	public bool createdDungeonEntranceOnSurface;

	public double dungeonEntranceStrengthX;

	public double dungeonEntranceStrengthY;

	public double dungeonEntranceStrengthX2;

	public double dungeonEntranceStrengthY2;

	public Vector2D lastDungeonHall = Vector2D.Zero;

	public DungeonBounds dungeonBounds = new DungeonBounds();

	public DungeonBounds[] outerProgressionBounds = new DungeonBounds[0];

	public int[] wallVariants = new int[3];

	public int chandelierItemType;

	public int platformItemType;

	public int doorItemType;

	public int[] lanternStyles = new int[3];

	public int[] shelfStyles = new int[3];

	public int[] bannerStyles = new int[6];

	public double globalFeatureScalar = 1.0;

	public double dungeonStepScalar = 1.0;

	public double hallStrengthScalar = 1.0;

	public double hallStepScalar = 1.0;

	public double hallInteriorToExteriorRatio = 0.5;

	public double hallSlantVariantScalar = 1.0;

	public double roomStrengthScalar = 1.0;

	public double roomStepScalar = 1.0;

	public double roomInteriorToExteriorRatio = 0.5;

	public double roomSlantVariantScalar = 1.0;

	public DungeonGenVars genVars => GenVars.dungeonGenVars[Iteration];
}
