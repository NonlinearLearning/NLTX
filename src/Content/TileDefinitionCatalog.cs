using System.Collections.Frozen;

namespace Terraria.Content;

public sealed class TileDefinitionCatalog : ITileDefinitionQuery
{
  private readonly FrozenDictionary<int, TileDefinition> _definitionsByType;

  public TileDefinitionCatalog(IEnumerable<TileDefinition> definitions)
  {
    ArgumentNullException.ThrowIfNull(definitions);
    _definitionsByType = definitions.ToFrozenDictionary(definition => definition.TypeId);
  }

  public int Count => _definitionsByType.Count;

  public FrozenDictionary<int, TileDefinition> DefinitionsByType => _definitionsByType;

  public int MaximumTypeId => _definitionsByType.Count == 0 ? -1 : _definitionsByType.Keys.Max();

  public bool TryGet(int typeId, out TileDefinition definition)
  {
    return _definitionsByType.TryGetValue(typeId, out definition!);
  }

  internal IEnumerable<TileDefinition> Definitions => _definitionsByType.Values;
}
