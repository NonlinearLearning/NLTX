using System;
using System.Collections.Generic;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Housing;

public readonly record struct HousingRoomEvaluationResult(
  bool CanSpawn,
  HousingRoomValidationFailure Failure,
  int RoomTileCount,
  TilePosition RoomMinimum,
  TilePosition RoomMaximum,
  HousingRoomRequirementResult Requirements,
  bool HasStinkbug,
  bool HasEchoStinkbug)
{
  public IReadOnlyList<TilePosition> VisitedTiles { get; init; } = Array.Empty<TilePosition>();

  public IReadOnlyList<int> ClassifiedTileTypes { get; init; } = Array.Empty<int>();

  public bool HasRoomBounds => RoomTileCount > 0;
}
