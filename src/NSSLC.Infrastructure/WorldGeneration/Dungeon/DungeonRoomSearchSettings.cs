using NSSLC.WorldGeneration.GameContent.Generation.Dungeon.Rooms;

namespace NSSLC.WorldGeneration.GameContent.Generation.Dungeon;

public struct DungeonRoomSearchSettings
{
	public int Fluff;

	public DungeonRoom ExcludedRoom;

	public ProgressionStageCheck ProgressionStageCheck;

	public int? ProgressionStage;

	public int? MaximumDistance;
}
