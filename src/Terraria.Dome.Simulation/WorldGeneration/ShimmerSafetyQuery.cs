using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class ShimmerSafetyQuery
{
  public static bool IsWithinSafetyRadius(
    int x,
    int y,
    int shimmerX,
    int shimmerY,
    WorldGenerationDistanceDefaults distances)
  {
    long deltaX = (long)x - shimmerX;
    long deltaY = (long)y - shimmerY;
    long distanceSquared = deltaX * deltaX + deltaY * deltaY;
    long safetyDistance = distances.ShimmerSafetyDistance;
    return distanceSquared < safetyDistance * safetyDistance;
  }
}
