namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcCheckDeadQualificationInput(
  bool IsNpcActive,
  bool IsRootSegment,
  int Life);
