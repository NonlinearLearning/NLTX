using System.Collections.Frozen;

namespace Terraria.Content;

public sealed class NpcDefinitionCatalog : INpcDefinitionQuery
{
  private readonly FrozenDictionary<int, NpcDefinition> _definitionsByNetId;
  private readonly FrozenDictionary<int, NpcDefinition> _definitionsByType;

  public NpcDefinitionCatalog(IEnumerable<NpcDefinition> definitions)
  {
    ArgumentNullException.ThrowIfNull(definitions);
    _definitionsByNetId = definitions.ToFrozenDictionary(definition => definition.NetId);
    _definitionsByType = _definitionsByNetId.Values
      .GroupBy(static definition => definition.TypeId)
      .ToFrozenDictionary(group => group.Key, group => group.First());
  }

  public int Count => _definitionsByNetId.Count;

  public FrozenDictionary<int, NpcDefinition> DefinitionsByNetId => _definitionsByNetId;

  public FrozenDictionary<int, NpcDefinition> DefinitionsByType => _definitionsByType;

  public int MinimumNetId => _definitionsByNetId.Count == 0 ? 0 : _definitionsByNetId.Keys.Min();

  public int MaximumNetId => _definitionsByNetId.Count == 0 ? 0 : _definitionsByNetId.Keys.Max();

  public bool TryGetByNetId(int netId, out NpcDefinition definition)
  {
    return _definitionsByNetId.TryGetValue(netId, out definition!);
  }

  public bool TryGetByTypeId(int typeId, out NpcDefinition definition)
  {
    return _definitionsByType.TryGetValue(typeId, out definition!);
  }

  internal IEnumerable<NpcDefinition> Definitions => _definitionsByNetId.Values;
}
