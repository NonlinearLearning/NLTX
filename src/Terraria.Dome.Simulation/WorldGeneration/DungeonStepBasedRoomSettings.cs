using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct DungeonStepBasedRoomSettings
{
  public DungeonStepBasedRoomSettings(
    int overrideStrength,
    int overrideSteps,
    double overrideInteriorToExteriorRatio = 0)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(overrideStrength);
    ArgumentOutOfRangeException.ThrowIfNegative(overrideSteps);

    if (double.IsNaN(overrideInteriorToExteriorRatio) ||
        overrideInteriorToExteriorRatio < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(overrideInteriorToExteriorRatio));
    }

    OverrideStrength = overrideStrength;
    OverrideSteps = overrideSteps;
    OverrideInteriorToExteriorRatio = overrideInteriorToExteriorRatio;
  }

  public int OverrideStrength { get; }

  public int OverrideSteps { get; }

  public double OverrideInteriorToExteriorRatio { get; }

  public int GetBoundingRadius()
  {
    return DungeonRoomBoundingRadiusQuery.StepBased(OverrideStrength, OverrideSteps);
  }
}
