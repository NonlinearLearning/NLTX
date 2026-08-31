using System;

namespace Terraria.Dome.Simulation.Items.Systems;

public static class MeleeItemScalePolicy
{
  private const float MeleeScaleGloveMultiplier = 1.1f;

  public static float Apply(float itemScale, bool isMelee, bool hasMeleeScaleGlove)
  {
    if (!float.IsFinite(itemScale) || itemScale <= 0.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(itemScale));
    }

    if (isMelee && hasMeleeScaleGlove)
    {
      return itemScale * MeleeScaleGloveMultiplier;
    }

    return itemScale;
  }
}
