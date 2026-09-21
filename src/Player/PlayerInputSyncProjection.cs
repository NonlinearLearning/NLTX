namespace Terraria.Player;

public readonly record struct PlayerInputSyncProjection(
  bool ControlLeft,
  bool ControlRight,
  bool ControlUp,
  bool ControlDown,
  bool ControlJump,
  bool PressingAnyInput);
