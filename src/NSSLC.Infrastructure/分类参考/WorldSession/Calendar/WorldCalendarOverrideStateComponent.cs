using System;

namespace Terraria.WorldSession.Calendar;

public sealed class WorldCalendarOverrideStateComponent
{
  public WorldCalendarOverrideStateComponent(
    bool bloodMoon = false,
    bool eclipse = false,
    bool pumpkinMoon = false,
    bool snowMoon = false)
  {
    BloodMoon = bloodMoon;
    Eclipse = eclipse;
    PumpkinMoon = pumpkinMoon;
    SnowMoon = snowMoon;
    Validate();
  }

  public bool BloodMoon { get; internal set; }

  public bool Eclipse { get; internal set; }

  public bool PumpkinMoon { get; internal set; }

  public bool SnowMoon { get; internal set; }

  public void Validate()
  {
    if (PumpkinMoon && SnowMoon)
    {
      throw new ArgumentException(
        "Pumpkin Moon and Snow Moon cannot be active together.",
        nameof(SnowMoon));
    }

    if (BloodMoon && (PumpkinMoon || SnowMoon))
    {
      throw new ArgumentException(
        "Blood Moon cannot be active with a seasonal moon.",
        nameof(BloodMoon));
    }
  }
}
