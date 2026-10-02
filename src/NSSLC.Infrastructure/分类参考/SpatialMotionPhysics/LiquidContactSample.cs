namespace Terraria.SpatialMotionPhysics;

public readonly record struct LiquidContactSample(
  bool Wet,
  bool ShimmerWet,
  bool HoneyWet,
  byte WetCount,
  bool LavaWet);
