using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class ShadowOrbBreakProgressionPolicy
{
  private const int BossSpawnThreshold = 3;

  public static ShadowOrbBreakProgression Apply(
    int shadowOrbCount,
    bool dontStarveWorld,
    bool getGoodWorld,
    bool remixWorld)
  {
    if (shadowOrbCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(shadowOrbCount));
    }

    if (shadowOrbCount == int.MaxValue)
    {
      throw new InvalidOperationException("Shadow orb count cannot advance past Int32.MaxValue.");
    }

    int nextCount = shadowOrbCount + 1;
    bool shouldAttemptBossSpawn = nextCount >= BossSpawnThreshold ||
      (dontStarveWorld && getGoodWorld && !remixWorld);
    return new ShadowOrbBreakProgression(true, nextCount, shouldAttemptBossSpawn);
  }
}
