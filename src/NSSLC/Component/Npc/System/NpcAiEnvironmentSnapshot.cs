using System.Numerics;

namespace Terraria.Npc;

public readonly record struct NpcAiEnvironmentSnapshot(
  bool DayTime,
  bool IsGrounded,
  bool HasHome,
  Vector2 HomeCenter);
