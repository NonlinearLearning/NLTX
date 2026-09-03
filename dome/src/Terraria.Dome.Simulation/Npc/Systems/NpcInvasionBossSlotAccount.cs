namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcInvasionBossSlotAccount(
  int NpcType,
  bool IsActive,
  float NpcSlotCost);
