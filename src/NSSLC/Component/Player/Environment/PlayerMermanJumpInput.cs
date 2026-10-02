namespace Terraria.Player.Environment;

public readonly record struct PlayerMermanJumpInput(
  bool HasActiveJumpCounter,
  bool IsAirborne,
  bool IsMerman,
  bool MountActive,
  bool CartMount);
