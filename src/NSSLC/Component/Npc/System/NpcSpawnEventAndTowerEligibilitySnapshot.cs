namespace Terraria.Npc;

// status: implementation-started
// sourceMembers: ZoneTowerSolar, ZoneTowerVortex, ZoneTowerNebula,
// ZoneTowerStardust, ZoneOldOneArmy, ZoneWaterCandle, ZonePeaceCandle,
// ZoneShadowCandle
// lifecycle: one spawn evaluation; not durable entity state
public readonly record struct NpcSpawnEventAndTowerEligibilitySnapshot(
  bool ZoneTowerSolar,
  bool ZoneTowerVortex,
  bool ZoneTowerNebula,
  bool ZoneTowerStardust,
  bool ZoneOldOneArmy,
  bool ZoneWaterCandle,
  bool ZonePeaceCandle,
  bool ZoneShadowCandle);
