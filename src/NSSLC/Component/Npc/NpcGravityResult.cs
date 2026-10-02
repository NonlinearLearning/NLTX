namespace Terraria.Npc;

public readonly record struct NpcGravityResult(
  float Gravity,
  float MaximumFallSpeed,
  float VelocityYAfter);
