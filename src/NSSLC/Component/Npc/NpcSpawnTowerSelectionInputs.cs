namespace Terraria.Npc;

public readonly record struct NpcSpawnTowerSelectionInputs(
  NpcSpawnEventAndTowerEligibilitySnapshot EventAndTower,
  int SpawnTileX,
  int SpawnTileY);
