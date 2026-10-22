namespace MinimalImplementation.Server.World;

/// <summary>WetCollision's first central contact and the independent full-hitbox lava check.</summary>
public readonly record struct WorldLiquidContact(
    bool Wet, bool Honey, bool Shimmer, bool Lava);
