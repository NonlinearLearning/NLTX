namespace Terraria.Dome.Simulation.Player;

public readonly record struct PlayerInputState(
  bool MoveLeft,
  bool MoveRight,
  bool Jump,
  bool Fire,
  bool UseItem,
  bool Up,
  bool Down,
  bool UseTile,
  bool Dash,
  int Facing,
  int SelectedSlot);
