using System;
using System.Numerics;

namespace Terraria.Physics;

/// <summary>Tile collision output for one movement query.</summary>
public readonly record struct SpatialTileMotionResult(
    Vector2 Position,
    Vector2 Velocity,
    bool CollidedX,
    bool CollidedY)
{
  public bool IsFinite =>
      float.IsFinite(Position.X) && float.IsFinite(Position.Y) &&
      float.IsFinite(Velocity.X) && float.IsFinite(Velocity.Y);
}
