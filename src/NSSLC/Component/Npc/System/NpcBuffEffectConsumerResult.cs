namespace Terraria.Npc;

public readonly record struct NpcBuffEffectConsumerResult(
  int BuffSlotSyncRequests,
  int WaterPerishableCleanupRequests);
