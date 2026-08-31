using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed record LegacyDirtToMudPassDefinition(
  double Density,
  int MinimumStrength,
  int MaximumStrengthExclusive,
  int MinimumSteps,
  int MaximumStepsExclusive,
  int TileType,
  int OverrideTileType)
{
  public int CalculateInvocationCount(int width, int height)
  {
    if (width <= 0 || height <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }

    return checked((int)(width * (double)height * Density));
  }

  public void Validate()
  {
    if (Density < 0.0 || MaximumStrengthExclusive <= MinimumStrength ||
        MaximumStepsExclusive <= MinimumSteps || TileType < 0 || OverrideTileType < 0)
    {
      throw new InvalidOperationException("DirtToMud pass definition is invalid.");
    }
  }

  public static LegacyDirtToMudPassDefinition CreateDefault()
  {
    return new LegacyDirtToMudPassDefinition(0.001, 2, 6, 2, 40, 59, 53);
  }
}
