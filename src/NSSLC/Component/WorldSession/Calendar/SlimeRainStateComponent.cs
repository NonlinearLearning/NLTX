using System;

namespace Terraria.WorldSession.Calendar;

public sealed class SlimeRainStateComponent
{
  public const int DefaultWarningDelay = 420;

  public SlimeRainStateComponent(
    bool active = false,
    double timeState = 0.0d,
    int killCount = 0,
    int warningTime = 0,
    int warningDelay = DefaultWarningDelay)
  {
    Active = active;
    TimeState = timeState;
    KillCount = killCount;
    WarningTime = warningTime;
    WarningDelay = warningDelay;
    Validate();
  }

  public bool Active;
  public double TimeState;
  public int KillCount;
  public int WarningTime;
  public int WarningDelay;

  public void Validate()
  {
    if (!double.IsFinite(TimeState))
    {
      throw new ArgumentOutOfRangeException(nameof(TimeState));
    }

    ArgumentOutOfRangeException.ThrowIfNegative(KillCount);
    ArgumentOutOfRangeException.ThrowIfNegative(WarningTime);
    ArgumentOutOfRangeException.ThrowIfNegative(WarningDelay);

    if (Active && TimeState <= 0.0d)
    {
      throw new ArgumentException(
        "An active Slime Rain must have a positive active time state.",
        nameof(TimeState));
    }
  }
}
