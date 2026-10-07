namespace Terraria.Player.Movement;

public readonly record struct PlayerTraversalPhysicsInput(
  float DefaultGravity,
  bool PortalPhysicsEnabled,
  bool Wet,
  bool IsPerformingJumpDownDash,
  bool ShimmerWet,
  bool Shimmering,
  bool HoneyWet,
  bool Merman,
  bool Trident,
  bool LavaWet,
  bool ControlUp,
  bool VortexDebuff);
