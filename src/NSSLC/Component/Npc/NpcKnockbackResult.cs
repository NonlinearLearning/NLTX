using System.Numerics;

namespace Terraria.Npc;

public readonly record struct NpcKnockbackResult(
  bool IsEligible,
  Vector2 VelocityBefore,
  Vector2 VelocityAfter);
