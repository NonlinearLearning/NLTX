using System;

namespace Terraria.Dome.Simulation.WorldModel.Systems;

public sealed class NormalEventEligibilitySystem
{
  public bool ShouldBlockNormalEvents(
    bool isLanternNight,
    NormalEventEligibilitySnapshot eligibility)
  {
    ArgumentNullException.ThrowIfNull(eligibility);
    if (!eligibility.IsLunarApocalypseActive &&
        !eligibility.HasActiveLegacyNpc398 &&
        eligibility.MoonLordCountdown == 0)
    {
      return isLanternNight;
    }

    return true;
  }
}
