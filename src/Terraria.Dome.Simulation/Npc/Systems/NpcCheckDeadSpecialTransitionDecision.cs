namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcCheckDeadSpecialTransitionDecision(
  bool ShouldReturnFromCheckDead,
  NpcCheckDeadSpecialTransitionKind Kind,
  NpcCheckDeadSpecialTransitionState State,
  NpcCheckDeadSpawnIntent? SpawnIntent);
