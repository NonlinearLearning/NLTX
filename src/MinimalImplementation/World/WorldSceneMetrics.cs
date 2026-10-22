using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.World;

/// <summary>
/// SceneMetrics tile/liquid counts for one pylon anchor. Event progression and NPC presence
/// remain explicit inputs to their owners. Biome predicates below are the pylon thresholds,
/// rather than the player's height-filtered Zone flags.
/// </summary>
public sealed class WorldSceneMetrics {
  public const int ScanWidth = 169;
  public const int ScanHeight = 124;

  public TileCoordinate Anchor { get; }
  public WorldHeaderSnapshot Header { get; }
  public ulong WorldEpoch { get; }
  public CommitVersion Version { get; }
  public WorldSectionBounds ScanBounds { get; }
  public IReadOnlyList<int> TileCounts { get; }
  public IReadOnlyList<int> LiquidCounts { get; }
  public ushort AnchorWall { get; }
  public int JungleTileCount { get; }
  public int HolyTileCount { get; }
  public int EvilTileCount { get; }
  public int BloodTileCount { get; }
  public int SnowTileCount { get; }
  public int MushroomTileCount { get; }
  public int SandTileCount { get; }
  public int OceanSandTileCount { get; }
  public int DesertSandTileCount => Math.Max(0, SandTileCount - OceanSandTileCount);
  public int ShimmerTileCount => LiquidCounts[(int)LiquidType.Shimmer];
  public bool Jungle => JungleTileCount >= 140;
  public bool Hallow => HolyTileCount >= 125;
  public bool Corruption => EvilTileCount >= 300;
  public bool Crimson => BloodTileCount >= 300;
  public bool Snow => SnowTileCount >= (Header.Facts.SkyblockWorld ? 300 : 1500);
  public bool Desert => DesertSandTileCount >= (Header.Facts.SkyblockWorld ? 300 : 1500);
  public bool GlowingMushroom => MushroomTileCount >= 100;
  public bool Shimmer => ShimmerTileCount >= 300;
  public bool TempleWall => AnchorWall == 87;
  public bool SurfacePurity =>
      !(Jungle || Hallow || Corruption || Crimson || Snow || Desert || GlowingMushroom);

  private WorldSceneMetrics(
      WorldSpatialSnapshot snapshot, TileCoordinate anchor, WorldSectionBounds bounds,
      int[] tiles, int[] liquids, int oceanSand, bool infectedSeed) {
    Anchor = anchor;
    Header = snapshot.Header;
    WorldEpoch = snapshot.WorldEpoch;
    Version = snapshot.Version;
    ScanBounds = bounds;
    TileCounts = Array.AsReadOnly(tiles);
    LiquidCounts = Array.AsReadOnly(liquids);
    AnchorWall = snapshot.GetTile(anchor).Wall;
    JungleTileCount = Sum(60, 61, 62, 74, 225) + (Header.Facts.RemixWorld ? 0 : tiles[226]);
    SnowTileCount = Sum(147, 148, 161, 162, 164, 163, 200);
    MushroomTileCount = Sum(70, 71, 72, 528);
    SandTileCount = Sum(53, 112, 116, 234, 397, 398, 402, 399, 396, 400, 403, 401);
    OceanSandTileCount = oceanSand;
    int sunflower = tiles[27] * (infectedSeed ? -30 : -10);
    int holy = Sum(109, 492, 110, 113, 117, 116, 164, 403, 402);
    int evil = Math.Max(0, Sum(23, 661, 24, 25, 32, 112, 163, 400, 398) + sunflower +
        (Header.Facts.RemixWorld ? tiles[474] : 0));
    int blood = Math.Max(0, Sum(199, 662, 201, 203, 200, 401, 399, 234, 352) + sunflower +
        (Header.Facts.RemixWorld ? tiles[195] : 0));
    HolyTileCount = Math.Max(0, holy - evil - blood);
    EvilTileCount = Math.Max(0, evil - holy);
    BloodTileCount = Math.Max(0, blood - holy);
  }

  public static WorldSectionBounds GetScanBounds(WorldGeometry geometry, TileCoordinate anchor) {
    if (!geometry.Contains(anchor)) {
      throw new ArgumentOutOfRangeException(nameof(anchor));
    }
    int left = Math.Max(0, anchor.X - ScanWidth / 2);
    int top = Math.Max(0, anchor.Y - ScanHeight / 2);
    int right = Math.Min(geometry.Width, anchor.X - ScanWidth / 2 + ScanWidth);
    int bottom = Math.Min(geometry.Height, anchor.Y - ScanHeight / 2 + ScanHeight);
    return new WorldSectionBounds(left, top, right - left, bottom - top);
  }

  /// <param name="infectedSeed">Actual persisted time-policy seed; never a guessed default.</param>
  public static WorldSceneMetrics Capture(
      WorldSpatialSnapshot snapshot, TileCoordinate anchor, bool infectedSeed) {
    ArgumentNullException.ThrowIfNull(snapshot);
    WorldSectionBounds bounds = GetScanBounds(snapshot.Header.Geometry, anchor);
    snapshot.RequireCoverage(bounds);
    var tiles = new int[WorldFacts.TileCount];
    var liquids = new int[4];
    int oceanSand = 0;
    double oceanLevel = (snapshot.Header.Facts.WorldSurface + snapshot.Header.Facts.RockLayer) /
        2d + 40d;
    for (int x = bounds.OriginX; x < bounds.OriginX + bounds.Width; x++) {
      for (int y = bounds.OriginY; y < bounds.OriginY + bounds.Height; y++) {
        WorldTile tile = snapshot.GetTile(new TileCoordinate(x, y));
        if (tile.Active) {
          tiles[tile.Type]++;
          if ((tile.Type is 53 or 397 or 396 or 400 or 403 or 401) && y <= oceanLevel &&
              (x < 380 || x > snapshot.Header.Geometry.Width - 380)) {
            oceanSand++;
          }
        } else if (tile.LiquidAmount > 0) {
          liquids[(int)tile.LiquidType]++;
        }
      }
    }
    return new WorldSceneMetrics(snapshot, anchor, bounds, tiles, liquids, oceanSand, infectedSeed);
  }

  private int Sum(params int[] types) {
    int count = 0;
    foreach (int type in types) {
      count += TileCounts[type];
    }
    return count;
  }
}
