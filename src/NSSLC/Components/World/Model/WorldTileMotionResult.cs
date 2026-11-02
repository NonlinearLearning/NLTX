using System.Numerics;

namespace Terraria.World;

/// <summary>
/// Tile collision output computed by <see cref="WorldTileMotionSolver" />.
/// </summary>
/// <remarks>
/// The World domain owns this shape. <see cref="WorldRuntime" /> projects it to the
/// <c>Terraria.Physics</c> port result when it answers a spatial query.
/// </remarks>
public readonly record struct WorldTileMotionResult(
    Vector2 Position,
    Vector2 Velocity,
    bool CollidedX,
    bool CollidedY)
{
  public bool IsFinite =>
      float.IsFinite(Position.X) && float.IsFinite(Position.Y) &&
      float.IsFinite(Velocity.X) && float.IsFinite(Velocity.Y);
}
