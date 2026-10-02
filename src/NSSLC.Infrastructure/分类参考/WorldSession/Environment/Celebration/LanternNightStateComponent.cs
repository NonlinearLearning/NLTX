using System;

namespace Terraria.WorldSession.Environment.Celebration;

public sealed class LanternNightStateComponent
{
  public LanternNightStateComponent(
    bool manualOverride = false,
    bool genuineActive = false,
    bool nextNightRequested = false,
    int nightsOnCooldown = 0)
  {
    ManualOverride = manualOverride;
    GenuineActive = genuineActive;
    NextNightRequested = nextNightRequested;
    NightsOnCooldown = nightsOnCooldown;
    Validate();
  }

  public bool ManualOverride { get; internal set; }

  public bool GenuineActive { get; internal set; }

  public bool NextNightRequested { get; internal set; }

  public int NightsOnCooldown { get; internal set; }

  public void Validate()
  {
    ArgumentOutOfRangeException.ThrowIfNegative(NightsOnCooldown);
  }
}
