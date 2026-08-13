namespace Terraria.Dome.Simulation;

public readonly record struct PlayerInput(
  PlayerHandle Player,
  bool MoveLeft,
  bool MoveRight,
  bool Jump,
  bool Fire);
