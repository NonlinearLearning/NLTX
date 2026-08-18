using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct BiomeSurfaceComponent
{
  public BiomeSurfaceComponent(string biomeId)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(biomeId);
    BiomeId = biomeId;
  }

  public string BiomeId { get; }
}
