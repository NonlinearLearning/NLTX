using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldModel;

public sealed class WorldWeatherSystem
{
  public WorldEnvironmentTransitionResult AdvanceWithTransition(
    WorldClockSnapshot clock,
    WorldRuleState rules,
    IReadOnlyList<WorldRainStartCommand> rainRequests,
    IReadOnlyList<WorldWindChangeCommand> windRequests,
    bool isLanternNight = false)
  {
    return AdvanceWithTransition(
      clock,
      clock.TicksPerUpdate,
      rules,
      rainRequests,
      windRequests,
      isLanternNight);
  }

  public WorldEnvironmentTransitionResult AdvanceWithTransition(
    WorldClockSnapshot clock,
    int ticksToAdvance,
    WorldRuleState rules,
    IReadOnlyList<WorldRainStartCommand> rainRequests,
    IReadOnlyList<WorldWindChangeCommand> windRequests,
    bool isLanternNight = false)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(ticksToAdvance);
    WorldRuleState next = Advance(
      clock,
      ticksToAdvance,
      rules,
      rainRequests,
      windRequests,
      isLanternNight);
    bool rainStarted = !rules.IsRaining && next.IsRaining;
    bool rainStopped = rules.IsRaining && !next.IsRaining;
    long sequence = rainStarted
      ? FindFirstValidRainSequence(rainRequests)
      : FindFirstValidWindSequence(windRequests);
    return new WorldEnvironmentTransitionResult(next, new WorldEnvironmentTransition(
      clock.TickNumber,
      rainStarted,
      rainStopped,
      next.RainTimeTicks,
      next.RainStrength,
      next.WindSpeedTarget,
      next.WindSpeedCurrent)
    {
      Sequence = sequence
    });
  }

  private static long FindFirstValidRainSequence(IReadOnlyList<WorldRainStartCommand> requests)
  {
    for (int index = 0; index < requests.Count; index++)
    {
      if (requests[index].IsValid)
      {
        return requests[index].Sequence;
      }
    }

    return -1;
  }

  private static long FindFirstValidWindSequence(IReadOnlyList<WorldWindChangeCommand> requests)
  {
    for (int index = 0; index < requests.Count; index++)
    {
      if (requests[index].IsValid)
      {
        return requests[index].Sequence;
      }
    }

    return -1;
  }

  public WorldRuleState Advance(
    WorldClockSnapshot clock,
    WorldRuleState rules,
    IReadOnlyList<WorldRainStartCommand> rainRequests,
    IReadOnlyList<WorldWindChangeCommand> windRequests,
    bool isLanternNight = false)
  {
    return Advance(
      clock,
      clock.TicksPerUpdate,
      rules,
      rainRequests,
      windRequests,
      isLanternNight);
  }

  private static WorldRuleState Advance(
    WorldClockSnapshot clock,
    int ticksToAdvance,
    WorldRuleState rules,
    IReadOnlyList<WorldRainStartCommand> rainRequests,
    IReadOnlyList<WorldWindChangeCommand> windRequests,
    bool isLanternNight)
  {
    ArgumentNullException.ThrowIfNull(rules);
    ArgumentNullException.ThrowIfNull(rainRequests);
    ArgumentNullException.ThrowIfNull(windRequests);
    WorldRuleState next = rules;
    for (int index = 0; index < windRequests.Count; index++)
    {
      WorldWindChangeCommand request = windRequests[index];
      if (request.IsValid)
      {
        next = next.WithWind(request.TargetSpeed, next.WindSpeedCurrent);
      }
    }

    if (ticksToAdvance > 0)
    {
      next = next.AdvanceWind(ticksToAdvance, next.RainStrength);
    }
    if (isLanternNight)
    {
      return next.WithRain(0, 0.0f);
    }

    if (next.IsRaining)
    {
      return ticksToAdvance > 0 ? next.AdvanceRain(ticksToAdvance) : next;
    }

    for (int index = 0; index < rainRequests.Count; index++)
    {
      WorldRainStartCommand request = rainRequests[index];
      if (request.IsValid)
      {
        return next.WithRain(request.DurationTicks, request.Strength);
      }
    }

    return next;
  }
}

public readonly record struct WorldEnvironmentTransitionResult(
  WorldRuleState State,
  WorldEnvironmentTransition Transition);
