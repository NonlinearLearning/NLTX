namespace MinimalImplementation.Combat.Npcs;

/// <summary>Trusted world facts; liquid flags are from the previous committed update.</summary>
public sealed record NpcZombieEnvironment(
    bool IsDay,
    bool Eclipse,
    bool ExpertMode,
    double WorldSurfaceTiles,
    int MaximumTilesX,
    int MaximumTilesY,
    bool UsesSpecialWorldRules,
    bool WasWet = false,
    bool WasHoneyWet = false,
    bool WasShimmerWet = false);
