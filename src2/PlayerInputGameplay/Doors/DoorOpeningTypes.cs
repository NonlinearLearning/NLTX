namespace NLTX.PlayerInputGameplay.Doors;

public readonly record struct DoorTileCoordinate(int X, int Y);

public readonly record struct DoorOpeningGeometry(
  int HitboxLeft,
  int HitboxTop,
  int HitboxWidth,
  int HitboxHeight,
  int CheckLeft,
  int CheckTop,
  int CheckWidth,
  int CheckHeight);

public readonly record struct DoorOpenCloseTogglingInfo(
  DoorTileCoordinate Tile,
  int HandlerTileType,
  int IntendedOpeningDirection,
  int PlayerGravityDirection);

public readonly record struct DoorOpeningRequest(
  DoorTileCoordinate Tile,
  int HandlerTileType,
  DoorOpeningGeometry Geometry,
  int IntendedOpeningDirection,
  int PlayerGravityDirection);
