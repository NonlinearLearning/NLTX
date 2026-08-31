using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyWebsPassDefinition(
  double Density,
  int MinimumXInset,
  int MinimumStrength,
  int MaximumStrengthExclusive,
  int MinimumSteps,
  int MaximumStepsExclusive,
  int TileType,
  double SpeedY)
{
  public int CalculateIterationCount(int width, int height)
  {
    if (width <= 0 || height <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    return checked((int)(width * (double)height * Density));
  }
}

public static class LegacyWebsPassDefinitionFactory
{
  public static LegacyWebsPassDefinition CreateDefault()
  {
    return new LegacyWebsPassDefinition(
      Density: 0.0006,
      MinimumXInset: 20,
      MinimumStrength: 4,
      MaximumStrengthExclusive: 11,
      MinimumSteps: 2,
      MaximumStepsExclusive: 4,
      TileType: 51,
      SpeedY: -1.0);
  }
}
