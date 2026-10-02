using System.Collections.Immutable;

namespace NLTX.EconomyCraftingFishingLoot.Content;

public sealed class RecipeQualificationContext
{
  public RecipeQualificationContext(
    IReadOnlyDictionary<int, int> itemCounts,
    IEnumerable<int> availableTileIds,
    IReadOnlyDictionary<RecipeConditionKind, bool> conditionValues)
  {
    ArgumentNullException.ThrowIfNull(itemCounts);
    ArgumentNullException.ThrowIfNull(availableTileIds);
    ArgumentNullException.ThrowIfNull(conditionValues);

    Dictionary<int, int> itemCountSnapshot = [];
    foreach ((int itemTypeId, int count) in itemCounts)
    {
      if (itemTypeId < 0)
      {
        throw new ArgumentException(
          "Item type IDs cannot be negative.",
          nameof(itemCounts));
      }

      if (count < 0)
      {
        throw new ArgumentException(
          "Item counts cannot be negative.",
          nameof(itemCounts));
      }

      itemCountSnapshot.Add(itemTypeId, count);
    }

    HashSet<int> tileSnapshot = [];
    foreach (int tileId in availableTileIds)
    {
      if (tileId < 0)
      {
        throw new ArgumentException(
          "Tile IDs cannot be negative.",
          nameof(availableTileIds));
      }

      tileSnapshot.Add(tileId);
    }

    Dictionary<RecipeConditionKind, bool> conditionSnapshot = [];
    foreach ((RecipeConditionKind kind, bool value) in conditionValues)
    {
      if (kind == RecipeConditionKind.None ||
        !Enum.IsDefined(typeof(RecipeConditionKind), kind))
      {
        throw new ArgumentException(
          "Recipe condition keys must identify a defined condition.",
          nameof(conditionValues));
      }

      conditionSnapshot.Add(kind, value);
    }

    ItemCounts = itemCountSnapshot.ToImmutableDictionary();
    AvailableTileIds = tileSnapshot.ToImmutableHashSet();
    ConditionValues = conditionSnapshot.ToImmutableDictionary();
  }

  public ImmutableDictionary<int, int> ItemCounts { get; }

  public ImmutableHashSet<int> AvailableTileIds { get; }

  public ImmutableDictionary<RecipeConditionKind, bool> ConditionValues { get; }
}
