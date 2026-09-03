using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyRockLayerCavesPassDefinition(
  double Density,
  int LiquidPreservingChanceDenominator,
  int MinimumStrength,
  int MaximumStrengthExclusive,
  int MinimumSteps,
  int MaximumStepsExclusive)
{
  public int CalculateInvocationCount(int width, int height, bool isRemixWorld)
  {
    if (width <= 0 || height <= 0 || !double.IsFinite(Density) || Density < 0.0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    int baseCount = checked((int)(width * (double)height * Density));
    return isRemixWorld
      ? checked((int)(baseCount * 1.1))
      : baseCount;
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

    return isRemixWorld ? (int)(strength * 0.7) : strength;
  }

  public int ScaleSteps(int steps, bool isRemixWorld)
  {
    if (steps < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(steps));
    }

    return isRemixWorld ? (int)(steps * 0.7) : steps;
  }

  public void Validate()
  {
    if (!double.IsFinite(Density) || Density < 0.0 ||
        LiquidPreservingChanceDenominator <= 0 ||
        MinimumStrength <= 0 || MaximumStrengthExclusive <= MinimumStrength ||
        MinimumSteps <= 0 || MaximumStepsExclusive <= MinimumSteps)
    {
      throw new ArgumentException(
        "RockLayerCaves pass definition contains an invalid source contract.",
        nameof(Density));
    }
  }
}

public static class LegacyRockLayerCavesPassDefinitionFactory
{
  public static LegacyRockLayerCavesPassDefinition CreateDefault()
  {
    return new LegacyRockLayerCavesPassDefinition(
      Density: 0.00013,
      LiquidPreservingChanceDenominator: 10,
      MinimumStrength: 6,
      MaximumStrengthExclusive: 20,
      MinimumSteps: 50,
      MaximumStepsExclusive: 300);
  }
}
