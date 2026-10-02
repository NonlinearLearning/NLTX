namespace Terraria.Npc;

public readonly record struct NpcGravityEnvironmentSnapshot(
  float NpcPositionY,
  int WorldMaxTilesX,
  double WorldSurface,
  bool Wet,
  bool ShimmerWet,
  bool HoneyWet);
