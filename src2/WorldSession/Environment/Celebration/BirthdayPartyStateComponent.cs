using System;

namespace Terraria.WorldSession.Environment.Celebration;

public sealed class BirthdayPartyStateComponent
{
  public BirthdayPartyStateComponent(
    bool manualOverride = false,
    bool genuineActive = false,
    int daysOnCooldown = 0)
  {
    ManualOverride = manualOverride;
    GenuineActive = genuineActive;
    DaysOnCooldown = daysOnCooldown;
    Validate();
  }

  public bool ManualOverride { get; internal set; }

  public bool GenuineActive { get; internal set; }

  public int DaysOnCooldown { get; internal set; }

  public void Validate()
  {
    ArgumentOutOfRangeException.ThrowIfNegative(DaysOnCooldown);
  }
}
