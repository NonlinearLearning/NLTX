namespace Terraria.Npc;

// status: implementation-started
// sourceMembers: townNPCs, skyMob, noWorms, noGroundWorms, invaders,
// spawnFriendly, ignoreSafeWalls, spawnSpider, isSpawningInWindDirection,
// offensiveToTim, playerHasStartingHealth
// lifecycle: one spawn evaluation; source state remains externally owned
public readonly record struct NpcSpawnPolicyEligibilitySnapshot(
  int TownNpcCount,
  bool SkyMob,
  bool NoWorms,
  bool NoGroundWorms,
  bool Invaders,
  bool SpawnFriendly,
  bool IgnoreSafeWalls,
  bool SpawnSpider,
  bool IsSpawningInWindDirection,
  bool OffensiveToTim,
  bool PlayerHasStartingHealth);
