using System.Numerics;

namespace Terraria.SpatialMotionPhysics;

public readonly record struct TeleportCommitCommand(
  int EntityId,
  Vector2 Position,
  Vector2 Velocity);
