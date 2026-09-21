namespace Terraria.Dome.Simulation.WorldGeneration;

public static class SmallConsecutiveClumpMetricsPolicy
{
  public static SmallConsecutiveClumpMetrics Record(
    SmallConsecutiveClumpMetrics current,
    int clumpLength,
    int tileCounterValue,
    int tileCounterMax)
  {
    if (clumpLength <= 0 || clumpLength >= tileCounterMax)
    {
      return current;
    }

    int eliminatedCount = current.EliminatedCount;
    if (tileCounterValue < tileCounterMax)
    {
      eliminatedCount++;
    }

    return new SmallConsecutiveClumpMetrics(current.FoundCount + 1, eliminatedCount);
  }
}
