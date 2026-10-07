using System;

namespace Terraria.WorldGeneration.Components;

public readonly record struct GenerationPassDescriptor
{
  public GenerationPassDescriptor(string id, double weight, int version = 1)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(id);
    if (!double.IsFinite(weight) || weight < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(weight));
    }

    if (version <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(version));
    }

    Id = id;
    Weight = weight;
    Version = version;
  }

  public string Id { get; }

  public double Weight { get; }

  public int Version { get; }
}
