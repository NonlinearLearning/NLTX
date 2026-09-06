using System.Collections.Frozen;
using System.Collections.Immutable;

namespace Terraria.Content;

public sealed class RecipeDefinitionCatalog
{
  private readonly FrozenDictionary<int, RecipeDefinition> _definitions;

  public RecipeDefinitionCatalog(IEnumerable<RecipeDefinition> definitions, bool isComplete = false)
  {
    ArgumentNullException.ThrowIfNull(definitions);
    _definitions = definitions.ToFrozenDictionary(definition => definition.RecipeId);
    IsComplete = isComplete;
  }

  public int Count => _definitions.Count;

  public FrozenDictionary<int, RecipeDefinition> DefinitionsByRecipeId => _definitions;

  public ImmutableArray<int> RecipeIds => _definitions.Keys.Order().ToImmutableArray();

  public bool IsComplete { get; }

  public bool TryGet(int recipeId, out RecipeDefinition definition)
  {
    return _definitions.TryGetValue(recipeId, out definition!);
  }

  internal IEnumerable<RecipeDefinition> Definitions => _definitions.Values;
}
