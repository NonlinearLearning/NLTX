namespace Terraria.Dome.Simulation;

public readonly record struct NpcSnapshot(
  NpcHandle Npc,
  SimulationVector Position,
  int Health,
  bool HasTarget);
