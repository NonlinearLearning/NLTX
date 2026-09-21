namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcCheckActiveDeactivationDecision(
  bool WasProcessed,
  bool IsActive,
  int TimeLeft,
  int Life,
  bool ShouldSkipNextSpawnCycle);
