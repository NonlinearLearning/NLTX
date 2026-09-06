using System.Collections.Frozen;

namespace Terraria.Content;

public sealed class WallDefinitionCatalog : IWallDefinitionQuery
{
  private readonly FrozenDictionary<int, WallDefinition> _definitionsByType;

  public WallDefinitionCatalog(IEnumerable<WallDefinition> definitions)
  {
    ArgumentNullException.ThrowIfNull(definitions);
    _definitionsByType = definitions.ToFrozenDictionary(definition => definition.TypeId);
  }

  public int Count => _definitionsByType.Count;

  public FrozenDictionary<int, WallDefinition> DefinitionsByType => _definitionsByType;

  public int MaximumTypeId => _definitionsByType.Count == 0 ? -1 : _definitionsByType.Keys.Max();

  public bool TryGet(int typeId, out WallDefinition definition)
  {
    return _definitionsByType.TryGetValue(typeId, out definition!);
  }

  internal IEnumerable<WallDefinition> Definitions => _definitionsByType.Values;
}
