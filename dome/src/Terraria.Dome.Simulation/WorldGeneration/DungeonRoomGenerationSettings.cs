using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct DungeonRoomGenerationSettings
{
  public DungeonRoomGenerationSettings(
    DungeonRoomType roomType,
    int randomSeed,
    int progressionStage,
    bool startingRoom,
    int overridePaintTile = -1,
    int overridePaintWall = -1,
    bool forceStyleForDoorsAndPlatforms = false,
    bool onCurvedLine = false,
    DungeonSnakeOrientation orientation = DungeonSnakeOrientation.Unknown,
    int? hallwayPointAdjuster = null)
  {
    if (!Enum.IsDefined(roomType))
    {
      throw new ArgumentOutOfRangeException(nameof(roomType));
    }

    if (!Enum.IsDefined(orientation))
    {
      throw new ArgumentOutOfRangeException(nameof(orientation));
    }

    ArgumentOutOfRangeException.ThrowIfNegative(progressionStage);

    if (overridePaintTile < -1)
    {
      throw new ArgumentOutOfRangeException(nameof(overridePaintTile));
    }

    if (overridePaintWall < -1)
    {
      throw new ArgumentOutOfRangeException(nameof(overridePaintWall));
    }

    if (hallwayPointAdjuster is < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(hallwayPointAdjuster));
    }

    RoomType = roomType;
    RandomSeed = randomSeed;
    ProgressionStage = progressionStage;
    StartingRoom = startingRoom;
    OverridePaintTile = overridePaintTile;
    OverridePaintWall = overridePaintWall;
    ForceStyleForDoorsAndPlatforms = forceStyleForDoorsAndPlatforms;
    OnCurvedLine = onCurvedLine;
    Orientation = orientation;
    HallwayPointAdjuster = hallwayPointAdjuster;
  }

  public DungeonRoomType RoomType { get; }

  public int RandomSeed { get; }

  public int ProgressionStage { get; }

  public bool StartingRoom { get; }

  public int OverridePaintTile { get; }

  public int OverridePaintWall { get; }

  public bool ForceStyleForDoorsAndPlatforms { get; }

  public bool OnCurvedLine { get; }

  public DungeonSnakeOrientation Orientation { get; }

  public int? HallwayPointAdjuster { get; }
}
