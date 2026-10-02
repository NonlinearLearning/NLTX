using System;

namespace Terraria.WorldSession.Calendar;

public sealed class CalendarClockComponent
{
  public CalendarClockComponent(
    bool isDayTime = true,
    double timeOfDay = 13500.0d,
    int moonPhase = 0)
  {
    IsDayTime = isDayTime;
    TimeOfDay = timeOfDay;
    MoonPhase = moonPhase;
    Validate();
  }

  public bool IsDayTime { get; internal set; }

  public double TimeOfDay { get; internal set; }

  public int MoonPhase { get; internal set; }

  public void Validate()
  {
    if (!double.IsFinite(TimeOfDay) || TimeOfDay < 0.0d)
    {
      throw new ArgumentOutOfRangeException(nameof(TimeOfDay));
    }

    if (MoonPhase is < 0 or > 7)
    {
      throw new ArgumentOutOfRangeException(nameof(MoonPhase));
    }
  }
}
