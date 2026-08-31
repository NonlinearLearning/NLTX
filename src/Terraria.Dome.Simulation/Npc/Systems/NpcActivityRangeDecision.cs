namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcActivityRangeDecision(
  bool HasActivePlayerInRange,
  bool ShouldRefreshInactivityTimer);
