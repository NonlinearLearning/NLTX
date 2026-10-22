using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.World;

public sealed class WorldHeaderSnapshot
{
  public WorldHeaderSnapshot(
    string name,
    WorldGeometry geometry,
    TileCoordinate spawn,
    Guid uniqueId,
    bool dayTime,
    double time,
    byte moonPhase,
    int worldId,
    WorldHeaderFacts facts,
    WorldTeamSpawnFacts? teamSpawns = null)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(name);
    ArgumentNullException.ThrowIfNull(facts);
    long tileCount = (long)geometry.Width * geometry.Height;
    if (geometry.Width <= 0 || geometry.Height <= 0 ||
        tileCount > WorldGeometry.MaximumTileCount)
    {
      throw new ArgumentOutOfRangeException(nameof(geometry));
    }

    if (!geometry.Contains(spawn))
    {
      throw new ArgumentOutOfRangeException(nameof(spawn));
    }

    if (!double.IsFinite(time) || time < 0 ||
        time > (dayTime ? WorldTimeState.DayLength : WorldTimeState.NightLength))
    {
      throw new ArgumentOutOfRangeException(nameof(time));
    }

    if (moonPhase > 7)
    {
      throw new ArgumentOutOfRangeException(nameof(moonPhase));
    }

    teamSpawns ??= WorldTeamSpawnFacts.Unavailable;
    if (teamSpawns.ExtraSpawnPoints.Any(point => !geometry.Contains(point))) {
      throw new ArgumentOutOfRangeException(nameof(teamSpawns));
    }

    Name = name;
    Geometry = geometry;
    Spawn = spawn;
    UniqueId = uniqueId;
    DayTime = dayTime;
    Time = time;
    MoonPhase = moonPhase;
    WorldId = worldId;
    Facts = facts;
    TeamSpawns = teamSpawns;
  }

  public string Name { get; }

  public WorldGeometry Geometry { get; }

  public TileCoordinate Spawn { get; }

  public Guid UniqueId { get; }

  public bool DayTime { get; }

  public double Time { get; }

  public byte MoonPhase { get; }

  public int WorldId { get; }

  public WorldHeaderFacts Facts { get; }

  public WorldTeamSpawnFacts TeamSpawns { get; }
}

/// <summary>Small immutable P03-to-admission capture that does not copy the tile map.</summary>
public sealed class WorldHeaderCapture
{
  internal WorldHeaderCapture(
    WorldHeaderSnapshot header,
    WorldFacts facts,
    ulong worldEpoch,
    CommitVersion version)
  {
    ArgumentNullException.ThrowIfNull(header);
    ArgumentNullException.ThrowIfNull(facts);
    if (worldEpoch == 0 || !version.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(worldEpoch));
    }

    Header = header;
    Facts = facts;
    WorldEpoch = worldEpoch;
    Version = version;
  }

  public WorldHeaderSnapshot Header { get; }

  public WorldFacts Facts { get; }

  public ulong WorldEpoch { get; }

  public CommitVersion Version { get; }
}

/// <summary>Version-bound initial section plan supplied to the session owner.</summary>
public sealed class WorldSectionPlan
{
  internal WorldSectionPlan(
    ulong worldEpoch,
    CommitVersion version,
    IReadOnlyCollection<WorldSectionCoordinate> sections)
  {
    if (worldEpoch == 0 || !version.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(worldEpoch));
    }

    ArgumentNullException.ThrowIfNull(sections);
    WorldEpoch = worldEpoch;
    Version = version;
    Sections = Array.AsReadOnly(sections
      .Distinct()
      .OrderBy(section => section.Y)
      .ThenBy(section => section.X)
      .ToArray());
  }

  public ulong WorldEpoch { get; }

  public CommitVersion Version { get; }

  public IReadOnlyList<WorldSectionCoordinate> Sections { get; }
}

public sealed class WorldSectionSnapshot
{
  private readonly WorldTile[] _tiles;
  private readonly IReadOnlyList<WorldTile> _readOnlyTiles;

  internal WorldSectionSnapshot(
    WorldSectionCoordinate coordinate,
    WorldSectionBounds bounds,
    ulong worldEpoch,
    CommitVersion version,
    WorldTile[] tiles)
  {
    Coordinate = coordinate;
    Bounds = bounds;
    WorldEpoch = worldEpoch;
    Version = version;
    _tiles = tiles;
    _readOnlyTiles = Array.AsReadOnly(_tiles);
  }

  public WorldSectionCoordinate Coordinate { get; }

  public WorldSectionBounds Bounds { get; }

  /// <summary>Identifies the published world instance that produced this section.</summary>
  public ulong WorldEpoch { get; }

