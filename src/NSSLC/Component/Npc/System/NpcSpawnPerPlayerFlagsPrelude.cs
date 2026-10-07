namespace Terraria.Npc;

public readonly record struct NpcSpawnPerPlayerFlagsPrelude(
  NpcSpawnContextSnapshot Context,
  NpcSpawnBiomeZoneEligibilitySnapshot BiomeZones,
  NpcSpawnEventAndTowerEligibilitySnapshot EventAndTower,
  bool DownedPlantBoss,
  bool HardMode,
  bool DualDungeonsSeed,
  bool PlayerInsideUnbreakableWalls,
  int DungeonProgressCanSafelyMatch,
  int DungeonProgressPlayerNeedsToMatch);
