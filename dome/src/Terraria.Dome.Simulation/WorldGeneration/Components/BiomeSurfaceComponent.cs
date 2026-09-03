using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct BiomeSurfaceComponent
{
  public BiomeSurfaceComponent(string biomeId)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(biomeId);
    BiomeId = biomeId;
  }

  public BiomeSurfaceComponent(BiomeSurfaceDefinition definition)
    : this(definition, 0)
  {
  }

  public BiomeSurfaceComponent(BiomeSurfaceDefinition definition, int surfaceY)
  {
    if (surfaceY < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(surfaceY));
    }

    Definition = definition;
    BiomeId = definition.Id;
    SurfaceY = surfaceY;
  }

  public string BiomeId { get; }

  public BiomeSurfaceDefinition? Definition { get; }

  public int SurfaceY { get; }
}
