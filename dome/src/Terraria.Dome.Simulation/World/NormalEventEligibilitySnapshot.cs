using System;

namespace Terraria.Dome.Simulation.WorldModel;

public sealed class NormalEventEligibilitySnapshot
{
  public NormalEventEligibilitySnapshot(
    bool isLunarApocalypseActive,
    bool hasActiveLegacyNpc398,
    int moonLordCountdown)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(moonLordCountdown);

    IsLunarApocalypseActive = isLunarApocalypseActive;
    HasActiveLegacyNpc398 = hasActiveLegacyNpc398;
    MoonLordCountdown = moonLordCountdown;
  }

  public bool HasActiveLegacyNpc398 { get; }
  public bool IsLunarApocalypseActive { get; }
  public int MoonLordCountdown { get; }
}
