using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct LegacyDunesCandidate(
  int X,
  int Y,
  int AttemptCount);

public static class LegacyDunesCandidatePolicy
{
  private const int RandomWorldPointPadding = 500;
  private const int WorldCenterPadding = 300;
  private const int SnowPadding = 300;
  private const int JungleDistanceAtReferenceWidth = 600;
  private const int ReferenceWorldWidth = 4200;

  public static bool IsLocationRejected(
    int x,
    int worldWidth,
    int jungleOriginX,
    int snowOriginLeft,
    int snowOriginRight,
    int attemptCount)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(worldWidth);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(attemptCount);

    long jungleDistance = Math.Abs((long)x - jungleOriginX);
    int jungleDistanceLimit =
      (int)(JungleDistanceAtReferenceWidth * (double)worldWidth / ReferenceWorldWidth);
    bool nearJungle = jungleDistance < jungleDistanceLimit;
    bool nearCenter = Math.Abs((long)x - worldWidth / 2) < WorldCenterPadding;
    bool nearSnow = x > snowOriginLeft - SnowPadding &&
      x < snowOriginRight + SnowPadding;
    if (attemptCount >= worldWidth)
    {
      nearJungle = false;
    }

    if (attemptCount >= (long)worldWidth * 2)
    {
      nearSnow = false;
    }

    return nearJungle || nearCenter || nearSnow;
  }

  public static LegacyDunesCandidate Select(
    WorldMetadata metadata,
    int jungleOriginX,
    int snowOriginLeft,
    int snowOriginRight,
    LegacyPassRandomState random)
  {
    ArgumentNullException.ThrowIfNull(metadata);
    ArgumentNullException.ThrowIfNull(random);
    if (metadata.Width <= RandomWorldPointPadding * 2)
    {
      throw new ArgumentOutOfRangeException(nameof(metadata));
    }

    int attemptCount = 0;
    while (true)
    {
      int x = random.Next(RandomWorldPointPadding, metadata.Width - RandomWorldPointPadding);
      int y = random.Next(metadata.Height);
      attemptCount++;
      if (!IsLocationRejected(
            x,
            metadata.Width,
            jungleOriginX,
            snowOriginLeft,
            snowOriginRight,
            attemptCount))
      {
        return new LegacyDunesCandidate(x, y, attemptCount);
      }
    }
  }
}
