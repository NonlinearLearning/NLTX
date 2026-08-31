namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcInvasionBossCapDecision(
  bool HasReachedCap,
  float ActiveBossSlotCost,
  int PerPlayerBossSlotLimit,
  long GlobalBossSlotLimit,
  int ActivePlayerCount);
