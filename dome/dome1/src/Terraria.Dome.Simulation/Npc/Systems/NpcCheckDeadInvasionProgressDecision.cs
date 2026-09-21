namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcCheckDeadInvasionProgressDecision(
  bool Applies,
  int InvasionGroup,
  int Points,
  int RemainingSize,
  int ProgressStart);
