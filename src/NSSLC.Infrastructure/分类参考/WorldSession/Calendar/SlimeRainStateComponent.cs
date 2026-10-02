using System;

namespace Terraria.WorldSession.Calendar;

public sealed class SlimeRainStateComponent
{
  public SlimeRainStateComponent(
    bool active = false,
    double timeState = 0.0d,
    int warningRemaining = 0)
  {
    Active = active;
    TimeState = timeState;
    WarningRemaining = warningRemaining;
    Validate();
  }

  public bool Active { get; internal set; }

  public double TimeState { get; internal set; }

  public int WarningRemaining { get; internal set; }

  public void Validate()
  {
    if (!double.IsFinite(TimeState))
    {
      throw new ArgumentOutOfRangeException(nameof(TimeState));
    }

    ArgumentOutOfRangeException.ThrowIfNegative(WarningRemaining);

    if (Active && TimeState <= 0.0d)
    {
      throw new ArgumentException(
        "An active Slime Rain must have a positive time state.",
        nameof(TimeState));
    }
  }
}
