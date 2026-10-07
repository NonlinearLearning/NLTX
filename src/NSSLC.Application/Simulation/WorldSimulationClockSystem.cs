using System;
using Terraria.WorldSession.Components;

namespace Terraria.NonAuthoritative.Simulation;

public static class WorldSimulationClockSystem
{
  public const int DayLength = 54_000;
  public const int NightLength = 32_400;

  public static void Advance(
    WorldTimeWeatherState state,
    int timeRate,
    WorldEventProgressState? progression = null)
  {
    ArgumentNullException.ThrowIfNull(state);
    ArgumentOutOfRangeException.ThrowIfNegative(timeRate);
    int cycleLength = state.DayTime ? DayLength : NightLength;
    if (!double.IsFinite(state.Time) || state.Time < 0 || state.Time > cycleLength)
    {
      throw new InvalidOperationException("The active world clock has an invalid time value.");
    }

    if (state.MoonPhase is < 0 or > 7)
    {
      throw new InvalidOperationException("The active world clock has an invalid moon phase.");
    }

    if (timeRate == 0)
    {
      state.ClockRevision++;
      return;
    }

    AdvanceCooldowns(state, timeRate);
    AdvanceWeather(state, timeRate);
    if (progression is not null)
    {
      AdvanceEventTimers(progression, timeRate);
    }

    if (state.FastForwardTimeToDawn && !state.DayTime)
    {
      state.FastForwardTimeToDawn = false;
      TransitionToDawn(state);
    }
    else if (state.FastForwardTimeToDusk && state.DayTime)
    {
      state.FastForwardTimeToDusk = false;
      TransitionToDusk(state);
    }

    double remaining = timeRate;
    while (remaining > 0)
    {
      int currentCycleLength = state.DayTime ? DayLength : NightLength;
      if (state.Time >= currentCycleLength)
      {
        CrossCycleBoundary(state);
        continue;
      }

      double advance = Math.Min(remaining, currentCycleLength - state.Time);
      state.Time += advance;
      remaining -= advance;
      if (state.Time >= currentCycleLength)
      {
        CrossCycleBoundary(state);
      }
    }

    state.ClockRevision++;
  }

  private static void CrossCycleBoundary(WorldTimeWeatherState state)
  {
    if (state.DayTime)
    {
      TransitionToDusk(state);
      return;
    }

    TransitionToDawn(state);
  }

  private static void TransitionToDawn(WorldTimeWeatherState state)
  {
    state.DayTime = true;
    state.Time = 0;
    state.MoonPhase = (state.MoonPhase + 1) & 7;
    state.BloodMoon = false;
    state.PumpkinMoon = false;
    state.SnowMoon = false;
    if (state.BirthdayParty.PartyDaysOnCooldown > 0)
    {
      state.BirthdayParty.PartyDaysOnCooldown--;
    }
    if (state.LanternNight.LanternNightsOnCooldown > 0)
    {
      state.LanternNight.LanternNightsOnCooldown--;
    }
  }

  private static void TransitionToDusk(WorldTimeWeatherState state)
  {
    state.DayTime = false;
    state.Time = 0;
    state.Eclipse = false;
  }

  private static void AdvanceCooldowns(WorldTimeWeatherState state, int elapsed)
  {
    state.SundialCooldown = DecreaseTimer(state.SundialCooldown, elapsed);
    state.MoondialCooldown = DecreaseTimer(state.MoondialCooldown, elapsed);
    state.CultistDelay = DecreaseTimer(state.CultistDelay, elapsed);
  }

  private static void AdvanceEventTimers(WorldEventProgressState progression, int elapsed)
  {
    InvasionRuntimeState invasion = progression.Invasion;
    invasion.Delay = DecreaseTimer(invasion.Delay, elapsed);
    invasion.WarningTimer = DecreaseTimer(invasion.WarningTimer, elapsed);
    progression.Invasion = invasion;

    Dd2ProgressState dd2 = progression.Dd2;
    dd2.TimeLeftUntilSpawningBegins = DecreaseTimer(
      dd2.TimeLeftUntilSpawningBegins,
      elapsed);
    progression.Dd2 = dd2;

    LunarProgressState lunar = progression.Lunar;
    lunar.MoonLordCountdown = DecreaseTimer(lunar.MoonLordCountdown, elapsed);
    progression.Lunar = lunar;
  }

  private static int DecreaseTimer(int timer, int elapsed)
  {
    return timer <= 0 ? timer : Math.Max(0, timer - elapsed);
  }

  private static void AdvanceWeather(WorldTimeWeatherState state, int elapsed)
  {
    if (state.Raining && state.RainTime > 0)
    {
      state.RainTime = DecreaseTimer(state.RainTime, elapsed);
      if (state.RainTime == 0)
      {
        state.Raining = false;
        state.RainStrength = 0;
      }
    }

    if (state.SlimeRainTime > 0)
    {
      state.SlimeRainTime = Math.Max(0, state.SlimeRainTime - elapsed);
      if (state.SlimeRainTime == 0)
      {
        state.SlimeRain = false;
        state.SlimeRainKillCount = 0;
        state.SlimeWarningTime = 0;
      }
    }
    else if (state.SlimeWarningTime > 0)
    {
      state.SlimeWarningTime = DecreaseTimer(state.SlimeWarningTime, elapsed);
    }

    if (state.Sandstorm.Happening && state.Sandstorm.TimeLeft > 0)
    {
      state.Sandstorm.TimeLeft = DecreaseTimer(state.Sandstorm.TimeLeft, elapsed);
      if (state.Sandstorm.TimeLeft == 0)
      {
        state.Sandstorm.Happening = false;
        state.Sandstorm.Severity = 0;
        state.Sandstorm.IntendedSeverity = 0;
      }
    }

    state.WindCurrent = MoveTowards(state.WindCurrent, state.WindTarget, 0.01f * elapsed);
  }

  private static float MoveTowards(float current, float target, float maximumDelta)
  {
    if (MathF.Abs(target - current) <= maximumDelta)
    {
      return target;
    }

    return current + MathF.CopySign(maximumDelta, target - current);
  }
}
