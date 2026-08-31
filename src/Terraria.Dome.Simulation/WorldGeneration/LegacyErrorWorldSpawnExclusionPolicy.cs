using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyErrorWorldSpawnExclusionPolicy
{
  private const int RectangleHalfWidth = 50;
  private const int SingleTileHalfWidth = 30;

  public static bool IsSingleTileExcluded(
    LegacyErrorWorldSpawnExclusionProfile profile,
    int x,
    int y)
  {
    ValidateProfile(profile);
    int center = profile.WorldWidth / 2;
    return x > center - SingleTileHalfWidth && x < center + SingleTileHalfWidth &&
      IsExcludedAtY(profile, y);
  }

  public static bool IsRectangleExcluded(
    LegacyErrorWorldSpawnExclusionProfile profile,
    int x,
    int y,
    int width)
  {
    ValidateProfile(profile);
    if (width <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    int center = profile.WorldWidth / 2;
    return x + width / 2 > center - RectangleHalfWidth &&
      x < center + RectangleHalfWidth && IsExcludedAtY(profile, y);
  }

  private static bool IsExcludedAtY(LegacyErrorWorldSpawnExclusionProfile profile, int y)
  {
    return profile.IsRemixWorld ? y > profile.UnderworldLayer : y < profile.WorldSurface;
  }

  private static void ValidateProfile(LegacyErrorWorldSpawnExclusionProfile profile)
  {
    if (profile.WorldWidth <= 0 || profile.UnderworldLayer < 0 ||
        !double.IsFinite(profile.WorldSurface))
    {
      throw new ArgumentOutOfRangeException(nameof(profile));
    }
  }
}
