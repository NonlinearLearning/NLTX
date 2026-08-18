using System;

namespace Terraria.Dome.Simulation.WorldModel;

public sealed record WorldMetadata
{
  public WorldMetadata(
    string name,
    WorldSeed seed,
    int width,
    int height,
    int? worldId = null,
    int? spawnX = null,
    int? spawnY = null,
    string seedVariant = "default",
    int randomStreamVersion = 1)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(name);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
    ArgumentException.ThrowIfNullOrWhiteSpace(seedVariant);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(randomStreamVersion);
    if (width % WorldGrid.SectionWidth != 0 || height % WorldGrid.SectionHeight != 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(width),
        "World dimensions must be whole Terraria section units.");
    }

    Name = name;
    Seed = seed;
    Width = width;
    Height = height;
    WorldId = worldId ?? seed.Value;
    SpawnX = spawnX ?? width / 2;
    SpawnY = spawnY ?? height / 4;
    SeedVariant = seedVariant;
    RandomStreamVersion = randomStreamVersion;
    if (WorldId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldId));
    }

    if (SpawnX < 0 || SpawnX >= width)
    {
      throw new ArgumentOutOfRangeException(nameof(spawnX));
    }

    if (SpawnY < 0 || SpawnY >= height)
    {
      throw new ArgumentOutOfRangeException(nameof(spawnY));
    }
  }

  public int Height { get; }
  public string Name { get; }
  public WorldSeed Seed { get; }
  public int Width { get; }
  public int WorldId { get; }
  public int SpawnX { get; }
  public int SpawnY { get; }
  public int RandomStreamVersion { get; }
  public string SeedVariant { get; }

  public bool IsInside(int x, int y)
  {
    return x >= 0 && x < Width && y >= 0 && y < Height;
  }
}
