namespace Terraria.NonAuthoritative.ContentDefinitions;

public sealed class TileObjectInheritanceDefinition
{
  private readonly List<int> _alternates = new();

  public TileObjectInheritanceDefinition(int tileType)
  {
    if (tileType < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tileType));
    }

    TileType = tileType;
  }

  public int TileType { get; }

  public int? ParentTileType { get; private set; }

  public bool LinkedAlternates { get; private set; }

  public bool HasOwnAlternates { get; private set; }

  public int AlternatesCount => _alternates.Count;

  public bool IsReadOnly { get; internal set; }

  public IReadOnlyList<int> Alternates => _alternates;

  internal void CopyFrom(TileObjectInheritanceDefinition source)
  {
    EnsureWritable();
    ParentTileType = source.TileType;
    LinkedAlternates = source.LinkedAlternates;
    HasOwnAlternates = source.HasOwnAlternates;
    _alternates.Clear();
    _alternates.AddRange(source._alternates);
  }

  internal void FullCopyFrom(TileObjectInheritanceDefinition source)
  {
    EnsureWritable();
    ParentTileType = source.ParentTileType;
    LinkedAlternates = source.LinkedAlternates;
    HasOwnAlternates = source.HasOwnAlternates;
    _alternates.Clear();
    _alternates.AddRange(source._alternates);
  }

  internal void AddAlternate(int tileType)
  {
    EnsureWritable();
    if (tileType < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tileType));
    }

    if (!_alternates.Contains(tileType))
    {
      _alternates.Add(tileType);
    }

    HasOwnAlternates = true;
  }

  private void EnsureWritable()
  {
    if (IsReadOnly)
    {
      throw new InvalidOperationException("Tile object definitions are read-only after registration.");
    }
  }
}

public sealed class TileObjectDefinitionCatalog
{
  private readonly Dictionary<int, TileObjectInheritanceDefinition> _definitions = new();

  public bool IsReadOnly { get; private set; }

  public IReadOnlyCollection<TileObjectInheritanceDefinition> Definitions => _definitions.Values;

  internal void Register(TileObjectInheritanceDefinition definition)
  {
    if (IsReadOnly)
    {
      throw new InvalidOperationException("Tile object definitions are already frozen.");
    }

    ArgumentNullException.ThrowIfNull(definition);
    _definitions[definition.TileType] = definition;
  }

  internal bool TryGet(int tileType, out TileObjectInheritanceDefinition? definition)
  {
    return _definitions.TryGetValue(tileType, out definition);
  }

  internal void Freeze()
  {
    IsReadOnly = true;
    foreach (TileObjectInheritanceDefinition definition in _definitions.Values)
    {
      definition.IsReadOnly = true;
    }
  }
}

public static class TileObjectDefinitionRegistrationSystem
{
  public static void Register(
    TileObjectDefinitionCatalog catalog,
    TileObjectInheritanceDefinition definition)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    catalog.Register(definition);
  }

  public static void CopyFrom(
    TileObjectInheritanceDefinition target,
    TileObjectInheritanceDefinition source)
  {
    ArgumentNullException.ThrowIfNull(target);
    ArgumentNullException.ThrowIfNull(source);
    target.CopyFrom(source);
  }

  public static void FullCopyFrom(
    TileObjectInheritanceDefinition target,
    TileObjectInheritanceDefinition source)
  {
    ArgumentNullException.ThrowIfNull(target);
    ArgumentNullException.ThrowIfNull(source);
    target.FullCopyFrom(source);
  }

  public static void AddAlternate(
    TileObjectInheritanceDefinition definition,
    int alternateTileType)
  {
    ArgumentNullException.ThrowIfNull(definition);
    definition.AddAlternate(alternateTileType);
  }

  public static void Freeze(TileObjectDefinitionCatalog catalog)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    catalog.Freeze();
  }
}

public static class TileObjectDefinitionQuery
{
  public static bool TryGet(
    TileObjectDefinitionCatalog catalog,
    int tileType,
    out TileObjectInheritanceDefinition? definition)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    return catalog.TryGet(tileType, out definition);
  }
}
