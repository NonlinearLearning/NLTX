using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct LegacyUnderworldSurfaceColumn(int TopY, int EffectiveTopY);

public static class LegacyUnderworldSurfaceColumnPolicy
{
  public static int CreateInitialTop(int height, LegacyPassRandomState random)
  {
    ArgumentNullException.ThrowIfNull(random);
    if (height <= 190)
    {
      throw new ArgumentOutOfRangeException(nameof(height));
    }

    return height - random.Next(150, 190);
  }

  public static LegacyUnderworldSurfaceColumn Advance(
    int previousTopY,
    int height,
    LegacyPassRandomState random,
    bool isNotTheBeesWorld)
  {
    ArgumentNullException.ThrowIfNull(random);
    if (previousTopY < 0 || height <= 190 || previousTopY >= height)
    {
      throw new ArgumentOutOfRangeException(nameof(previousTopY));
    }

    int topY = previousTopY + random.Next(-3, 4);
    topY = Math.Clamp(topY, height - 190, height - 160);
    int effectiveTopY = isNotTheBeesWorld ? topY - 30 : topY;
    return new LegacyUnderworldSurfaceColumn(topY, effectiveTopY);
  }
}
