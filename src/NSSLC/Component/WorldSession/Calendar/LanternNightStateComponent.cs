using System;

namespace Terraria.WorldSession.Calendar;

public sealed class LanternNightStateComponent
{
  public LanternNightStateComponent(
    bool manualLanterns = false,
    bool genuineLanterns = false,
    bool nextNightIsLanternNight = false,
    int lanternNightsOnCooldown = 0,
    bool wasLanternNight = false)
  {
    ManualLanterns = manualLanterns;
    GenuineLanterns = genuineLanterns;
    NextNightIsLanternNight = nextNightIsLanternNight;
    LanternNightsOnCooldown = lanternNightsOnCooldown;
    WasLanternNight = wasLanternNight;
    Validate();
  }

  public bool ManualLanterns;
  public bool GenuineLanterns;
  public bool NextNightIsLanternNight;
  public int LanternNightsOnCooldown;
  public bool WasLanternNight;

  public bool IsUp => ManualLanterns || GenuineLanterns;

  public void Validate()
  {
    ArgumentOutOfRangeException.ThrowIfNegative(LanternNightsOnCooldown);
  }
}
