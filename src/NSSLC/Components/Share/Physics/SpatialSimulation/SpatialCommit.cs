using Terraria.Entity;
using Terraria.Liquid;
using Terraria.Movement;

namespace Terraria.Physics;

/// <summary>State that the spatial owner is allowed to write at the commit barrier.</summary>
public readonly record struct SpatialCommit(
    LocationComponent Location,
    VelocityComponent Velocity,
    GroundingComponent Grounding,
    LiquidContactComponent LiquidContact);
