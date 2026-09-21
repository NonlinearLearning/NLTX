using System.Numerics;

namespace Terraria.SpatialMotionPhysics;

public readonly record struct BallCollisionPayload(
  Vector2 Normal,
  Vector2 ImpactPoint,
  TileHandle Tile,
  EntityReference Entity,
  float TimeScale);
