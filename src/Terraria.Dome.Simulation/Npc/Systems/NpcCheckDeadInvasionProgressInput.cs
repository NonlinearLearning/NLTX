namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcCheckDeadInvasionProgressInput(
  int NpcType,
  int InvasionType,
  int InvasionSize,
  int InvasionSizeStart);
