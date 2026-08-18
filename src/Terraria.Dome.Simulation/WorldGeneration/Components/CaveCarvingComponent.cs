using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct CaveCarvingComponent
{
  public CaveCarvingComponent(string algorithmId, int radius, int density)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(algorithmId);
    if (radius < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(radius));
    }

    if (density <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(density));
    }

    AlgorithmId = algorithmId;
    Radius = radius;
    Density = density;
  }

  public string AlgorithmId { get; }
  public int Radius { get; }
  public int Density { get; }
}
