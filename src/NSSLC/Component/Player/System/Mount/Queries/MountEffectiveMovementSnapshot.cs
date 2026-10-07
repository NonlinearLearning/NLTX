namespace Terraria.Player.Mount;

public readonly record struct MountEffectiveMovementSnapshot(
  float RunSpeed,
  float DashSpeed,
  float SwimSpeed,
  float Acceleration,
  float JumpSpeed,
  int JumpHeight,
  bool IsSuperCartOverride);
