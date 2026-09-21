using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class AltarBreakProgressionPolicy
{
  public static AltarBreakProgression Apply(int altarCount)
  {
    if (altarCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(altarCount));
    }

    if (altarCount == int.MaxValue)
    {
      throw new InvalidOperationException("Altar count cannot advance past Int32.MaxValue.");
    }

    return new AltarBreakProgression(
      altarCount,
      altarCount + 1,
      altarCount % 3,
      altarCount / 3 + 1);
  }
}
