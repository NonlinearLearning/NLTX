using System.Numerics;

namespace MinimalImplementation.Server.World;

/// <summary>Resolved tile displacement; Velocity follows Collision.TileCollision semantics.</summary>
public readonly record struct WorldMotionResult(
    Vector2 Position, Vector2 Velocity, bool CollideX, bool CollideY);
