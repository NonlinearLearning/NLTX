namespace Terraria.Dome.Simulation;

public readonly record struct PlayerSnapshot(
  PlayerHandle Player,
  SimulationVector Position,
  SimulationVector Velocity,
  int Facing,
  bool IsGrounded,
  int Health);
