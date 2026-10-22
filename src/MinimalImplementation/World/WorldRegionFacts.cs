using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.World;

/// <summary>
/// Version-bound read-only facts for a bounded world region. This is a projection, not a second
/// tile store, and is suitable for item reach, projectile collision and pylon preflight checks.
/// </summary>
public sealed class WorldRegionFacts
{
  private WorldRegionFacts(
      ulong worldEpoch,
      CommitVersion version,
      WorldSectionBounds bounds,
      int activeTiles,
      int solidTiles,
      int wallTiles,
      int[] liquidCounts,
      bool hasFrameImportantTile,
      bool hasPylon,
      bool hasUnbreakableWall)
  {
    WorldEpoch = worldEpoch;
    Version = version;
    Bounds = bounds;
    ActiveTileCount = activeTiles;
    SolidTileCount = solidTiles;
    WallTileCount = wallTiles;
    LiquidCounts = Array.AsReadOnly(liquidCounts);
    HasFrameImportantTile = hasFrameImportantTile;
    HasPylon = hasPylon;
    HasUnbreakableWall = hasUnbreakableWall;
  }

  public ulong WorldEpoch { get; }

  public CommitVersion Version { get; }

  public WorldSectionBounds Bounds { get; }

  public int ActiveTileCount { get; }

  public int SolidTileCount { get; }

  public int WallTileCount { get; }

  public IReadOnlyList<int> LiquidCounts { get; }

  public bool HasFrameImportantTile { get; }

  public bool HasPylon { get; }

  public bool HasUnbreakableWall { get; }

  internal static WorldRegionFacts Capture(
      WorldSpatialSnapshot snapshot, WorldSectionBounds bounds)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    snapshot.RequireCoverage(bounds);
    int activeTiles = 0;
    int solidTiles = 0;
    int wallTiles = 0;
    var liquids = new int[4];
    bool hasFrameImportantTile = false;
    bool hasPylon = false;
    bool hasUnbreakableWall = false;
    for (int y = bounds.OriginY; y < bounds.OriginY + bounds.Height; y++)
    {
      for (int x = bounds.OriginX; x < bounds.OriginX + bounds.Width; x++)
      {
        WorldTile tile = snapshot.GetTile(new TileCoordinate(x, y));
        if (tile.Active)
        {
          activeTiles++;
          if (snapshot.Facts.Solid[tile.Type])
          {
            solidTiles++;
          }

          hasFrameImportantTile |= snapshot.Facts.FrameImportant[tile.Type];
          hasPylon |= tile.Type == 597;
        }

        if (tile.Wall != 0)
        {
          wallTiles++;
          hasUnbreakableWall |= tile.Wall == 350 && tile.WallColor >= 16;
        }

        if (tile.LiquidAmount > 0)
        {
          liquids[(int)tile.LiquidType]++;
        }
      }
    }

    return new WorldRegionFacts(
        snapshot.WorldEpoch,
        snapshot.Version,
        bounds,
        activeTiles,
        solidTiles,
        wallTiles,
        liquids,
        hasFrameImportantTile,
        hasPylon,
        hasUnbreakableWall);
  }
}

public sealed partial class WorldOwner
{
  /// <summary>Captures bounded world facts under one epoch/version without retaining tile state.</summary>
  public WorldRegionFacts CaptureRegionFacts(WorldSectionBounds bounds)
  {
    lock (_gate)
    {
      EnsureReady();
      if (bounds.Width <= 0 || bounds.Height <= 0 ||
          bounds.OriginX < 0 || bounds.OriginY < 0 ||
          (long)bounds.OriginX + bounds.Width > _header!.Geometry.Width ||
          (long)bounds.OriginY + bounds.Height > _header.Geometry.Height ||
          (long)bounds.Width * bounds.Height > WorldSpatialSnapshot.MaximumCaptureTiles)
      {
        throw new ArgumentOutOfRangeException(nameof(bounds));
      }

      return WorldRegionFacts.Capture(CaptureSpatialSnapshot(bounds), bounds);
    }
  }
}
