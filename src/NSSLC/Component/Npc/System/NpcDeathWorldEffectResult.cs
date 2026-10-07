namespace Terraria.Npc;

public readonly record struct NpcDeathWorldEffectResult(
  int SpawnCalls,
  int SyncRequests,
  int TileSearchAttempts,
  int ExhaustedSearches);
