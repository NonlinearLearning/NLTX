using System;

namespace Terraria.Dome.Simulation.Combat.Systems;

public static class PlayerMiscCounterPolicy
{
  public static int Advance(int miscCounter)
  {
    if (miscCounter < 0 || miscCounter >= PlayerLuckCalculationPolicy.MiscCounterPeriod)
    {
      throw new ArgumentOutOfRangeException(nameof(miscCounter));
    }

    int nextCounter = miscCounter + 1;
    if (nextCounter >= PlayerLuckCalculationPolicy.MiscCounterPeriod)
    {
      nextCounter = 0;
    }

    return nextCounter;
  }
}
