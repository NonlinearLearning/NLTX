namespace Terraria.NonAuthoritative.ContentDefinitions;

public enum PlacementHookKind
{
  CheckIfCanPlace,
  PostPlaceEveryone,
  PostPlaceMyPlayer,
  PlaceOverride
}

public sealed class TileObjectPlacementHookDefinition
{
  private readonly Dictionary<PlacementHookKind, string> _hookKeys = new();
  private readonly List<int> _subTiles = new();

  public TileObjectPlacementHookDefinition(int tileType)
  {
    if (tileType < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tileType));
    }

    TileType = tileType;
  }

  public int TileType { get; }

  public bool HasOwnPlacementHooks { get; internal set; }

  public bool HasOwnSubTiles { get; internal set; }

  public bool HasOwnTileObjectBase { get; internal set; }

  public bool HasOwnTileObjectCoordinates { get; internal set; }

  public IReadOnlyDictionary<PlacementHookKind, string> HookKeys => _hookKeys;

  public IReadOnlyList<int> SubTiles => _subTiles;

  internal void SetHook(PlacementHookKind kind, string key)
  {
    if (string.IsNullOrWhiteSpace(key))
    {
      throw new ArgumentException("A placement hook requires a stable key.", nameof(key));
    }

    _hookKeys[kind] = key;
    HasOwnPlacementHooks = true;
  }

  internal void AddSubTile(int tileType)
  {
    if (tileType < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tileType));
    }

    if (!_subTiles.Contains(tileType))
    {
      _subTiles.Add(tileType);
    }

    HasOwnSubTiles = true;
  }
}

public readonly record struct TilePlacementCommand(
  int TileType,
  PointValue Position,
  int Style);

public readonly record struct PlacementCommitResult(bool Committed, string? FailureReason);

public interface IWorldTilePlacementWriter
{
  PlacementCommitResult Commit(TilePlacementCommand command);
}

public sealed class TilePlacementCommandAdapter
{
  private readonly IWorldTilePlacementWriter _writer;

  public TilePlacementCommandAdapter(IWorldTilePlacementWriter writer)
  {
    _writer = writer ?? throw new ArgumentNullException(nameof(writer));
  }

  public PlacementCommitResult Submit(TilePlacementCommand command)
  {
    return _writer.Commit(command);
  }
}

public static class TilePlacementHookRegistrationSystem
{
  public static void RegisterHook(
    TileObjectPlacementHookDefinition definition,
    PlacementHookKind kind,
    string key)
  {
    ArgumentNullException.ThrowIfNull(definition);
    definition.SetHook(kind, key);
  }

  public static void RegisterSubTile(
    TileObjectPlacementHookDefinition definition,
    int tileType)
  {
    ArgumentNullException.ThrowIfNull(definition);
    definition.AddSubTile(tileType);
  }
}
