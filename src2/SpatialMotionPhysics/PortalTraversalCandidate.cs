using System.Numerics;

namespace Terraria.SpatialMotionPhysics;

public readonly record struct PortalTraversalCandidate(
  int EntityId,
  Vector2 Position,
  Vector2 Velocity);
