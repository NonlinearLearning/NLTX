namespace MinimalImplementation.Server.World;

public readonly record struct WorldTimeState(bool DayTime, double Time, byte MoonPhase)
{
  public const double DayLength = 54000.0;
  public const double NightLength = 32400.0;
  public const double FullCycleLength = DayLength + NightLength;

  public double CurrentPeriodLength => DayTime ? DayLength : NightLength;
}

public static class WorldTimeSystem
{
  /// <summary>Advances one time update; elapsedGameTicks scales this update, not a catch-up loop.</summary>
  /// <remarks>Matches Main.UpdateTime: a strict period overflow switches once and discards excess.</remarks>
  public static WorldTimeState Advance(WorldTimeState current, double elapsedGameTicks, int dayRate)
  {
    return CalculateAdvance(current, elapsedGameTicks, dayRate).State;
  }

  internal static WorldTimeAdvanceResult CalculateAdvance(
    WorldTimeState current,
    double elapsedGameTicks,
    int dayRate)
  {
    if (!double.IsFinite(current.Time) || current.Time < 0 ||
        current.Time > current.CurrentPeriodLength || current.MoonPhase > 7)
    {
      throw new ArgumentOutOfRangeException(nameof(current));
    }

    if (elapsedGameTicks < 0 || double.IsNaN(elapsedGameTicks) ||
        double.IsInfinity(elapsedGameTicks))
    {
      throw new ArgumentOutOfRangeException(nameof(elapsedGameTicks));
    }

    if (dayRate < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(dayRate));
    }

    if (dayRate == 0 || elapsedGameTicks == 0)
    {
      return new WorldTimeAdvanceResult(current, 0);
    }

    double elapsed = elapsedGameTicks * dayRate;
    if (!double.IsFinite(elapsed))
    {
      throw new ArgumentOutOfRangeException(nameof(elapsedGameTicks));
    }

    double nextTime = current.Time + elapsed;
    if (!double.IsFinite(nextTime))
    {
      throw new ArgumentOutOfRangeException(nameof(elapsedGameTicks));
    }

    if (nextTime <= current.CurrentPeriodLength)
    {
      return new WorldTimeAdvanceResult(
        new WorldTimeState(current.DayTime, nextTime, current.MoonPhase),
        0);
    }

    bool dayTime = !current.DayTime;
    long morningTransitionCount = dayTime ? 1 : 0;
    byte moonPhase = dayTime
      ? (byte)((current.MoonPhase + 1) % 8)
      : current.MoonPhase;
    return new WorldTimeAdvanceResult(
      new WorldTimeState(dayTime, 0, moonPhase),
      morningTransitionCount);
  }
}

internal readonly record struct WorldTimeAdvanceResult(
  WorldTimeState State,
  long MorningTransitionCount);
