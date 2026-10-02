using System.Numerics;

namespace Terraria.Npc;

public readonly record struct NpcKnockbackRequest(
  NpcTypeId NpcType,
  Vector2 CurrentVelocity,
  float Knockback,
  int HitDirection,
  float KnockbackResistance,
  bool OnFire2,
  bool Critical,
  int ResolvedDamage,
  int LifeMaximum,
  bool ExpertMode,
  bool NoGravity);
