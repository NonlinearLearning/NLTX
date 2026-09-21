using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacySmallHolesPassDefinition(
    double Density,
    int LiquidChanceDenominator,
    int MinimumFirstStrength,
    int MaximumFirstStrengthExclusive,
    int MinimumFirstSteps,
    int MaximumFirstStepsExclusive,
    int MinimumSecondStrength,
    int MaximumSecondStrengthExclusive,
    int MinimumSecondSteps,
    int MaximumSecondStepsExclusive)
{
  public int CalculateIterationCount(int width, int height)
  {
    if (width <= 0 || height <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    return checked((int)(width * (double)height * Density));
  }

  public int SelectTileType(LegacyPassRandomState random)
  {
    ArgumentNullException.ThrowIfNull(random);
    return random.Next(LiquidChanceDenominator) == 0 ? -2 : -1;
  }
}

public static class LegacySmallHolesPassDefinitionFactory
{
  public static LegacySmallHolesPassDefinition CreateDefault()
  {
    return new LegacySmallHolesPassDefinition(
      Density: 0.0015,
      LiquidChanceDenominator: 5,
      MinimumFirstStrength: 2,
      MaximumFirstStrengthExclusive: 5,
      MinimumFirstSteps: 2,
      MaximumFirstStepsExclusive: 20,
      MinimumSecondStrength: 8,
      MaximumSecondStrengthExclusive: 15,
      MinimumSecondSteps: 7,
      MaximumSecondStepsExclusive: 30);
  }
}
