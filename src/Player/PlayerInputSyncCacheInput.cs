namespace Terraria.Player;

public readonly record struct PlayerInputSyncCacheInput(
  bool ControlLeft,
  bool ControlRight,
  bool ControlUp,
  bool ControlDown,
  bool ControlJump);
