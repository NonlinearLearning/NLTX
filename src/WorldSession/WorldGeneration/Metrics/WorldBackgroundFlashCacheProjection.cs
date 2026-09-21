using System;
using System.Collections.Generic;

namespace Terraria.WorldGeneration.Metrics;

public sealed class WorldBackgroundFlashCacheProjection
{
  public const int BackgroundAreaCount = 13;

  private readonly int[] _variations = new int[BackgroundAreaCount];
  private readonly float[] _flashPower = new float[BackgroundAreaCount];

  public int GetVariation(int areaId)
  {
    ValidateAreaId(areaId);
    return _variations[areaId];
  }

  public float GetFlashPower(int areaId)
  {
    ValidateAreaId(areaId);
    return _flashPower[areaId];
  }

  public void UpdateCache(IReadOnlyList<int> variations, bool isGameMenu)
  {
    ArgumentNullException.ThrowIfNull(variations);
    if (variations.Count != BackgroundAreaCount)
    {
      throw new ArgumentException(
        $"Background cache updates require {BackgroundAreaCount} areas.",
        nameof(variations));
    }

    for (int areaId = 0; areaId < BackgroundAreaCount; areaId++)
    {
      int newVariation = variations[areaId];
      if (_variations[areaId] == newVariation)
      {
        continue;
      }

      _variations[areaId] = newVariation;
      if (!isGameMenu)
      {
        _flashPower[areaId] = 1f;
      }
    }
  }

  public void UpdateFlashValues()
  {
    for (int areaId = 0; areaId < BackgroundAreaCount; areaId++)
    {
      _flashPower[areaId] = Math.Clamp(_flashPower[areaId] - 0.05f, 0f, 1f);
    }
  }

  private static void ValidateAreaId(int areaId)
  {
    if ((uint)areaId >= BackgroundAreaCount)
    {
      throw new ArgumentOutOfRangeException(nameof(areaId));
    }
  }
}
