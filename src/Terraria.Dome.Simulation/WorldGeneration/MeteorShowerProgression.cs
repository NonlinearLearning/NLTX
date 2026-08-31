using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct MeteorShowerProgression
{
  public MeteorShowerProgression(int remainingCount)
  {
    if (remainingCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(remainingCount));
    }

    RemainingCount = remainingCount;
  }

  public int RemainingCount { get; }
}
