namespace Terraria.SpatialMotionPhysics;

public readonly record struct BallPassThroughPayload(
  TileHandle Tile,
  EntityReference Entity,
  BallPassThroughType Type,
  float TimeScale);
