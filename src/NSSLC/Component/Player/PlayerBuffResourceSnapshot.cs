namespace Terraria.Player;

public readonly record struct PlayerBuffResourceSnapshot(
  int BreathMax,
  int Breath,
  int LavaMax,
  int LavaTime,
  bool IgnoreWater,
  bool LavaVision,
  float LavaOpacity);
