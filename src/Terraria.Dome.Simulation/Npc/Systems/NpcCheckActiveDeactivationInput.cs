namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcCheckActiveDeactivationInput(
  bool IsNpcActive,
  bool HasKeepAlive,
  bool DoesNotDespawnToInactivityAndCountsNpcSlots,
  int TimeLeft,
  int Life);
