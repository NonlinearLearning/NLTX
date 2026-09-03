using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class DungeonRoomBoundingRadiusQuery
{
  public static int Regular(int overrideInnerBoundsSize, int overrideOuterBoundsSize)
  {
    ValidateNonNegative(overrideInnerBoundsSize, nameof(overrideInnerBoundsSize));
    ValidateNonNegative(overrideOuterBoundsSize, nameof(overrideOuterBoundsSize));
    int total = checked(overrideInnerBoundsSize + overrideOuterBoundsSize);
    return checked(total * 142 / 100);
  }

  public static int LivingTree(int boundingRadius)
  {
    ValidateNonNegative(boundingRadius, nameof(boundingRadius));
    return boundingRadius;
  }

  public static int Wormlike(int firstSideIterations, int secondSideIterations)
  {
    ValidateNonNegative(firstSideIterations, nameof(firstSideIterations));
    ValidateNonNegative(secondSideIterations, nameof(secondSideIterations));
    int iterations = checked(firstSideIterations + secondSideIterations);
    double radius = (16.200000000000003 + iterations * 0.5 * 1.4) * 0.5;
    return checked((int)radius);
  }

  public static int StepBased(int overrideStrength, int overrideSteps)
  {
    ValidateNonNegative(overrideStrength, nameof(overrideStrength));
    ValidateNonNegative(overrideSteps, nameof(overrideSteps));
    double radius = overrideStrength * 0.8 + 5.0 + overrideSteps * 0.5 * 1.4;
    return checked((int)radius);
  }

  private static void ValidateNonNegative(int value, string parameterName)
  {
    if (value < 0)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