  public CommitVersion Version { get; }

  public IReadOnlyList<WorldTile> Tiles => _readOnlyTiles;

  public WorldTile GetTile(int x, int y)
  {
    if (x < 0 || y < 0 || x >= Bounds.Width || y >= Bounds.Height)
    {
      throw new ArgumentOutOfRangeException();
    }

    return _tiles[(y * Bounds.Width) + x];
  }
}

/// <summary>
/// Immutable section batch captured under one world-owner lock and one commit version.
/// </summary>
public sealed class WorldSectionSnapshotBatch
{
  internal WorldSectionSnapshotBatch(
    ulong worldEpoch,
    CommitVersion version,
    IReadOnlyCollection<WorldSectionSnapshot> sections)
  {
    if (worldEpoch == 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldEpoch));
    }

    if (!version.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(version));
    }

    ArgumentNullException.ThrowIfNull(sections);
    WorldSectionSnapshot[] orderedSections = sections
      .OrderBy(section => section.Coordinate.Y)
      .ThenBy(section => section.Coordinate.X)
      .ToArray();
    if (orderedSections.Any(section =>
        section.WorldEpoch != worldEpoch || section.Version != version))
    {
      throw new ArgumentException(
        "Every section in a batch must belong to the same world epoch and commit version.",
        nameof(sections));
    }

    if (orderedSections
      .Select(section => section.Coordinate)
      .Distinct()
      .Count() != orderedSections.Length)
    {
      throw new ArgumentException("A section batch cannot contain duplicate coordinates.", nameof(sections));
    }

    WorldEpoch = worldEpoch;
    Version = version;
    Sections = Array.AsReadOnly(orderedSections);
  }

  public ulong WorldEpoch { get; }

  public CommitVersion Version { get; }

  public IReadOnlyList<WorldSectionSnapshot> Sections { get; }
}

public sealed class WorldSnapshot
{
  private readonly WorldTile[] _tiles;
  private readonly IReadOnlyList<WorldTile> _readOnlyTiles;

  internal WorldSnapshot(
    WorldHeaderSnapshot header,
    WorldFacts facts,
    ulong worldEpoch,
    CommitVersion version,
    IReadOnlyList<WorldTile> tiles,
    IReadOnlyCollection<WorldSectionCoordinate> dirtySections)
  {
    Header = header;
    Facts = facts;
    WorldEpoch = worldEpoch;
    Version = version;
    _tiles = tiles.ToArray();
    _readOnlyTiles = Array.AsReadOnly(_tiles);
    DirtySections = Array.AsReadOnly(dirtySections
      .OrderBy(coordinate => coordinate.Y)
      .ThenBy(coordinate => coordinate.X)
      .ToArray());
  }

  public WorldHeaderSnapshot Header { get; }

  public WorldFacts Facts { get; }

  public ulong WorldEpoch { get; }

  public CommitVersion Version { get; }

  public IReadOnlyList<WorldTile> Tiles => _readOnlyTiles;

  public IReadOnlyCollection<WorldSectionCoordinate> DirtySections { get; }

  public WorldTile GetTile(TileCoordinate coordinate)
  {
    if (!Header.Geometry.Contains(coordinate))
    {
      throw new ArgumentOutOfRangeException(nameof(coordinate));
    }

    return _tiles[(coordinate.Y * Header.Geometry.Width) + coordinate.X];
  }

  public WorldSectionSnapshot CaptureSection(WorldSectionCoordinate coordinate)
  {
    WorldSectionBounds bounds = Header.Geometry.GetSectionBounds(coordinate);
    var tiles = new WorldTile[bounds.Width * bounds.Height];
    for (int y = 0; y < bounds.Height; y++)
    {
      for (int x = 0; x < bounds.Width; x++)
      {
        tiles[(y * bounds.Width) + x] = GetTile(
          new TileCoordinate(bounds.OriginX + x, bounds.OriginY + y));
      }
    }

    return new WorldSectionSnapshot(coordinate, bounds, WorldEpoch, Version, tiles);
  }
}

/// <summary>Immutable save boundary supplied to P08 after a single world commit.</summary>
public sealed class WorldSaveSnapshot
{
  internal WorldSaveSnapshot(WorldSnapshot world)
  {
    ArgumentNullException.ThrowIfNull(world);
    World = world;
  }

  public WorldSnapshot World { get; }

  public WorldHeaderSnapshot Header => World.Header;

  public WorldFacts Facts => World.Facts;

  public ulong WorldEpoch => World.WorldEpoch;

  public CommitVersion Version => World.Version;

  public IReadOnlyList<WorldTile> Tiles => World.Tiles;
}
