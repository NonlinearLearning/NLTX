namespace Terraria.Dome.Simulation.WorldGeneration;

public static class PileGenerationAttemptPolicy
{
  private const int BaseDivisor = 2;
  private const int SkyblockDivisor = 10;

  public static int GetAttempts(int worldWidth, bool isSkyblockWorld)
  {
    int attempts = worldWidth / BaseDivisor;

    if (isSkyblockWorld)
    {
      attempts /= SkyblockDivisor;
    }

    return attempts;
  }
}
