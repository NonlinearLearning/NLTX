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
  public bool HasRoomBounds => RoomTileCount > 0;
}
