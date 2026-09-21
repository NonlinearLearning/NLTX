namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcCheckDeadGoodWorldProjectileInput(
  bool IsGoodWorld,
  int NpcType,
  SimulationVector Center,
  int ProjectileOwner);
