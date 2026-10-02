using System.Collections.Immutable;

namespace NLTX.EconomyCraftingFishingLoot.Crafting;

public sealed class RecipeCraftingRuntimeState
{
  public RecipeCraftingRuntimeState(
    IReadOnlyDictionary<int, int>? ownedItems = null,
    IReadOnlyDictionary<int, int>? recipeChestItemCounts = null)
  {
    OwnedItems = SnapshotCounts(ownedItems, nameof(ownedItems));
    RecipeChestItemCounts = SnapshotCounts(
      recipeChestItemCounts,
      nameof(recipeChestItemCounts));
  }

  public ImmutableDictionary<int, int> OwnedItems { get; }

  public ImmutableDictionary<int, int> RecipeChestItemCounts { get; }

  public int GetOwnedItemCount(int itemTypeId)
  {
    ValidateItemTypeId(itemTypeId);
    return OwnedItems.TryGetValue(itemTypeId, out int count) ? count : 0;
  }

  public int GetRecipeChestItemCount(int itemTypeId)
  {
    ValidateItemTypeId(itemTypeId);
    return RecipeChestItemCounts.TryGetValue(itemTypeId, out int count)
      ? count
      : 0;
  }

  public int GetTotalAvailableItemCount(int itemTypeId)
  {
    return checked(
      GetOwnedItemCount(itemTypeId) + GetRecipeChestItemCount(itemTypeId));
  }

  private static ImmutableDictionary<int, int> SnapshotCounts(
    IReadOnlyDictionary<int, int>? counts,
    string parameterName)
  {
    if (counts is null)
    {
      return ImmutableDictionary<int, int>.Empty;
    }

    Dictionary<int, int> snapshot = [];
    foreach ((int itemTypeId, int count) in counts)
    {
      ValidateItemTypeId(itemTypeId, parameterName);
      if (count < 0)
      {
        throw new ArgumentException(
          "Recipe item counts cannot be negative.",
          parameterName);
      }

      snapshot.Add(itemTypeId, count);
    }

    return snapshot.ToImmutableDictionary();
  }

  private static void ValidateItemTypeId(int itemTypeId, string parameterName = "itemTypeId")
  {
    if (itemTypeId < 0)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
