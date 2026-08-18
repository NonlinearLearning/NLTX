using System;

namespace Terraria.Dome.Simulation.WorldModel;

public sealed class WorldClock
{
  public const int DefaultDayLengthTicks = 54000;
  public const int DefaultNightLengthTicks = 32400;

  public WorldClock(
    long tickNumber = 0,
    int timeOfDay = 0,
    bool isDayTime = true,
    bool isPaused = false,
    int ticksPerUpdate = 1,
    int dayLengthTicks = DefaultDayLengthTicks,
    int nightLengthTicks = DefaultNightLengthTicks)
  {
    ValidateTickNumber(tickNumber);
    ValidateRate(ticksPerUpdate);
    ValidateLength(dayLengthTicks, nameof(dayLengthTicks));
    ValidateLength(nightLengthTicks, nameof(nightLengthTicks));
    ValidateTimeOfDay(timeOfDay, isDayTime, dayLengthTicks, nightLengthTicks);

    TickNumber = tickNumber;
    TimeOfDay = timeOfDay;
    IsDayTime = isDayTime;
    IsPaused = isPaused;
    TicksPerUpdate = ticksPerUpdate;
    DayLengthTicks = dayLengthTicks;
    NightLengthTicks = nightLengthTicks;
  }

  public int DayLengthTicks { get; }
  public bool IsDayTime { get; private set; }
  public bool IsPaused { get; private set; }
  public int NightLengthTicks { get; }
  public int TicksPerUpdate { get; }
  public long TickNumber { get; private set; }
  public int TimeOfDay { get; private set; }

  public WorldClockSnapshot CreateSnapshot()
  {
    return new WorldClockSnapshot(
      TickNumber,
      TimeOfDay,
      IsDayTime,
      IsPaused,
      TicksPerUpdate,
      DayLengthTicks,
      NightLengthTicks);
  }

  public void Advance()
  {
    if (IsPaused)
    {
      return;
    }

    for (int index = 0; index < TicksPerUpdate; index++)
    {
      TickNumber = checked(TickNumber + 1);
      TimeOfDay++;
      int limit = IsDayTime ? DayLengthTicks : NightLengthTicks;
      if (TimeOfDay >= limit)
      {
        TimeOfDay = 0;
        IsDayTime = !IsDayTime;
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
    TickNumber = snapshot.TickNumber;
    TimeOfDay = snapshot.TimeOfDay;
    IsDayTime = snapshot.IsDayTime;
    IsPaused = snapshot.IsPaused;
  }

  private static void ValidateLength(int length, string parameterName)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length, parameterName);
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
    int timeOfDay,
    bool isDayTime,
    int dayLengthTicks,
    int nightLengthTicks)
  {
    int limit = isDayTime ? dayLengthTicks : nightLengthTicks;
    if (timeOfDay < 0 || timeOfDay >= limit)
    {
      throw new ArgumentOutOfRangeException(nameof(timeOfDay));
    }
  }
}

public readonly record struct WorldClockSnapshot(
  long TickNumber,
  int TimeOfDay,
  bool IsDayTime,
  bool IsPaused,
  int TicksPerUpdate,
  int DayLengthTicks = WorldClock.DefaultDayLengthTicks,
  int NightLengthTicks = WorldClock.DefaultNightLengthTicks);
