namespace Terraria.Npc;

// status: implementation-started
// sourceMembers: spawnSpaceX, spawnSpaceY, fairyLog, numberOfActivePlayers,
// reachedInvasionBossCap, pX, pY, luck, dayTime, raining
// lifecycle: one spawn evaluation; not a durable entity component
public readonly record struct NpcSpawnContextSnapshot(
  int SpawnSpaceX,
  int SpawnSpaceY,
  bool FairyLog,
  int NumberOfActivePlayers,
  bool ReachedInvasionBossCap,
  int PlayerTileX,
  int PlayerTileY,
  float Luck,
  bool DayTime,
  bool Raining);
