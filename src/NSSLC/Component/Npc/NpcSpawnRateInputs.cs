namespace Terraria.Npc;

public readonly record struct NpcSpawnRateInputs(
  NpcSpawnContextSnapshot Context,
  NpcSpawnPolicyEligibilitySnapshot Policy,
  NpcSpawnSpatialEligibilitySnapshot Spatial,
  NpcSpawnBiomeAndDungeonEligibilitySnapshot BiomeAndDungeon,
  NpcSpawnBiomeZoneEligibilitySnapshot BiomeZones,
  NpcSpawnEventAndTowerEligibilitySnapshot EventAndTower,
  NpcSpawnRatePlayerInputs Player,
  NpcSpawnRateWorldInputs World);
