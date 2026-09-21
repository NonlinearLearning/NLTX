namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcCheckDeadSpawnIntent(
  NpcHandle Parent,
  int DefinitionId,
  SimulationVector Position,
  float ChildAi3,
  bool ChildNetUpdate);
