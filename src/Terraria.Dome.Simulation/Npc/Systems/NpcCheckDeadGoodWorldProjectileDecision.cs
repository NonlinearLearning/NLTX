namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcCheckDeadGoodWorldProjectileDecision(
  bool ShouldSpawn,
  int ProjectileType,
  SimulationVector Position,
  SimulationVector Velocity,
  int Damage,
  float Knockback,
  int ProjectileOwner);
