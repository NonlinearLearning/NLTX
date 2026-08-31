using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct WorldTransformationStateSnapshot
{
  public WorldTransformationStateSnapshot(int activeTransformations)
  {
    if (activeTransformations < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(activeTransformations));
    }

    ActiveTransformations = activeTransformations;
  }

  public int ActiveTransformations { get; }

  public bool IsTransforming => ActiveTransformations > 0;
}
