namespace MinimalImplementation.Combat.Npcs;

/// <summary>
/// Trusted ordinary-world facts. WasWet/Honey/Shimmer are the prior update's flags,
/// before this update's collision queries. InRockLayer refers to the NPC's top tile.
/// </summary>
public sealed record NpcSlimeEnvironment(
    bool IsDay,
    double WorldSurfaceTiles,
    int MaximumTilesX,
    bool InRockLayer,
    bool SlimeRain,
    bool GenuineParty,
    bool UsesSpecialWorldRules,
    bool WasWet = false,
    bool WasHoneyWet = false,
    bool WasShimmerWet = false);
