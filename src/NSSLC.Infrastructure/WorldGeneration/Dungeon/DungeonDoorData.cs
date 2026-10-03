using NSSLC.WorldGeneration.Geometry;

namespace NSSLC.WorldGeneration.GameContent.Generation.Dungeon;

public struct DungeonDoorData
{
	public Point Position;

	public ushort? OverrideBrickTileType;

	public ushort? OverrideBrickWallType;

	public int? OverrideStyle;

	public int Direction;

	public bool InAHallway;

	public int? OverrideWidthFluff;

	public bool SkipOtherDoorsCheck;

	public bool SkipSpaceCheck;

	public bool AlwaysClearArea;
}
