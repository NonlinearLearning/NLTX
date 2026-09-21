using System.Numerics;

namespace Terraria.SpatialMotionPhysics;

public readonly record struct InterceptionResult(
  bool InterceptionHappens,
  Vector2 InterceptionPosition,
  float InterceptionTime,
  Vector2 ChaserVelocity);
