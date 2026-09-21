using System;

namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct LegacyRainsForAYearResult(WorldRuleState Rules, int CloudCount);

public static class LegacyRainsForAYearPolicy
{
  private const int CloudCount = 200;
  private const int RainDurationTicks = 1892160000;
  private const float RainStrength = 1.0f;

  public static LegacyRainsForAYearResult Apply(WorldRuleState rules)
  {
    ArgumentNullException.ThrowIfNull(rules);
    WorldRuleState raining = rules.WithRawRain(isRaining: true, maximumRainStrength: RainStrength)
      .WithRain(RainDurationTicks, RainStrength);
    return new LegacyRainsForAYearResult(raining, CloudCount);
  }
}
