using System.Collections.Immutable;

namespace NLTX.EconomyCraftingFishingLoot.Content;

public sealed class RecipeDefinition
{
  public const int MaxRequirements = 15;

  public RecipeDefinition(
    RecipeId id,
    RecipeResultDefinition result,
    IEnumerable<RecipeIngredientDefinition> ingredients,
    int? requiredTileId,
    IEnumerable<RecipeResultDefinition> customShimmerResults,
    IEnumerable<RecipeConditionDefinition> conditions,
    bool notDecraftable = false)
  {
    if (!id.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(id));
    }

    ArgumentNullException.ThrowIfNull(result);
    ArgumentNullException.ThrowIfNull(ingredients);
    ArgumentNullException.ThrowIfNull(customShimmerResults);
    ArgumentNullException.ThrowIfNull(conditions);

    if (requiredTileId is < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(requiredTileId));
    }

    ImmutableArray<RecipeIngredientDefinition> ingredientSnapshot =
      ingredients.ToImmutableArray();
    if (ingredientSnapshot.Length > MaxRequirements)
    {
      throw new ArgumentException(
        $"A recipe cannot contain more than {MaxRequirements} ingredients.",
        nameof(ingredients));
    }

    if (ingredientSnapshot.Any(ingredient => ingredient is null))
    {
      throw new ArgumentException(
        "Recipe ingredients cannot contain null entries.",
        nameof(ingredients));
    }

    ImmutableArray<RecipeResultDefinition> shimmerResultSnapshot =
      customShimmerResults.ToImmutableArray();
    if (shimmerResultSnapshot.Any(resultDefinition => resultDefinition is null))
    {
      throw new ArgumentException(
        "Recipe shimmer results cannot contain null entries.",
        nameof(customShimmerResults));
    }

    ImmutableArray<RecipeConditionDefinition> conditionSnapshot =
      conditions.ToImmutableArray();
    if (conditionSnapshot.Any(condition => condition is null))
    {
      throw new ArgumentException(
        "Recipe conditions cannot contain null entries.",
        nameof(conditions));
    }

    if (conditionSnapshot.Select(condition => condition.Kind).Distinct().Count() !=
      conditionSnapshot.Length)
    {
      throw new ArgumentException(
        "A recipe cannot declare the same condition more than once.",
        nameof(conditions));
    }

    Id = id;
    Result = result;
    Ingredients = ingredientSnapshot;
    RequiredTileId = requiredTileId;
    CustomShimmerResults = shimmerResultSnapshot;
    Conditions = conditionSnapshot;
    NotDecraftable = notDecraftable;
  }

  public RecipeId Id { get; }

  public RecipeResultDefinition Result { get; }

  public ImmutableArray<RecipeIngredientDefinition> Ingredients { get; }

  public int? RequiredTileId { get; }

  public ImmutableArray<RecipeResultDefinition> CustomShimmerResults { get; }

  public ImmutableArray<RecipeConditionDefinition> Conditions { get; }

  public bool NotDecraftable { get; }
}
