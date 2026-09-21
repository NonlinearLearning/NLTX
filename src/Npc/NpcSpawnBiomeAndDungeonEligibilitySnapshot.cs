namespace Terraria.Npc;

// status: implementation-started
// sourceMembers: waterTile, nearGranite, nearMarble, dualDungeonsSpawnRules,
// inDualDungeon, tresspassingDualDungeon
// lifecycle: one spawn evaluation; not durable entity state
public readonly record struct NpcSpawnBiomeAndDungeonEligibilitySnapshot(
  bool WaterTile,
  bool NearGranite,
  bool NearMarble,
  bool DualDungeonsSpawnRules,
  bool InDualDungeon,
  bool TresspassingDualDungeon);
