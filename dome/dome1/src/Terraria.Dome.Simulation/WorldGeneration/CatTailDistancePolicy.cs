using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class CatTailDistancePolicy
{
  public const int DefaultDistance = 8;

  public static CatTailDistanceDecision Evaluate(int distance, int maximumDistance = DefaultDistance)
  {
    if (distance < 0 || maximumDistance < 2)
    {
      throw new ArgumentOutOfRangeException(nameof(distance));
    }

    return new CatTailDistanceDecision(
      distance,
      maximumDistance - 1,
      2,
      distance >= 2 && distance < maximumDistance,
      distance > maximumDistance);
  }
}
