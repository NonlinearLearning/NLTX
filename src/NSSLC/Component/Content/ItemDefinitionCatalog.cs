using System.Collections.Frozen;

namespace Terraria.Content;

public sealed class ItemDefinitionCatalog : IItemDefinitionQuery
{
  private readonly FrozenDictionary<int, ItemDefinition> _definitionsByType;

  public ItemDefinitionCatalog(IEnumerable<ItemDefinition> definitions)
  {
    ArgumentNullException.ThrowIfNull(definitions);
    _definitionsByType = definitions.ToFrozenDictionary(definition => definition.Identity.TypeId);
  }

  public int Count => _definitionsByType.Count;

  public FrozenDictionary<int, ItemDefinition> DefinitionsByType => _definitionsByType;

  public int MaximumTypeId => _definitionsByType.Count == 0 ? -1 : _definitionsByType.Keys.Max();

  public bool TryGet(int typeId, out ItemDefinition definition)
  {
    return _definitionsByType.TryGetValue(typeId, out definition!);
  }

  internal IEnumerable<ItemDefinition> Definitions => _definitionsByType.Values;
}
