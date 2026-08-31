using System;

namespace Terraria.Dome.Simulation.WorldModel;

public sealed record WorldMetadata
{
  private const double NoFunctionalSurfaceThreshold = 30.0;
  private const double WorldSurfaceThreshold = 50.0;

  public WorldMetadata(
    string name,
    WorldSeed seed,
    int width,
    int height,
    int? worldId = null,
    int? spawnX = null,
    int? spawnY = null,
    string seedVariant = "default",
    int randomStreamVersion = 1,
    double? worldSurface = null,
    double? rockLayer = null,
    bool? isRemixWorld = null,
    ulong? worldGeneratorVersion = null,
    Guid? uniqueId = null,
    string? seedText = null,
    bool? isNoTrapsWorld = null,
    bool? isSkyblockWorld = null,
    bool? isGoodWorld = null)
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

    if (rockLayer is double selectedRockLayer &&
        (!double.IsFinite(selectedRockLayer) || selectedRockLayer < 0 || selectedRockLayer >= height))
    {
      throw new ArgumentOutOfRangeException(nameof(rockLayer));
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
    WorldSurface = worldSurface;
    RockLayer = rockLayer;
    IsRemixWorld = isRemixWorld;
    WorldGeneratorVersion = worldGeneratorVersion;
    UniqueId = uniqueId;
    SeedText = seedText;
    IsNoTrapsWorld = isNoTrapsWorld;
    IsSkyblockWorld = isSkyblockWorld;
    IsGoodWorld = isGoodWorld;
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
  public float BottomWorld => Height * 16f;
  public string Name { get; }
  public WorldSeed Seed { get; }
  public float LeftWorld => 0f;
  public int MaxSectionsX => Width / WorldGrid.SectionWidth;
  public int MaxSectionsY => Height / WorldGrid.SectionHeight;
  public float RightWorld => Width * 16f;
  public float TopWorld => 0f;
  public int Width { get; }
  public int WorldId { get; }
  public int SpawnX { get; }
  public int SpawnY { get; }
  public int RandomStreamVersion { get; }
  public string SeedVariant { get; }
  public double? WorldSurface { get; }
  public double? RockLayer { get; }
  public bool? IsRemixWorld { get; }
  public bool? IsNoTrapsWorld { get; }
  public bool? IsSkyblockWorld { get; }
  public bool? IsGoodWorld { get; }
  public bool HasNoFunctionalSurface =>
    WorldSurface is not double worldSurface || worldSurface <= NoFunctionalSurfaceThreshold;
  public bool HasWorldSurface =>
    WorldSurface is double worldSurface && double.IsFinite(worldSurface) &&
    worldSurface > WorldSurfaceThreshold;
  public ulong? WorldGeneratorVersion { get; }
  public Guid? UniqueId { get; }
  public string? SeedText { get; }

  public WorldMetadata WithRockLayer(double rockLayer)
  {
    return new WorldMetadata(
      Name,
      Seed,
      Width,
      Height,
      WorldId,
      SpawnX,
      SpawnY,
      SeedVariant,
      RandomStreamVersion,
      WorldSurface,
      rockLayer,
      IsRemixWorld,
      WorldGeneratorVersion,
      UniqueId,
      SeedText,
      IsNoTrapsWorld,
      IsSkyblockWorld,
      IsGoodWorld);
  }

  public bool IsInside(int x, int y)
  {
    return x >= 0 && x < Width && y >= 0 && y < Height;
  }
}
