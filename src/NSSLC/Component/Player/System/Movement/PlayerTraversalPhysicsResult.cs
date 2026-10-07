namespace Terraria.Player.Movement;

public readonly record struct PlayerTraversalPhysicsResult(
  float Gravity,
  float MaxFallSpeed,
  float MaxRunSpeed,
  float RunAcceleration,
  float RunSlowdown,
  int JumpHeight,
  float JumpSpeed);
