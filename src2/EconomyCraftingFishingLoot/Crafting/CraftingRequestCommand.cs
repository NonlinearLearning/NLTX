using System.Collections.Immutable;
using NLTX.EconomyCraftingFishingLoot.Content;

namespace NLTX.EconomyCraftingFishingLoot.Crafting;

public sealed class CraftingRequestCommand
{
  public CraftingRequestCommand(
    string requestId,
    RecipeDefinition recipe,
    CraftingItemSnapshot result,
    IEnumerable<CraftingItemSnapshot> consumedItems,
    IEnumerable<CraftingIngredientRequest> requestedIngredients,
    bool quickCraft)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(requestId);
    ArgumentNullException.ThrowIfNull(recipe);
    ArgumentNullException.ThrowIfNull(consumedItems);
    ArgumentNullException.ThrowIfNull(requestedIngredients);

    ImmutableArray<CraftingItemSnapshot> consumedSnapshot =
      consumedItems.ToImmutableArray();
    ImmutableArray<CraftingIngredientRequest> requestedSnapshot =
      requestedIngredients.ToImmutableArray();

    RequestId = requestId;
    Recipe = recipe;
    Result = result;
    ConsumedItems = consumedSnapshot;
    RequestedIngredients = requestedSnapshot;
    QuickCraft = quickCraft;
  }

  public string RequestId { get; }

  public RecipeDefinition Recipe { get; }

  public CraftingItemSnapshot Result { get; }

  public ImmutableArray<CraftingItemSnapshot> ConsumedItems { get; }

  public ImmutableArray<CraftingIngredientRequest> RequestedIngredients { get; }

  public bool QuickCraft { get; }
}
