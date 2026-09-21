namespace Terraria.Player;

public readonly record struct PlayerInputCommand(
  LegacyPlayerSlot Player,
  DirectionKind Facing,
  bool ControlLeft,
  bool ControlRight,
  bool ControlUp,
  bool ControlDown,
  bool ControlJump,
  bool ControlTorch,
  bool ControlDash,
  bool ControlDownHold,
  uint Sequence);
