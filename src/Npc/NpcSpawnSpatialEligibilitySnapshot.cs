namespace Terraria.Npc;

// status: implementation-started
// sourceMembers: surfaceSpawn, spawnUndergroundDesert, hardDungeon,
// deeperThanRockLayer, underGround, isOcean, isBeach, skyBehindPlayer,
// livingTree, inRemixStartingArea
// lifecycle: one spawn evaluation; not durable entity state
public readonly record struct NpcSpawnSpatialEligibilitySnapshot(
  bool SurfaceSpawn,
  bool SpawnUndergroundDesert,
  bool HardDungeon,
  bool DeeperThanRockLayer,
  bool UnderGround,
  bool IsOcean,
  bool IsBeach,
  bool SkyBehindPlayer,
  bool LivingTree,
  bool InRemixStartingArea);
