namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcCheckActiveTimerRefreshInput(
  bool IsNpcActive,
  bool HasScreenRangePlayer,
  bool DoesNotDespawnToInactivityAndCountsNpcSlots,
  int ActiveTime,
  int TimeLeft,
  bool DespawnEncouraged);
