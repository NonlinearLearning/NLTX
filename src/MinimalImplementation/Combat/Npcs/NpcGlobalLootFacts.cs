using MinimalImplementation.Combat.Identity;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>
/// Required death-time facts for the production global item rules. All player zones
/// belong to the same closest player used by common loot and the luck adapter.
/// The caller keeps these external facts current through preparation and commit.
/// </summary>
public sealed record NpcGlobalLootFacts(
    CombatEntityIdentity Entity,
    long SourceNpcRevision,
    bool Friendly,
    bool HardMode,
    bool Halloween,
    bool Christmas,
    bool RemixWorld,
    bool DownedMechBoss1,
    bool DownedMechBoss2,
    bool DownedMechBoss3,
    bool DownedMechBossAny,
    bool DownedBoss3,
    bool DownedPlantBoss,
    double WorldSurfaceTiles,
    double RockLayerTiles,
    int UnderworldLayerTiles,
    int MaximumTilesX,
    int MaximumTilesY,
    float EnemyMoneyDropMultiplier,
    float EnemyDamageMultiplier,
    bool ZoneJungle,
    bool ZoneCorrupt,
    bool ZoneCrimson,
    bool ZoneHallow,
    bool ZoneSnow,
    bool ZoneDesert,
    bool ZoneBeach,
    bool ZoneDungeon) {
  // Defensive arithmetic bounds, not content limits. The multipliers are sampled
  // from the actual difficulty curves by the caller, not inferred from ExpertMode.
  internal bool HasValidNumbers => double.IsFinite(WorldSurfaceTiles) &&
      double.IsFinite(RockLayerTiles) && double.IsFinite(RockLayerTiles * 16d) &&
      UnderworldLayerTiles >= 0 && MaximumTilesX > 0 &&
      MaximumTilesY > 0 && MaximumTilesY <= int.MaxValue / 2 &&
      EnemyMoneyDropMultiplier > 0f && float.IsFinite(500f * EnemyMoneyDropMultiplier) &&
      EnemyDamageMultiplier > 0f && float.IsFinite(40f * EnemyDamageMultiplier);
}
