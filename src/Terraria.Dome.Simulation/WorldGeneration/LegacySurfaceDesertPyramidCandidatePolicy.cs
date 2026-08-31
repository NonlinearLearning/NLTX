using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct LegacySurfaceDesertPyramidCandidate(
  int X,
  int CenterRetryCount);

public static class LegacySurfaceDesertPyramidCandidatePolicy
{
  private const int CandidateEdgePadding = 300;
  private const int ReferenceWorldWidth = 4200;
  private const int RandomCountMinimum = 5;
  private const int RandomCountMaximumExclusive = 8;
  private const double CenterLowerFraction = 0.47;
  private const double CenterUpperFraction = 0.53;

  public static int CalculateIterationCount(
    int worldWidth,
    LegacyPassRandomState random)
  {
    ArgumentNullException.ThrowIfNull(random);
    ValidateWorldWidth(worldWidth);
    int baseCount = random.Next(RandomCountMinimum, RandomCountMaximumExclusive);
    return checked(baseCount * (worldWidth / ReferenceWorldWidth));
  }

  public static IReadOnlyList<LegacySurfaceDesertPyramidCandidate> SelectCandidates(
    int worldWidth,
    LegacyPassRandomState random)
  {
    ArgumentNullException.ThrowIfNull(random);
    ValidateWorldWidth(worldWidth);
    int count = CalculateIterationCount(worldWidth, random);
    List<LegacySurfaceDesertPyramidCandidate> candidates = new(count);
    for (int index = 0; index < count; index++)
    {
      candidates.Add(SelectCandidate(worldWidth, random));
    }

    return candidates.AsReadOnly();
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

  private static LegacySurfaceDesertPyramidCandidate SelectCandidate(
    int worldWidth,
    LegacyPassRandomState random)
  {
    int minimumX = CandidateEdgePadding;
    int maximumXExclusive = worldWidth - CandidateEdgePadding;
    int centerRetryCount = 0;
    while (true)
    {
      int x = random.Next(minimumX, maximumXExclusive);
      if (!IsCenterRejected(x, worldWidth))
      {
        return new LegacySurfaceDesertPyramidCandidate(x, centerRetryCount);
      }

      centerRetryCount++;
    }
  }

  private static void ValidateWorldWidth(int worldWidth)
  {
    if (worldWidth <= CandidateEdgePadding * 2)
    {
      throw new ArgumentOutOfRangeException(nameof(worldWidth));
    }

    int maximumX = worldWidth - CandidateEdgePadding - 1;
    if (IsCenterRejected(CandidateEdgePadding, worldWidth) &&
        IsCenterRejected(maximumX, worldWidth))
    {
      throw new ArgumentOutOfRangeException(
        nameof(worldWidth),
        "The world width has no selectable surface-desert Pyramid X coordinate.");
    }
  }
}
