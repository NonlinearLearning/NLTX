namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcCheckDeadSpawnCycleInput(
  bool IsQualifiedDeath,
  bool SkipNextSpawnCycleAlreadyMarked);
