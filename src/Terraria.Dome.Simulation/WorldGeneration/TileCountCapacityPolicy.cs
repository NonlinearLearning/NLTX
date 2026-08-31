using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TileCountCapacityPolicy
{
  public static TileCountCapacityDecision Evaluate(int count, int maximum)
  {
    if (count < 0 || maximum <= 0 || count > maximum)
    {
      throw new ArgumentOutOfRangeException(nameof(count));
    }

    bool saturated = count >= maximum;
    return new TileCountCapacityDecision(count, maximum, !saturated, saturated);
  }

  public static TileCountCapacityDecision Saturate(int maximum)
  {
    if (maximum <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maximum));
    }

    return new TileCountCapacityDecision(maximum, maximum, false, true);
  }

  public static TileCountCapacityDecision Increment(
    TileCountCapacityDecision decision)
  {
    if (!decision.CanVisit)
    {
      return decision;
    }

    int nextCount = decision.Count + 1;
    return nextCount >= decision.Maximum
      ? Saturate(decision.Maximum)
      : new TileCountCapacityDecision(nextCount, decision.Maximum, true, false);
  }
}
