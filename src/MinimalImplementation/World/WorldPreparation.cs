using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.World;

public sealed class WorldPreparationRequest
{
  public WorldPreparationRequest(
    WorldHeaderSnapshot header,
    IReadOnlyList<WorldTile> tiles,
    string factsFingerprint)
  {
    ArgumentNullException.ThrowIfNull(header);
    ArgumentNullException.ThrowIfNull(tiles);
    ArgumentException.ThrowIfNullOrWhiteSpace(factsFingerprint);
    if (tiles.Count != header.Geometry.TileCount)
    {
      throw new ArgumentException(
        "The tile payload must contain exactly width * height entries.", nameof(tiles));
    }

    Header = header;
    // Copy at the persistence-to-domain boundary so a caller cannot mutate a candidate after
    // validation has started. WorldPreparedWorld can safely share this immutable view.
    Tiles = Array.AsReadOnly(tiles.ToArray());
    FactsFingerprint = factsFingerprint;
  }

  public WorldHeaderSnapshot Header { get; }

  public IReadOnlyList<WorldTile> Tiles { get; }

  public string FactsFingerprint { get; }
}

public sealed class WorldPreparedWorld
{
  internal Guid OwnerToken { get; }

  internal ulong BaseWorldEpoch { get; }

  internal CommitVersion BaseCommitVersion { get; }

  internal WorldPreparedWorld(
    WorldHeaderSnapshot header,
    IReadOnlyList<WorldTile> tiles,
    string factsFingerprint,
    Guid ownerToken,
    ulong baseWorldEpoch,
    CommitVersion baseCommitVersion)
  {
    Header = header;
    Tiles = tiles;
    FactsFingerprint = factsFingerprint;
    OwnerToken = ownerToken;
    BaseWorldEpoch = baseWorldEpoch;
    BaseCommitVersion = baseCommitVersion;
  }

  public WorldHeaderSnapshot Header { get; }

  public IReadOnlyList<WorldTile> Tiles { get; }

  public string FactsFingerprint { get; }
}

public sealed class WorldPublicationResult
{
  private WorldPublicationResult(
    bool published,
    ulong worldEpoch,
    CommitVersion? version,
    string? rejection)
  {
    Published = published;
    WorldEpoch = worldEpoch;
    Version = version;
    Rejection = rejection;
  }

  public bool Published { get; }

  public ulong WorldEpoch { get; }

  public CommitVersion? Version { get; }

  public string? Rejection { get; }

  internal static WorldPublicationResult Success(ulong epoch, CommitVersion version) =>
    new(true, epoch, version, null);

  internal static WorldPublicationResult Rejected(ulong epoch, string detail) =>
    new(false, epoch, null, detail);
}

public sealed class WorldValidationResult
{
  private WorldValidationResult(bool isValid, IReadOnlyList<string> errors)
  {
    IsValid = isValid;
    Errors = Array.AsReadOnly(errors.ToArray());
  }

  public bool IsValid { get; }

  public IReadOnlyList<string> Errors { get; }

  public static WorldValidationResult Valid() =>
    new(true, Array.Empty<string>());

  public static WorldValidationResult Invalid(IEnumerable<string> errors)
  {
    ArgumentNullException.ThrowIfNull(errors);
    string[] materialized = errors.Where(error => !string.IsNullOrWhiteSpace(error)).ToArray();
    return new(false, materialized.Length == 0
      ? ["The world candidate is invalid."]
      : materialized);
  }
}

public sealed class WorldMutationResult
{
  private WorldMutationResult(
    bool applied,
    CommitVersion? version,
    IReadOnlyCollection<TileCoordinate> changedTiles,
    IReadOnlyCollection<WorldSectionCoordinate> dirtySections,
    bool producesImmediateBroadcast,
    IReadOnlyCollection<LiquidMergeAction> mergeActions,
    string? rejection)
  {
    Applied = applied;
    Version = version;
    ChangedTiles = Array.AsReadOnly(changedTiles
      .OrderBy(coordinate => coordinate.Y)
      .ThenBy(coordinate => coordinate.X)
      .ToArray());
    DirtySections = Array.AsReadOnly(dirtySections
      .OrderBy(coordinate => coordinate.Y)
      .ThenBy(coordinate => coordinate.X)
      .ToArray());
    ProducesImmediateBroadcast = producesImmediateBroadcast;
    MergeActions = Array.AsReadOnly(mergeActions.ToArray());
    Rejection = rejection;
  }

  public bool Applied { get; }

  public CommitVersion? Version { get; }

  public IReadOnlyCollection<TileCoordinate> ChangedTiles { get; }

  public IReadOnlyCollection<WorldSectionCoordinate> DirtySections { get; }

  public bool ProducesImmediateBroadcast { get; }

  /// <summary>Committed merge facts; returning these does not execute cross-owner effects.</summary>
  public IReadOnlyList<LiquidMergeAction> MergeActions { get; }

  public string? Rejection { get; }

  internal static WorldMutationResult Success(
    CommitVersion version,
    IReadOnlyCollection<TileCoordinate> changedTiles,
    IReadOnlyCollection<WorldSectionCoordinate> dirtySections,
    bool producesImmediateBroadcast,
    IReadOnlyCollection<LiquidMergeAction>? mergeActions = null)
  {
    return new WorldMutationResult(
      true,
      version,
      changedTiles,
      dirtySections,
      producesImmediateBroadcast,
      mergeActions ?? Array.Empty<LiquidMergeAction>(),
      null);
  }

  internal static WorldMutationResult Rejected(string detail)
  {
    return new WorldMutationResult(
      false,
      null,
      Array.Empty<TileCoordinate>(),
      Array.Empty<WorldSectionCoordinate>(),
      false,
      Array.Empty<LiquidMergeAction>(),
      detail);
  }
}

public sealed class WorldDirtyBatch
{
  private readonly IReadOnlyDictionary<WorldSectionCoordinate, CommitVersion> _sectionVersions;

  internal Guid OwnerToken { get; }

  internal IReadOnlyDictionary<WorldSectionCoordinate, CommitVersion> SectionVersions =>
    _sectionVersions;

  internal WorldDirtyBatch(
    Guid ownerToken,
    ulong worldEpoch,
    CommitVersion version,
    IReadOnlyDictionary<WorldSectionCoordinate, CommitVersion> sectionVersions)
  {
    OwnerToken = ownerToken;
    WorldEpoch = worldEpoch;
    Version = version;
    _sectionVersions = new System.Collections.ObjectModel.ReadOnlyDictionary<WorldSectionCoordinate, CommitVersion>(
      sectionVersions.ToDictionary(pair => pair.Key, pair => pair.Value));
    Sections = Array.AsReadOnly(_sectionVersions.Keys
      .OrderBy(coordinate => coordinate.Y)
      .ThenBy(coordinate => coordinate.X)
      .ToArray());
  }

  public ulong WorldEpoch { get; }

  public CommitVersion Version { get; }

  public IReadOnlyCollection<WorldSectionCoordinate> Sections { get; }
}
