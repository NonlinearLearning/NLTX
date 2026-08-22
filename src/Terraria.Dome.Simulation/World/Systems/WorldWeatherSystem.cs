using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldModel;

public sealed class WorldWeatherSystem
{
  public WorldRuleState Advance(
    WorldClockSnapshot clock,
    WorldRuleState rules,
    IReadOnlyList<WorldRainStartCommand> rainRequests,
    IReadOnlyList<WorldWindChangeCommand> windRequests,
    bool isLanternNight = false)
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

    next = next.AdvanceWind(clock.TicksPerUpdate, next.RainStrength);
    if (isLanternNight)
    {
      return next.WithRain(0, 0.0f);
    }

    if (next.IsRaining)
    {
      return next.AdvanceRain(clock.TicksPerUpdate);
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
