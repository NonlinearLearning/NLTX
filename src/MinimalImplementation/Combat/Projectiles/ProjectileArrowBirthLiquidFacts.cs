namespace MinimalImplementation.Combat.Projectiles;

/// <summary>Outputs captured from NewProjectile's single WetCollision birth query.</summary>
public sealed record ProjectileArrowBirthLiquidFacts(
    bool WetCollision,
    bool Honey,
    bool Shimmer);
