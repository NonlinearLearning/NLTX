using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct WorldSeedComponent
{
  public WorldSeedComponent(int seed, string seedVariant, int randomStreamVersion)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(seedVariant);
    if (randomStreamVersion <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(randomStreamVersion));
    }

    Seed = seed;
    SeedVariant = seedVariant;
    RandomStreamVersion = randomStreamVersion;
  }

  public int Seed { get; }
  public string SeedVariant { get; }
  public int RandomStreamVersion { get; }
}
