namespace Terraria.NpcTownBestiary;

public readonly record struct ConditionalDialogueEligibilityResult(
  bool IsEligible,
  ConditionalDialogueKey? Key,
  bool ShowIndicator);
