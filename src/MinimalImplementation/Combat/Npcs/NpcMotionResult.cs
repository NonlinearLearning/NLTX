using System.Numerics;

namespace MinimalImplementation.Combat.Npcs;

public sealed record NpcMotionResult(
    Vector2 Position, Vector2 Velocity, bool CollideX, bool CollideY,
    Vector2? OldPosition = null, Vector2? OldVelocity = null);
