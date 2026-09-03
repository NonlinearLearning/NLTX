using Terraria.Dome.Simulation;

namespace Terraria.Dome.Server.Replication;

public readonly record struct PlayerReplicationState(
  byte PlayerSlot,
  SimulationVector Position,
  SimulationVector Velocity,
  int Facing,
  bool IsActive,
  int Health,
  int MaximumHealth = 100);
