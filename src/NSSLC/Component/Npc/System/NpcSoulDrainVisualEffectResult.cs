namespace Terraria.Npc;

public readonly record struct NpcSoulDrainVisualEffectResult(
  bool Succeeded,
  NpcSoulDrainEligibilityFailureReason EligibilityFailureReason,
  int EligiblePlayerCount,
  int DustCreatedCount);
