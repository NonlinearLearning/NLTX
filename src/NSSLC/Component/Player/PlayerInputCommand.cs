namespace Terraria.Player;

// Legacy slot is a lookup key; protocol sequence and facing stay outside this P10 command.
public readonly record struct PlayerInputCommand(
  LegacyPlayerSlot Player,
  bool ControlLeft,
  bool ControlRight,
  bool ControlUp,
  bool ControlDown,
  bool ControlJump,
  bool ControlTorch,
  bool ControlDash,
  bool ControlDownHold);
