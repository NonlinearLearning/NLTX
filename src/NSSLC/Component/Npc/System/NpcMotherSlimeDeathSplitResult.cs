namespace Terraria.Npc;

public readonly record struct NpcMotherSlimeDeathSplitResult(
  int SpawnCalls,
  int SpawnedChildren,
  int SyncRequests,
  int RejectedSpawns);
