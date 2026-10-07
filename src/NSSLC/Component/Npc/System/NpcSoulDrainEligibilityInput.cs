using System.Collections.Generic;
using System.Numerics;

namespace Terraria.Npc;

public readonly record struct NpcSoulDrainEligibilityInput(
  bool IsSoulDrainActive,
  Vector2 NpcCenter,
  IReadOnlyList<NpcSoulDrainPlayerSnapshot>? PlayerSnapshot);
