namespace Terraria.Dome.Simulation;

public readonly record struct PlayerInput(
  PlayerHandle Player,
  bool MoveLeft,
  bool MoveRight,
  bool Jump,
  bool Fire,
  int SelectedSlot = 0,
  bool UseItem = false,
  int Facing = 0,
  bool Down = false,
  bool Up = false,
  bool UseTile = false,
  bool Dash = false);
