namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcCheckActiveKeepAliveDecision(
  bool ShouldKeepActive,
  bool ShouldRefreshInactivityTimer);
