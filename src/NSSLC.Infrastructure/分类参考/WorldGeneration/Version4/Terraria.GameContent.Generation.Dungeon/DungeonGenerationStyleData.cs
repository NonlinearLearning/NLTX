using System.Collections.Generic;
using Terraria.GameContent.Generation.Dungeon.Features;
using Terraria.GameContent.Generation.Dungeon.Rooms;
using Terraria.ID;
using Terraria.Utilities;

namespace Terraria.GameContent.Generation.Dungeon;

public class DungeonGenerationStyleData
{
	public byte Style;

	public int UnbreakableWallProgressionTier = -1;

	public ushort BrickTileType;

	public ushort? BrickGrassTileType;

	public ushort BrickCrackedTileType;

	public ushort BrickWallType;

	public ushort WindowGlassWallType;

	public ushort WindowClosedGlassWallType;

	public ushort WindowEdgeWallType;

	public int[] WindowPlatformItemTypes;

	public ushort PitTrapTileType;

	public int LiquidType = -1;

	public int LockedBiomeChestType;

	public int LockedBiomeChestStyle;

	public int BiomeChestItemType;

	public int BiomeChestLootItemType;

	public int[] ChestItemTypes;

	public int[] DoorItemTypes;

	public int[] PlatformItemTypes;

	public int[] ChandelierItemTypes;

	public int[] LanternItemTypes;

	public int[] TableItemTypes;

	public int[] WorkbenchItemTypes;

	public int[] CandleItemTypes;

	public int[] VaseOrStatueItemTypes;

	public int[] BookcaseItemTypes;

	public int[] ChairItemTypes;

	public int[] BedItemTypes;

	public int[] PianoItemTypes;

	public int[] DresserItemTypes;

	public int[] SofaItemTypes;

	public int[] BathtubItemTypes;

	public int[] LampItemTypes;

	public int[] CandelabraItemTypes;

	public int[] ClockItemTypes;

	public int[] BannerItemTypes;

	public bool EdgeDither;

	public DungeonRoomType BiomeRoomType;

	public List<DungeonGenerationStyleData> SubStyles;

	public virtual bool CanGenerateFeatureAt(DungeonData data, DungeonRoom room, IDungeonFeature
  feature, int x, int y){
  return new bool ();
}
	public virtual void GetBookshelfMinMaxSizes(int defaultMin, int defaultMax, out int min, out int
  max){
  min = new int ();
  max = new int ();
}
}
