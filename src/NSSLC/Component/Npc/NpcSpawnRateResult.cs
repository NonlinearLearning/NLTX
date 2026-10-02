namespace Terraria.Npc;

public readonly record struct NpcSpawnRateResult(
  int SpawnRate,
  int MaxSpawns,
  bool NoWorms,
  bool SpawnFriendly);
