namespace Terraria.Dome.Simulation;

public readonly record struct ProjectileSnapshot(
  SimulationVector Position,
  int RemainingLifetime);
