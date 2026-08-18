namespace Terraria.Dome.Simulation;

public readonly record struct PlayerSnapshot(
  PlayerHandle Player,
  SimulationVector Position,
  SimulationVector Velocity,
  int Facing,
  bool IsGrounded,
  int Health,
  bool IsActive,
  int RespawnTicks,
  string AccountUuid = "",
  byte AssignedSlot = 0,
  int Mana = 0,
  int MaximumMana = 0);
