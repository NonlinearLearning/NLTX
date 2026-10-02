using System;

namespace Terraria.WorldSession.Calendar;

public sealed class CalendarClockComponent
{
  public const int DefaultDayLengthTicks = 54000;
  public const int DefaultNightLengthTicks = 32400;

  public CalendarClockComponent(
    long tickNumber,
    double timeOfDay,
    bool isDayTime = true,
    int dayRate = 1,
    int dayLengthTicks = DefaultDayLengthTicks,
    int nightLengthTicks = DefaultNightLengthTicks,
    byte moonPhase = 0,
    bool isPaused = false)
  {
    TickNumber = tickNumber;
    TimeOfDay = timeOfDay;
    IsDayTime = isDayTime;
    DayRate = dayRate;
    DayLengthTicks = dayLengthTicks;
    NightLengthTicks = nightLengthTicks;
    MoonPhase = moonPhase;
    IsPaused = isPaused;
    Validate();
  }

  public long TickNumber;
  public double TimeOfDay;
  public bool IsDayTime;
  public int DayRate;
  public int DayLengthTicks;
  public int NightLengthTicks;
  public byte MoonPhase;
  public bool IsPaused;

  public int CurrentCycleLength => IsDayTime ? DayLengthTicks : NightLengthTicks;

  public void Validate()
  {
    ArgumentOutOfRangeException.ThrowIfNegative(TickNumber);
    ArgumentOutOfRangeException.ThrowIfNegative(DayRate);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(DayLengthTicks);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(NightLengthTicks);

    if (!double.IsFinite(TimeOfDay) ||
        TimeOfDay < 0.0d ||
        TimeOfDay >= CurrentCycleLength)
    {
      throw new ArgumentOutOfRangeException(nameof(TimeOfDay));
    }

    if (MoonPhase > 7)
    {
      throw new ArgumentOutOfRangeException(nameof(MoonPhase));
    }
  }
}
