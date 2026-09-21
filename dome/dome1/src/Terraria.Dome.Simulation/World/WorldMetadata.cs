using System;

namespace Terraria.Dome.Simulation.WorldModel;

public sealed record WorldMetadata
{
  private const double NoFunctionalSurfaceThreshold = WorldSurfacePolicy.NoFunctionalSurfaceThreshold;

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
    bool? isGoodWorld = null,
    bool? isDrunkWorld = null,
    bool? isEverythingWorld = null,
    bool? isDontStarveWorld = null,
    bool? isNotTheBeesWorld = null,
    bool? isZenithWorld = null,
    bool? isTenthAnniversaryWorld = null,
    bool? isVampireWorld = null,
    bool? isInfectedWorld = null,
    bool? isTeamBasedSpawnsWorld = null,
    bool? isDualDungeonsWorld = null)
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
    IsDrunkWorld = isDrunkWorld;
    IsEverythingWorld = isEverythingWorld;
    IsDontStarveWorld = isDontStarveWorld;
    IsNotTheBeesWorld = isNotTheBeesWorld;
    IsZenithWorld = isZenithWorld;
    IsTenthAnniversaryWorld = isTenthAnniversaryWorld;
    IsVampireWorld = isVampireWorld;
    IsInfectedWorld = isInfectedWorld;
    IsTeamBasedSpawnsWorld = isTeamBasedSpawnsWorld;
    IsDualDungeonsWorld = isDualDungeonsWorld;
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
  public float BottomWorld => WorldCoordinateRules.BottomWorld(this);
  public string Name { get; }
  public WorldSeed Seed { get; }
  public float LeftWorld => WorldCoordinateRules.LeftWorld(this);
  public int MaxSectionsX => Width / WorldGrid.SectionWidth;
  public int MaxSectionsY => Height / WorldGrid.SectionHeight;
  public float RightWorld => WorldCoordinateRules.RightWorld(this);
  public float TopWorld => WorldCoordinateRules.TopWorld(this);
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
  public bool? IsDrunkWorld { get; }
  public bool? IsEverythingWorld { get; }
  public bool? IsDontStarveWorld { get; }
  public bool? IsNotTheBeesWorld { get; }
  public bool? IsZenithWorld { get; }
  public bool? IsTenthAnniversaryWorld { get; }
  public bool? IsVampireWorld { get; }
  public bool? IsInfectedWorld { get; }
  public bool? IsTeamBasedSpawnsWorld { get; }
  public bool? IsDualDungeonsWorld { get; }
  public bool HasNoFunctionalSurface =>
    WorldSurface is not double worldSurface || worldSurface <= NoFunctionalSurfaceThreshold;

  public bool NoFunctionalSurface => WorldSurfacePolicy.NoFunctionalSurface(this);
  public (int X, int Y) SpawnTile => WorldSpawnPolicy.Resolve(this);
  public bool HasWorldSurface => WorldSurfaceQuery.IsThereAWorldSurface(this);
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
      IsGoodWorld,
      IsDrunkWorld,
      IsEverythingWorld,
      IsDontStarveWorld,
      IsNotTheBeesWorld,
      IsZenithWorld,
      IsTenthAnniversaryWorld,
      IsVampireWorld,
      IsInfectedWorld,
      IsTeamBasedSpawnsWorld,
      IsDualDungeonsWorld);
  }

  public bool IsInside(int x, int y)
  {
    return x >= 0 && x < Width && y >= 0 && y < Height;
  }
}
