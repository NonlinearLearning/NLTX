using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacySiltPassDefinition(
  double Density,
  int MinimumStrength,
  int MaximumStrengthExclusive,
  int MinimumSteps,
  int MaximumStepsExclusive,
  int TileType,
  int FirstExcludedWallType,
  int SecondExcludedWallType)
{
  public int CalculateInvocationCount(int width, int height)
  {
    if (width <= 0 || height <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    return checked((int)(width * (double)height * Density));
  }

  public bool IsWallEligible(int wallType)
  {
    return wallType != FirstExcludedWallType && wallType != SecondExcludedWallType;
  }

  public void Validate()
  {
    if (Density < 0.0 || MaximumStrengthExclusive <= MinimumStrength ||
        MaximumStepsExclusive <= MinimumSteps || TileType < 0)
    {
      throw new InvalidOperationException("Silt pass definition is invalid.");
    }
  }

  public static LegacySiltPassDefinition CreateDefault()
  {
    return new LegacySiltPassDefinition(0.0001, 5, 12, 15, 50, 123, 187, 216);
  }

  public static LegacySiltPassDefinition CreateSecondary()
  {
    return new LegacySiltPassDefinition(0.0005, 2, 5, 2, 5, 123, 187, 216);
  }
}
