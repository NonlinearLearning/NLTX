using System.Numerics;

namespace Terraria.Npc;

public readonly record struct NpcSoulDrainVisualEffectInput(
  NpcSoulDrainEligibilityInput Eligibility,
  Vector2 NpcVelocity);
