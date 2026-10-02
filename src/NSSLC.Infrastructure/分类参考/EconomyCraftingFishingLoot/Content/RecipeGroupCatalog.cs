using System.Collections.Frozen;
using System.Collections.Immutable;

namespace NLTX.EconomyCraftingFishingLoot.Content;

public sealed class RecipeGroupCatalog
{
  private readonly FrozenDictionary<int, RecipeGroupDefinition> _definitionsByGroupId;
  private readonly FrozenDictionary<int, RecipeGroupDefinition> _definitionsByFakeItemId;
  private readonly ImmutableArray<RecipeGroupDefinition> _definitions;

  public RecipeGroupCatalog(IEnumerable<RecipeGroupDefinition> definitions)
  {
    ArgumentNullException.ThrowIfNull(definitions);

    List<RecipeGroupDefinition> snapshot = definitions.ToList();
    Dictionary<int, RecipeGroupDefinition> byGroupId = [];
    Dictionary<int, RecipeGroupDefinition> byFakeItemId = [];

    foreach (RecipeGroupDefinition definition in snapshot)
    {
      ArgumentNullException.ThrowIfNull(definition);
      if (!byGroupId.TryAdd(definition.Id.Value, definition))
      {
        throw new ArgumentException(
          $"Recipe group ID {definition.Id.Value} is registered more than once.",
          nameof(definitions));
      }

      if (!byFakeItemId.TryAdd(definition.FakeItemId, definition))
      {
        throw new ArgumentException(
          $"Fake item ID {definition.FakeItemId} is registered more than once.",
          nameof(definitions));
      }
    }

    _definitions = snapshot.ToImmutableArray();
    _definitionsByGroupId = byGroupId.ToFrozenDictionary();
    _definitionsByFakeItemId = byFakeItemId.ToFrozenDictionary();
  }

  public int Count => _definitions.Length;

  public ImmutableArray<RecipeGroupDefinition> Definitions => _definitions;

  public bool TryGetByGroupId(
    RecipeGroupId groupId,
    out RecipeGroupDefinition definition)
  {
    return _definitionsByGroupId.TryGetValue(groupId.Value, out definition!);
  }

  public bool TryGetByFakeItemId(
    int fakeItemId,
    out RecipeGroupDefinition definition)
  {
    return _definitionsByFakeItemId.TryGetValue(fakeItemId, out definition!);
  }

  public bool ContainsItemType(int fakeItemId, int itemTypeId)
  {
    return TryGetByFakeItemId(fakeItemId, out RecipeGroupDefinition definition) &&
      definition.ValidItemTypeIds.Contains(itemTypeId);
  }
}
