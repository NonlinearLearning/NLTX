namespace MinimalImplementation.Combat.Projectiles;

/// <summary>
/// Actual LavaCollision result followed by WetCollision and its honey/shimmer outputs.
/// These are per-query world facts, not the projectile's persistent liquid flags.
/// </summary>
public sealed record ProjectileLiquidFacts(
    bool LavaCollision, bool WetCollision, bool Honey, bool Shimmer);
