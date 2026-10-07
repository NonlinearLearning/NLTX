using System.Collections.Frozen;

namespace Terraria.Content;

public sealed class BuffDefinitionCatalog : IBuffDefinitionQuery
{
  private readonly FrozenDictionary<int, BuffDefinition> _definitionsByType;

  public BuffDefinitionCatalog(IEnumerable<BuffDefinition> definitions)
  {
    ArgumentNullException.ThrowIfNull(definitions);
    _definitionsByType = definitions.ToFrozenDictionary(definition => definition.TypeId);
  }

  public int Count => _definitionsByType.Count;

  public FrozenDictionary<int, BuffDefinition> DefinitionsByType => _definitionsByType;

  public int MaximumTypeId => _definitionsByType.Count == 0 ? -1 : _definitionsByType.Keys.Max();

  public bool TryGet(int typeId, out BuffDefinition definition)
  {
    return _definitionsByType.TryGetValue(typeId, out definition!);
  }

  internal IEnumerable<BuffDefinition> Definitions => _definitionsByType.Values;
}
