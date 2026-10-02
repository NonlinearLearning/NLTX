using System.Collections.Frozen;
using System.Collections.Immutable;

namespace Terraria.Content;

public sealed class RecipeGroupCatalog
{
  private readonly FrozenDictionary<int, RecipeGroupDefinition> _definitions;
  private readonly FrozenDictionary<int, int> _groupIdByFakeItemId;

  public RecipeGroupCatalog(IEnumerable<RecipeGroupDefinition> definitions, bool isComplete = false)
  {
    ArgumentNullException.ThrowIfNull(definitions);
    _definitions = definitions.ToFrozenDictionary(definition => definition.GroupId);
    _groupIdByFakeItemId = _definitions.Values.ToFrozenDictionary(
      definition => definition.FakeItemId,
      definition => definition.GroupId);
    IsComplete = isComplete;
  }

  public int Count => _definitions.Count;

  public FrozenDictionary<int, RecipeGroupDefinition> DefinitionsByGroupId => _definitions;

  public FrozenDictionary<int, int> GroupIdByFakeItemId => _groupIdByFakeItemId;

  public bool IsComplete { get; }

  internal IEnumerable<RecipeGroupDefinition> Definitions => _definitions.Values;

  public bool TryGet(int groupId, out RecipeGroupDefinition definition)
  {
    return _definitions.TryGetValue(groupId, out definition!);
  }
}
