using Terraria.Npc;

namespace Terraria.Npc.Queries;

public static class NpcSpawnEligibilityQuery
{
  /// <summary>
  /// Evaluates one player's natural-spawn gates from a single attempt snapshot.
  /// </summary>
  public static NpcSpawnPlayerEligibilityResult Evaluate(
    in NpcSpawnPlayerEligibilitySnapshot snapshot)
  {
    if (!snapshot.IsPlayerActive)
    {
      return new NpcSpawnPlayerEligibilityResult(
        NpcSpawnPlayerEligibilityReason.PlayerInactive);
    }

    if (snapshot.IsPlayerDead)
    {
      return new NpcSpawnPlayerEligibilityResult(
        NpcSpawnPlayerEligibilityReason.PlayerDead);
    }

    if (snapshot.IsJourneyMode &&
      snapshot.IsSpawnRatePowerUnlocked &&
      snapshot.DoesSpawnRatePowerDisablePlayer)
    {
      return new NpcSpawnPlayerEligibilityResult(
        NpcSpawnPlayerEligibilityReason.JourneySpawnRateDisabled);
    }

    if (snapshot.IsNearMoonLord)
    {
      return new NpcSpawnPlayerEligibilityResult(
        NpcSpawnPlayerEligibilityReason.NearMoonLord);
    }

    return new NpcSpawnPlayerEligibilityResult(
      NpcSpawnPlayerEligibilityReason.Eligible);
  }
}
