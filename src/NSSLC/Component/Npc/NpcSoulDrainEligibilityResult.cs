using System.Collections.Generic;

namespace Terraria.Npc;

public readonly record struct NpcSoulDrainEligibilityResult(
  bool Succeeded,
  NpcSoulDrainEligibilityFailureReason FailureReason,
  IReadOnlyList<int> EligibleLegacyPlayerSlots);
