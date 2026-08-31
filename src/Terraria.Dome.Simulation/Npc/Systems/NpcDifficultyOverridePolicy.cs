using System;

namespace Terraria.Dome.Simulation.Npc.Systems;

public static class NpcDifficultyOverridePolicy
{
  public static float Resolve(float baseMultiplier, float? overrideMultiplier)
  {
    if (!float.IsFinite(baseMultiplier) || baseMultiplier <= 0.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(baseMultiplier));
    }

    if (overrideMultiplier is not float value)
    {
      return baseMultiplier;
    }

    if (!float.IsFinite(value) || value <= 0.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(overrideMultiplier));
    }

    return value;
  }
}
