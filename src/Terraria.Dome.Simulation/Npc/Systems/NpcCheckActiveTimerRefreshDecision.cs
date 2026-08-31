namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcCheckActiveTimerRefreshDecision(
  bool ShouldRefresh,
  int TimeLeft,
  bool DespawnEncouraged);
