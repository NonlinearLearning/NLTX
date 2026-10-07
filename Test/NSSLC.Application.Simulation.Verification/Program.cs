using Terraria.NonAuthoritative.Simulation;
using Terraria.WorldSession.Components;

VerifyPausedClockPreservesWorldTime();
VerifyTickAndTimerRates();
VerifyDayAndNightBoundaries();
VerifyFastForwardCrossesMultipleBoundaries();
Console.WriteLine("PASS: world simulation clock verification");

static void VerifyPausedClockPreservesWorldTime()
{
  WorldTimeWeatherState state = CreateTimedState();
  state.FastForwardTimeToDawn = true;
  WorldEventProgressState progression = CreateProgression();

  WorldSimulationClockSystem.Advance(state, timeRate: 0, progression);

  Require(state.Time == 100, "A paused world clock changed time of day.");
  Require(state.DayTime, "A paused world clock changed the day/night state.");
  Require(state.MoonPhase == 7, "A paused world clock changed the moon phase.");
  Require(state.ClockRevision == 1, "A paused committed tick did not advance the clock revision.");
  Require(state.FastForwardTimeToDawn, "A paused clock consumed a pending fast-forward request.");
  Require(state.SundialCooldown == 3 && state.MoondialCooldown == 4 && state.CultistDelay == 5,
    "A paused clock changed a world cooldown.");
  Require(state.Raining && state.RainTime == 2 && state.RainStrength == 0.5f,
    "A paused clock changed rain state.");
  Require(state.SlimeRain && state.SlimeRainTime == 2 && state.SlimeRainKillCount == 9 &&
          state.SlimeWarningTime == 8,
    "A paused clock changed slime-rain state.");
  Require(state.Sandstorm.Happening && state.Sandstorm.TimeLeft == 2 &&
          state.Sandstorm.Severity == 0.7f && state.Sandstorm.IntendedSeverity == 0.8f,
    "A paused clock changed sandstorm state.");
  Require(state.WindCurrent == 0.2f, "A paused clock changed current wind.");
  Require(progression.Invasion.Delay == 3 && progression.Invasion.WarningTimer == 4 &&
          progression.Dd2.TimeLeftUntilSpawningBegins == 5 &&
          progression.Lunar.MoonLordCountdown == 6,
    "A paused clock changed event timers.");
}

static void VerifyTickAndTimerRates()
{
  WorldTimeWeatherState state = CreateTimedState();
  WorldEventProgressState progression = CreateProgression();

  WorldSimulationClockSystem.Advance(state, timeRate: 1, progression);

  Require(state.Time == 101 && state.ClockRevision == 1,
    "A rate-1 tick did not advance the clock by one unit.");
  Require(state.SundialCooldown == 2 && state.MoondialCooldown == 3 && state.CultistDelay == 4,
    "A rate-1 tick did not advance world cooldowns by one unit.");
  Require(state.Raining && state.RainTime == 1, "A rate-1 tick did not advance the rain timer.");
  Require(state.SlimeRain && state.SlimeRainTime == 1 && state.SlimeWarningTime == 8,
    "A rate-1 tick did not advance the slime-rain timer.");
  Require(state.Sandstorm.Happening && state.Sandstorm.TimeLeft == 1,
    "A rate-1 tick did not advance the sandstorm timer.");
  Require(MathF.Abs(state.WindCurrent - 0.21f) < 0.0001f,
    "A rate-1 tick did not advance wind by one weather step.");
  Require(progression.Invasion.Delay == 2 && progression.Invasion.WarningTimer == 3 &&
          progression.Dd2.TimeLeftUntilSpawningBegins == 4 &&
          progression.Lunar.MoonLordCountdown == 5,
    "A rate-1 tick did not advance event timers by one unit.");

  WorldTimeWeatherState expiredState = CreateTimedState();
  WorldEventProgressState expiredProgression = CreateProgression();
  WorldSimulationClockSystem.Advance(expiredState, timeRate: 3, expiredProgression);
  Require(!expiredState.Raining && expiredState.RainTime == 0 && expiredState.RainStrength == 0,
    "Rain did not expire when a fast tick passed its remaining timer.");
  Require(!expiredState.SlimeRain && expiredState.SlimeRainTime == 0 &&
          expiredState.SlimeRainKillCount == 0 && expiredState.SlimeWarningTime == 0,
    "Slime rain did not expire when a fast tick passed its remaining timer.");
  Require(!expiredState.Sandstorm.Happening && expiredState.Sandstorm.TimeLeft == 0 &&
          expiredState.Sandstorm.Severity == 0 && expiredState.Sandstorm.IntendedSeverity == 0,
    "A sandstorm did not expire when a fast tick passed its remaining timer.");
  Require(expiredProgression.Invasion.Delay == 0 && expiredProgression.Invasion.WarningTimer == 1 &&
          expiredProgression.Dd2.TimeLeftUntilSpawningBegins == 2 &&
          expiredProgression.Lunar.MoonLordCountdown == 3,
    "A fast tick did not reduce event timers by its configured rate.");
}

