using System.Numerics;

namespace Terraria.SpatialMotionPhysics;

public sealed record EntityGeometrySnapshot(
  Vector2 Position,
  Vector2 VisualPosition,
  Vector2 Center,
  Vector2 Left,
  Vector2 Right,
  Vector2 Top,
  Vector2 TopLeft,
  Vector2 TopRight,
  Vector2 Bottom,
  Vector2 BottomLeft,
  Vector2 BottomRight,
  Vector2 Size,
  EntityHitbox Hitbox);
