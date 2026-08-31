namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcCheckDeadSpawnCycleDecision(
  bool ShouldMarkSkipNextSpawnCycle,
  bool WasProcessed);
