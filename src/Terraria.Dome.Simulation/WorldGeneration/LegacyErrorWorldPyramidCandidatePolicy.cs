using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct LegacyErrorWorldPyramidCandidate(
  int X,
  int Y,
  int RetryCount);

public static class LegacyErrorWorldPyramidCandidatePolicy
{
  private const int CandidateEdgePadding = 300;
  private const int ReferenceWorldWidth = 4200;
  private const int RandomCountMinimum = 5;
  private const int RandomCountMaximumExclusive = 8;
  private const int ShimmerSafetyRadius = 300;
  private const double CenterLowerFraction = 0.47;
  private const double CenterUpperFraction = 0.53;

  public static int CalculateIterationCount(
    int worldWidth,
    int errorWorldAdjustment,
    LegacyPassRandomState random)
  {
    ArgumentNullException.ThrowIfNull(random);
    ValidateInputs(worldWidth, errorWorldAdjustment, 0, 1);
    int baseCount = random.Next(RandomCountMinimum, RandomCountMaximumExclusive);
    int scaledCount = checked(baseCount * (worldWidth / ReferenceWorldWidth));
    return scaledCount / errorWorldAdjustment;
  }

  public static IReadOnlyList<LegacyErrorWorldPyramidCandidate> SelectCandidates(
    int worldWidth,
    int minimumY,
    int maximumYExclusive,
    double shimmerX,
    double shimmerY,
    int errorWorldAdjustment,
    LegacyPassRandomState random)
  {
    ArgumentNullException.ThrowIfNull(random);
    ValidateInputs(
      worldWidth,
      errorWorldAdjustment,
      minimumY,
      maximumYExclusive);
    ValidateShimmerPosition(shimmerX, shimmerY);
    int count = CalculateIterationCount(worldWidth, errorWorldAdjustment, random);
    if (count == 0)
    {
      return Array.Empty<LegacyErrorWorldPyramidCandidate>();
    }

    if (!HasSelectableCoordinate(
          worldWidth,
          minimumY,
          maximumYExclusive,
          shimmerX,
          shimmerY))
    {
      throw new ArgumentOutOfRangeException(
        nameof(worldWidth),
        "The world dimensions and shimmer position have no selectable Pyramid coordinate.");
    }

    List<LegacyErrorWorldPyramidCandidate> candidates = new(count);
    for (int index = 0; index < count; index++)
    {
      candidates.Add(SelectCandidate(
        worldWidth,
        minimumY,
        maximumYExclusive,
        shimmerX,
        shimmerY,
        random));
    }

    return candidates.AsReadOnly();
  }

  public static bool IsLocationRejected(
    int x,
    int y,
    int worldWidth,
    double shimmerX,
    double shimmerY)
  {
    if (worldWidth <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldWidth));
    }

    ValidateShimmerPosition(shimmerX, shimmerY);

    bool nearCenter = IsCenterRejected(x, worldWidth);
    double deltaX = (double)x - shimmerX;
    double deltaY = (double)y - shimmerY;
    double distanceSquared = deltaX * deltaX + deltaY * deltaY;
    return nearCenter || distanceSquared < ShimmerSafetyRadius * ShimmerSafetyRadius;
  }

  public static bool IsCenterRejected(int x, int worldWidth)
  {
    if (worldWidth <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldWidth));
    }

    return (double)x > worldWidth * CenterLowerFraction &&
      (double)x < worldWidth * CenterUpperFraction;
  }

  private static LegacyErrorWorldPyramidCandidate SelectCandidate(
    int worldWidth,
    int minimumY,
    int maximumYExclusive,
    double shimmerX,
    double shimmerY,
    LegacyPassRandomState random)
  {
    int minimumX = CandidateEdgePadding;
    int maximumXExclusive = worldWidth - CandidateEdgePadding;
    int retryCount = 0;
    while (true)
    {
      int x = random.Next(minimumX, maximumXExclusive);
      int y = random.Next(minimumY, maximumYExclusive);
      if (!IsLocationRejected(x, y, worldWidth, shimmerX, shimmerY))
      {
        return new LegacyErrorWorldPyramidCandidate(x, y, retryCount);
      }

      retryCount++;
    }
  }

  private static bool HasSelectableCoordinate(
    int worldWidth,
    int minimumY,
    int maximumYExclusive,
    double shimmerX,
    double shimmerY)
  {
    int minimumX = CandidateEdgePadding;
    int maximumXExclusive = worldWidth - CandidateEdgePadding;
    for (int x = minimumX; x < maximumXExclusive; x++)
    {
      for (int y = minimumY; y < maximumYExclusive; y++)
      {
        if (!IsLocationRejected(x, y, worldWidth, shimmerX, shimmerY))
        {
          return true;
        }
      }
    }

    return false;
  }

  private static void ValidateInputs(
    int worldWidth,
    int errorWorldAdjustment,
    int minimumY,
    int maximumYExclusive)
  {
    if (worldWidth <= CandidateEdgePadding * 2)
    {
      throw new ArgumentOutOfRangeException(nameof(worldWidth));
    }

    if (errorWorldAdjustment <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(errorWorldAdjustment));
    }

    if (minimumY < 0 || maximumYExclusive <= minimumY)
    {
      throw new ArgumentOutOfRangeException(nameof(minimumY));
    }
  }

  private static void ValidateShimmerPosition(double shimmerX, double shimmerY)
  {
    if (!double.IsFinite(shimmerX) || !double.IsFinite(shimmerY))
    {
      throw new ArgumentOutOfRangeException(nameof(shimmerX));
    }
  }
}
