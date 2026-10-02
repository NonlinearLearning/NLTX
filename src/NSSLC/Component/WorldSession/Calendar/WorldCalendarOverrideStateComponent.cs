using System;

namespace Terraria.WorldSession.Calendar;

public sealed class WorldCalendarOverrideStateComponent
{
  public WorldCalendarOverrideStateComponent(
    bool bloodMoon = false,
    bool eclipse = false,
    bool pumpkinMoon = false,
    bool snowMoon = false,
    bool fastForwardTimeToDawn = false,
    bool fastForwardTimeToDusk = false,
    int sundialCooldown = 0,
    int moondialCooldown = 0)
  {
    BloodMoon = bloodMoon;
    Eclipse = eclipse;
    PumpkinMoon = pumpkinMoon;
    SnowMoon = snowMoon;
    FastForwardTimeToDawn = fastForwardTimeToDawn;
    FastForwardTimeToDusk = fastForwardTimeToDusk;
    SundialCooldown = sundialCooldown;
    MoondialCooldown = moondialCooldown;
    Validate();
  }

  public bool BloodMoon;
  public bool Eclipse;
  public bool PumpkinMoon;
  public bool SnowMoon;
  public bool FastForwardTimeToDawn;
  public bool FastForwardTimeToDusk;
  public int SundialCooldown;
  public int MoondialCooldown;

  public void Validate()
  {
    ArgumentOutOfRangeException.ThrowIfNegative(SundialCooldown);
    ArgumentOutOfRangeException.ThrowIfNegative(MoondialCooldown);

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
