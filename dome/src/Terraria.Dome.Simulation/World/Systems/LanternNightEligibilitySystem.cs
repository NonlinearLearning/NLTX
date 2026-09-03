using System;

namespace Terraria.Dome.Simulation.WorldModel.Systems;

public sealed class LanternNightEligibilitySystem
{
  private const int FirstLegacyBossTypeId = 13;
  private const int LastLegacyBossTypeId = 15;

  public bool CanPersist(
    WorldClockSnapshot clock,
    WorldProgressionState progression,
    LanternNightEligibilitySnapshot eligibility)
  {
    ArgumentNullException.ThrowIfNull(progression);
    ArgumentNullException.ThrowIfNull(eligibility);
    return !clock.IsDayTime && CanStart(progression, eligibility);
  }

  public bool CanStart(
    WorldProgressionState progression,
    LanternNightEligibilitySnapshot eligibility)
  {
    ArgumentNullException.ThrowIfNull(progression);
    ArgumentNullException.ThrowIfNull(eligibility);
    if (progression.IsMeteorScheduled || progression.IsBloodMoon ||
        eligibility.IsPumpkinMoon || eligibility.IsSnowMoon ||
        progression.InvasionType != 0 || eligibility.MoonLordCountdown != 0)
    {
      return false;
    }

    return !HasActiveBoss(eligibility);
  }

  private static bool HasActiveBoss(LanternNightEligibilitySnapshot eligibility)
  {
    for (int index = 0; index < eligibility.Npcs.Count; index++)
    {
      LanternNightNpcSnapshot npc = eligibility.Npcs[index];
      if (!npc.IsActive)
      {
        continue;
      }

      if (npc.IsBoss ||
          npc.TypeId >= FirstLegacyBossTypeId && npc.TypeId <= LastLegacyBossTypeId)
      {
        return true;
      }
    }

    return false;
  }
}
