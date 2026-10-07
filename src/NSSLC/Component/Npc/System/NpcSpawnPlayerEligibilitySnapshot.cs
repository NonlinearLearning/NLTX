namespace Terraria.Npc;

public readonly record struct NpcSpawnPlayerEligibilitySnapshot(
  bool IsPlayerActive,
  bool IsPlayerDead,
  bool IsJourneyMode,
  bool IsSpawnRatePowerUnlocked,
  bool DoesSpawnRatePowerDisablePlayer,
  bool IsNearMoonLord);
