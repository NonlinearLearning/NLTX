using Terraria.Entity;
using Terraria.Movement;

namespace Terraria.Projectile;

/// <summary>Motion produced for a spatial commit adapter; it is not an ECS write.</summary>
public readonly record struct ProjectileMotionResult(
    LocationComponent Location,
    VelocityComponent Velocity,
    bool CollidedX,
    bool CollidedY,
    int CompletedSubsteps);
