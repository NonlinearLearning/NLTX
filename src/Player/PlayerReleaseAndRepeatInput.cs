namespace Terraria.Player;

public readonly record struct PlayerReleaseAndRepeatInput(
  bool ControlJump,
  bool ControlUp,
  bool ControlLeft,
  bool ControlRight,
  bool ControlDown,
  bool ControlDash,
  bool ReleaseUseItem,
  bool ReleaseUseTile,
  bool TryKeepingHoveringDown,
  bool TryKeepingHoveringUp);
