using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// 保存世界身份、尺寸、种子、地层和出生点等基础信息。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Main。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Main.cs。</para>
/// <para>
/// 主要源成员：maxTilesX（第 522 行）； maxTilesY（第 524 行）； dungeonX（第 565 行）； dungeonY（第 567 行）；
/// worldName（第 585 行）； worldSurface（第 587 行）； rockLayer（第 589 行）； spawnTileX（第 986 行）；
/// spawnTileY（第 988 行）； worldID（第 1434 行）。
/// </para>
/// <para>拆分来源：Terraria.IO.WorldFileData。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.IO/WorldFileData.cs。</para>
/// <para>主要源成员：WorldGeneratorVersion（第 29 行）； UniqueId（第 39 行）； SeedText（第 84 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>
/// 拆分依据文件：2026-09-06-version4-world-generation-and-ecology-component-code-draft.md。
/// </para>
/// <para>依据位置：第 228 行。</para>
/// </remarks>
public sealed class WorldDescriptorComponent
{
  private const int SectionHeight = 150;
  private const int SectionWidth = 200;

  public WorldDescriptorComponent(
    int worldId,
    string name,
    int seed,
    int sizeX,
    int sizeY,
    WorldBounds bounds,
    double surfaceLayer,
    double rockLayer,
    TilePosition spawnTile,
    TilePosition? dungeonTile = null,
    Guid? uniqueId = null,
    string? seedText = null,
    ulong? generatorVersion = null)
  {
    if (worldId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldId));
    }

    ArgumentException.ThrowIfNullOrWhiteSpace(name);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sizeX);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sizeY);
    ValidateBounds(bounds);

    if (!double.IsFinite(surfaceLayer) || !double.IsFinite(rockLayer))
    {
      throw new ArgumentOutOfRangeException(nameof(surfaceLayer));
    }

    if (surfaceLayer > rockLayer)
    {
      throw new ArgumentException(
        "The surface layer cannot be below the rock layer.",
        nameof(rockLayer));
    }

    if (!Contains(sizeX, sizeY, spawnTile))
    {
      throw new ArgumentOutOfRangeException(nameof(spawnTile));
    }

    if (dungeonTile is TilePosition selectedDungeonTile &&
        !Contains(sizeX, sizeY, selectedDungeonTile))
    {
      throw new ArgumentOutOfRangeException(nameof(dungeonTile));
    }

    WorldId = worldId;
    UniqueId = uniqueId == Guid.Empty ? null : uniqueId;
    Name = name;
    Seed = seed;
    SeedText = seedText;
    GeneratorVersion = generatorVersion;
    SizeX = sizeX;
    SizeY = sizeY;
    Bounds = bounds;
    SurfaceLayer = surfaceLayer;
    RockLayer = rockLayer;
    SpawnTile = spawnTile;
    DungeonTile = dungeonTile;
  }

  public int WorldId { get; }

  public Guid? UniqueId { get; }

  public string Name { get; }

  public int Seed { get; }

  public string? SeedText { get; }

  public ulong? GeneratorVersion { get; }

  public int SizeX { get; }

  public int SizeY { get; }

  public WorldBounds Bounds { get; }

  public double SurfaceLayer { get; }

  public double RockLayer { get; }

  public TilePosition SpawnTile { get; }

  public TilePosition? DungeonTile { get; }

  public int SectionCountX => SizeX / SectionWidth;

  public int SectionCountY => SizeY / SectionHeight;

  public bool HasSurface => SurfaceLayer > 50.0;

  private static bool Contains(int sizeX, int sizeY, TilePosition position)
  {
    return position.X >= 0 && position.X < sizeX &&
      position.Y >= 0 && position.Y < sizeY;
  }

  private static void ValidateBounds(WorldBounds bounds)
  {
    if (!double.IsFinite(bounds.Left) || !double.IsFinite(bounds.Top) ||
        !double.IsFinite(bounds.Right) || !double.IsFinite(bounds.Bottom) ||
        bounds.Right <= bounds.Left || bounds.Bottom <= bounds.Top)
    {
      throw new ArgumentException(
        "World bounds must be finite and have positive width and height.",
        nameof(bounds));
    }
  }
}
