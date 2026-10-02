namespace Terraria.Player.Environment;

public readonly record struct PlayerFlipperJumpInput(
  bool CanStartNewJump,
  bool IsWet,
  bool HasFlippers);
