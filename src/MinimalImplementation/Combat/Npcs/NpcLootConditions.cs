namespace MinimalImplementation.Combat.Npcs;

/// <summary>Trusted condition results from the world and closest-player owners.</summary>
public sealed record NpcLootConditions(
    bool ExpertMode,
    bool ShouldDropExtraGel,
    bool EyeOfCthulhuDefeatedAndNoAltarsInWorld,
    bool SkyblockIsUpNoSickle);
