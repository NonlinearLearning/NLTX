namespace Terraria.Share.Physics;

/// <summary>Declares how an entity participates in tile collision queries.</summary>
public readonly record struct TileCollisionCapabilityComponent(
    bool Enabled,
    bool FallThroughPlatforms = false,
    bool UseSlopes = true,
    bool UseHoikCorrection = true);
