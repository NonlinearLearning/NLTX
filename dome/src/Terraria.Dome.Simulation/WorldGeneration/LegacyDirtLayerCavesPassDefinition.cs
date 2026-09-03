using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyDirtLayerCavesPassDefinition(
  double Density,
  int LiquidPreservingChanceDenominator,
  int MinimumStrength,
  int MaximumStrengthExclusive,
  int MinimumSteps,
  int MaximumStepsExclusive,
  int SmallHolesBeachAvoidance)
{
  public int CalculateInvocationCount(int width, int height, bool isRemixWorld)
  {
    if (width <= 0 || height <= 0 || !double.IsFinite(Density) || Density < 0.0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    double count = width * (double)height * Density;
    if (isRemixWorld)
    {
      count *= 2.0;
    }

    return checked((int)count);
  }

  public int SelectTileType(LegacyPassRandomState random)
  {
    ArgumentNullException.ThrowIfNull(random);
    return random.Next(LiquidPreservingChanceDenominator) == 0 ? -2 : -1;
  }

  public int ScaleStrength(int strength, bool isRemixWorld)
  {
    if (strength < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(strength));
    }

    return isRemixWorld ? (int)(strength * 1.1) : strength;
  }

  public int ScaleSteps(int steps, bool isRemixWorld)
  {
    if (steps < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(steps));
    }

    return isRemixWorld ? (int)(steps * 1.9) : steps;
  }

  public void Validate()
  {
    if (!double.IsFinite(Density) || Density < 0.0 ||
        LiquidPreservingChanceDenominator <= 0 ||
        MinimumStrength < 0 || MaximumStrengthExclusive <= MinimumStrength ||
        MinimumSteps <= 0 || MaximumStepsExclusive <= MinimumSteps ||
        SmallHolesBeachAvoidance < 0)
    {
      throw new ArgumentException(
        "DirtLayerCaves pass definition contains an invalid source contract.",
        nameof(Density));
    }
  }
}

public static class LegacyDirtLayerCavesPassDefinitionFactory
{
  public static LegacyDirtLayerCavesPassDefinition CreateDefault()
  {
    return new LegacyDirtLayerCavesPassDefinition(
      Density: 3E-05,
      LiquidPreservingChanceDenominator: 6,
      MinimumStrength: 5,
      MaximumStrengthExclusive: 15,
      MinimumSteps: 30,
      MaximumStepsExclusive: 200,
      SmallHolesBeachAvoidance: 340);
  }
}
