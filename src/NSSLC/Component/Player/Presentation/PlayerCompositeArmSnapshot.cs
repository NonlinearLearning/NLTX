namespace Terraria.Player.Presentation;

public readonly record struct PlayerCompositeArmSnapshot(
  bool IsEnabled,
  PlayerArmStretchAmount Stretch,
  float Rotation);
