using System.Collections.Frozen;

namespace Terraria.Content;

public sealed class ProjectileDefinitionCatalog : IProjectileDefinitionQuery
{
  private readonly FrozenDictionary<int, ProjectileDefinition> _definitionsByType;

  public ProjectileDefinitionCatalog(IEnumerable<ProjectileDefinition> definitions)
  {
    ArgumentNullException.ThrowIfNull(definitions);
    _definitionsByType = definitions.ToFrozenDictionary(definition => definition.TypeId);
  }

  public int Count => _definitionsByType.Count;

  public FrozenDictionary<int, ProjectileDefinition> DefinitionsByType => _definitionsByType;

  public int MaximumTypeId => _definitionsByType.Count == 0 ? -1 : _definitionsByType.Keys.Max();

  public bool TryGet(int typeId, out ProjectileDefinition definition)
  {
    return _definitionsByType.TryGetValue(typeId, out definition!);
  }

  internal IEnumerable<ProjectileDefinition> Definitions => _definitionsByType.Values;
}