static void VerifyDayAndNightBoundaries()
{
  var dusk = new WorldTimeWeatherState
  {
    DayTime = true,
    Time = WorldSimulationClockSystem.DayLength - 1,
    MoonPhase = 2,
    Eclipse = true,
  };
  WorldSimulationClockSystem.Advance(dusk, timeRate: 1);
  Require(!dusk.DayTime && dusk.Time == 0 && !dusk.Eclipse && dusk.MoonPhase == 2,
    "The final daytime tick did not commit the dusk boundary.");

  var dawn = new WorldTimeWeatherState
  {
    DayTime = false,
    Time = WorldSimulationClockSystem.NightLength - 1,
    MoonPhase = 7,
    BloodMoon = true,
    BirthdayParty = new BirthdayPartyState { PartyDaysOnCooldown = 2 },
    LanternNight = new LanternNightState { LanternNightsOnCooldown = 3 },
  };
  WorldSimulationClockSystem.Advance(dawn, timeRate: 1);
  Require(dawn.DayTime && dawn.Time == 0 && dawn.MoonPhase == 0 && !dawn.BloodMoon,
    "The final nighttime tick did not commit the dawn boundary.");
  Require(dawn.BirthdayParty.PartyDaysOnCooldown == 1 &&
          dawn.LanternNight.LanternNightsOnCooldown == 2,
    "Dawn did not advance day-based event cooldowns exactly once.");
}

static void VerifyFastForwardCrossesMultipleBoundaries()
{
  var state = new WorldTimeWeatherState
  {
    DayTime = true,
    Time = WorldSimulationClockSystem.DayLength - 1,
    MoonPhase = 7,
    BirthdayParty = new BirthdayPartyState { PartyDaysOnCooldown = 2 },
  };

  WorldSimulationClockSystem.Advance(state, timeRate: 87_001);

  Require(!state.DayTime && state.Time == 600 && state.MoonPhase == 0,
    "A fast-forward tick did not cross multiple boundaries with the remaining time preserved.");
  Require(state.BirthdayParty.PartyDaysOnCooldown == 1,
    "A fast-forward tick crossing dawn did not advance the day cooldown once.");
}

static WorldTimeWeatherState CreateTimedState()
{
  return new WorldTimeWeatherState
  {
    DayTime = true,
    Time = 100,
    MoonPhase = 7,
    SundialCooldown = 3,
    MoondialCooldown = 4,
    CultistDelay = 5,
    Raining = true,
    RainTime = 2,
    RainStrength = 0.5f,
    SlimeRain = true,
    SlimeRainTime = 2,
    SlimeRainKillCount = 9,
    SlimeWarningTime = 8,
    WindCurrent = 0.2f,
    WindTarget = 1f,
    Sandstorm = new SandstormState
    {
      Happening = true,
      TimeLeft = 2,
      Severity = 0.7f,
      IntendedSeverity = 0.8f,
    },
  };
}

static WorldEventProgressState CreateProgression()
{
  return new WorldEventProgressState
  {
    Invasion = new InvasionRuntimeState { Delay = 3, WarningTimer = 4 },
    Dd2 = new Dd2ProgressState { TimeLeftUntilSpawningBegins = 5 },
    Lunar = new LunarProgressState { MoonLordCountdown = 6 },
  };
}

static void Require(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}
