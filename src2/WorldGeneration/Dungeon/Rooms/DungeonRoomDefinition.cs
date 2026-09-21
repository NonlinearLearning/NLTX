using Terraria.WorldGeneration.Dungeon.Bounds;
using Terraria.WorldGeneration.Dungeon.Styles;

namespace Terraria.WorldGeneration.Dungeon.Rooms;

public sealed class DungeonRoomDefinition
{
  public DungeonRoomDefinition(
    int roomId,
    int roomType,
    int randomSeed,
    DungeonStyleMaterialDefinition styleData)
  {
    if (roomId < 0 || roomType < 0 || randomSeed < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(roomId));
    }

    RoomId = roomId;
    RoomType = roomType;
    RandomSeed = randomSeed;
    StyleData = styleData ?? throw new ArgumentNullException(nameof(styleData));
  }

  public int RoomId { get; }

  public int RoomType { get; }

  public int RandomSeed { get; }

  public DungeonStyleMaterialDefinition StyleData { get; }

  public int ProgressionStage { get; init; }

  public bool StartingRoom { get; init; }

  public bool IsEntranceRoom { get; init; }

  public DungeonTilePoint RoomPosition { get; init; }

  public DungeonTilePoint? HallwayConnectionPointOverride { get; init; }

  public bool OnCurvedLine { get; init; }

  public int Orientation { get; init; }

  public int BoundingRadius { get; init; }

  public float OverrideStrength { get; init; }

  public int OverrideSteps { get; init; }

  public DungeonTilePoint? OverrideStartPosition { get; init; }

  public DungeonTilePoint? OverrideEndPosition { get; init; }

  public DungeonTilePoint? OverrideVelocity { get; init; }

  public float OverrideInteriorToExteriorRatio { get; init; }
}
