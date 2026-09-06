using System;

namespace Terraria.WorldGeneration.Components;

public sealed class HousingScanStateComponent
{
  public HousingScanStateComponent(long generationId)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
  }

  public long GenerationId { get; }

  public ulong ScanRevision { get; init; }

  public long Cursor { get; init; }

  public int? PrioritizedTownNpcType { get; init; }

  public int RoomTiles { get; init; }

  public int MaxRoomTiles { get; init; }

  public int MaxRoomSize { get; init; }

  public int? RoomX1 { get; init; }

  public int? RoomX2 { get; init; }

  public int? RoomY1 { get; init; }

  public int? RoomY2 { get; init; }

  public int? BestX { get; init; }

  public int? BestY { get; init; }

  public int? HighScore { get; init; }

  public bool? CanSpawn { get; init; }

  public bool? HouseTile { get; init; }

  public bool? RoomTorch { get; init; }

  public bool? RoomDoor { get; init; }

  public bool? RoomChair { get; init; }

  public bool? RoomTable { get; init; }

  public bool? RoomHasStinkbug { get; init; }

  public bool? RoomHasEchoStinkbug { get; init; }

  public bool CurrentlyTryingAlternateSpot { get; init; }

  public int? SharedRoomX { get; init; }

  public TilePosition? LastFoundHouse { get; init; }

  public string? FailureReason { get; init; }
}
