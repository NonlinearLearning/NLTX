using System;

namespace Terraria.Dome.Simulation.WorldModel;

public sealed class WorldClock
{
  public const int DefaultDayLengthTicks = 54000;
  public const int DefaultNightLengthTicks = 32400;

  public WorldClock(
    long tickNumber = 0,
    double timeOfDay = 0,
    bool isDayTime = true,
    bool isPaused = false,
    int ticksPerUpdate = 1,
    int dayLengthTicks = DefaultDayLengthTicks,
    int nightLengthTicks = DefaultNightLengthTicks,
    byte moonPhase = 0)
  {
    ValidateTickNumber(tickNumber);
    ValidateRate(ticksPerUpdate);
    ValidateLength(dayLengthTicks, nameof(dayLengthTicks));
    ValidateLength(nightLengthTicks, nameof(nightLengthTicks));
    ValidateTimeOfDay(timeOfDay, isDayTime, dayLengthTicks, nightLengthTicks);
    ValidateMoonPhase(moonPhase);

    TickNumber = tickNumber;
    TimeOfDay = timeOfDay;
    IsDayTime = isDayTime;
    IsPaused = isPaused;
    TicksPerUpdate = ticksPerUpdate;
    DayLengthTicks = dayLengthTicks;
    NightLengthTicks = nightLengthTicks;
    MoonPhase = moonPhase;
  }

  public int DayLengthTicks { get; }
  public bool IsDayTime { get; private set; }
  public bool IsPaused { get; private set; }
  public int NightLengthTicks { get; }
  public byte MoonPhase { get; private set; }
  public int TicksPerUpdate { get; }
  public long TickNumber { get; private set; }
  public double TimeOfDay { get; private set; }

  public WorldClockSnapshot CreateSnapshot()
  {
    return new WorldClockSnapshot(
      TickNumber,
      TimeOfDay,
      IsDayTime,
      IsPaused,
      TicksPerUpdate,
      DayLengthTicks,
      NightLengthTicks,
      MoonPhase);
  }

  public void Advance()
  {
    Advance(TicksPerUpdate);
  }

  public void Advance(int ticksToAdvance)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(ticksToAdvance);
    if (IsPaused)
    {
      return;
    }

    for (int index = 0; index < ticksToAdvance; index++)
    {
      TickNumber = checked(TickNumber + 1);
      TimeOfDay += 1.0d;
      int limit = IsDayTime ? DayLengthTicks : NightLengthTicks;
      if (TimeOfDay >= limit)
      {
        TimeOfDay = 0.0d;
        IsDayTime = !IsDayTime;
        if (IsDayTime)
        {
          MoonPhase = (byte)((MoonPhase + 1) % 8);
        }
      }
    }
  }

  public void SetPaused(bool isPaused)
  {
    IsPaused = isPaused;
  }

  public void Restore(WorldClockSnapshot snapshot)
  {
    ValidateTickNumber(snapshot.TickNumber);
    ValidateRate(snapshot.TicksPerUpdate);
    if (snapshot.DayLengthTicks != DayLengthTicks ||
        snapshot.NightLengthTicks != NightLengthTicks)
    {
      throw new ArgumentException(
        "The clock snapshot uses incompatible cycle lengths.",
        nameof(snapshot));
    }

    ValidateTimeOfDay(
      snapshot.TimeOfDay,
      snapshot.IsDayTime,
      DayLengthTicks,
      NightLengthTicks);
    ValidateMoonPhase(snapshot.MoonPhase);
    TickNumber = snapshot.TickNumber;
    TimeOfDay = snapshot.TimeOfDay;
    IsDayTime = snapshot.IsDayTime;
    IsPaused = snapshot.IsPaused;
    MoonPhase = snapshot.MoonPhase;
  }

  private static void ValidateLength(int length, string parameterName)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length, parameterName);
  }

  private static void ValidateMoonPhase(byte moonPhase)
  {
    if (moonPhase > 7)
    {
      throw new ArgumentOutOfRangeException(nameof(moonPhase));
    }
  }

  private static void ValidateRate(int ticksPerUpdate)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ticksPerUpdate);
  }

  private static void ValidateTickNumber(long tickNumber)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(tickNumber);
  }

  private static void ValidateTimeOfDay(
    double timeOfDay,
    bool isDayTime,
    int dayLengthTicks,
    int nightLengthTicks)
  {
    int limit = isDayTime ? dayLengthTicks : nightLengthTicks;
    if (!double.IsFinite(timeOfDay) || timeOfDay < 0.0d || timeOfDay >= limit)
    {
      throw new ArgumentOutOfRangeException(nameof(timeOfDay));
    }
  }
}

public readonly record struct WorldClockSnapshot(
  long TickNumber,
  double TimeOfDay,
  bool IsDayTime,
  bool IsPaused,
  int TicksPerUpdate,
  int DayLengthTicks = WorldClock.DefaultDayLengthTicks,
  int NightLengthTicks = WorldClock.DefaultNightLengthTicks,
  byte MoonPhase = 0);
