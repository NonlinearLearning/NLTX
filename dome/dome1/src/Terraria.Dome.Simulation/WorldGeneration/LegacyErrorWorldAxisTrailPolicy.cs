using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyErrorWorldAxisTrailPolicy
{
  public static LegacyErrorWorldAxisTrailPlan Create(LegacyPassRandomState random)
  {
    ArgumentNullException.ThrowIfNull(random);
    LegacyErrorWorldAxisDirection direction;
    if (random.Next(2) == 0)
    {
      direction = random.Next(2) == 0
        ? new LegacyErrorWorldAxisDirection(-1, 0)
        : new LegacyErrorWorldAxisDirection(1, 0);
    }
    else
    {
      direction = random.Next(2) == 0
        ? new LegacyErrorWorldAxisDirection(0, -1)
        : new LegacyErrorWorldAxisDirection(0, 1);
    }

    return new LegacyErrorWorldAxisTrailPlan(direction, random.Next(5, 21));
  }
}
