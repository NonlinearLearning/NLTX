namespace Terraria.WorldGeneration.Housing;

public enum HousingRoomValidationFailure : byte
{
  None,
  TooCloseToWorldEdge,
  StartedInSolidTile,
  BlockingTile,
  OpenGate,
  RoomTooBig,
  UnsafeWall,
  MissingWall,
  RoomTooSmall,
  MissingRequirement,
}
