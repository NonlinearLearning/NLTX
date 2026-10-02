namespace Terraria.Npc;

public readonly record struct NpcKnockbackInput(
  float Knockback,
  int HitDirection,
  float KnockbackResistance,
  bool OnFire2,
  bool ExpertMode,
  bool NoGravity);
