using System.Collections.Immutable;

namespace NLTX.EconomyCraftingFishingLoot.Content;

public sealed class RecipeGroupDefinition
{
  public RecipeGroupDefinition(
    RecipeGroupId id,
    int fakeItemId,
    string defaultCombineFormat,
    IEnumerable<int> itemTypeIds,
    int decraftItemTypeId,
    int? registeredId)
  {
    if (!id.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(id));
    }

    if (fakeItemId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(fakeItemId));
    }

    if (string.IsNullOrWhiteSpace(defaultCombineFormat))
    {
      throw new ArgumentException(
        "A recipe group must have a display format.",
        nameof(defaultCombineFormat));
    }

    ArgumentNullException.ThrowIfNull(itemTypeIds);
    ImmutableArray<int> itemTypeIdSnapshot = itemTypeIds.ToImmutableArray();
    if (itemTypeIdSnapshot.IsDefaultOrEmpty)
    {
      throw new ArgumentException(
        "A recipe group must contain at least one item type.",
        nameof(itemTypeIds));
    }

    if (itemTypeIdSnapshot.Any(itemTypeId => itemTypeId < 0))
    {
      throw new ArgumentException(
        "Recipe group item type IDs cannot be negative.",
        nameof(itemTypeIds));
    }

    if (decraftItemTypeId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(decraftItemTypeId));
    }

    if (registeredId is < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(registeredId));
    }

    Id = id;
    FakeItemId = fakeItemId;
    DefaultCombineFormat = defaultCombineFormat;
    ItemTypeIds = itemTypeIdSnapshot;
    ValidItemTypeIds = itemTypeIdSnapshot.ToImmutableHashSet();
    DecraftItemTypeId = decraftItemTypeId;
    RegisteredId = registeredId;
  }

  public RecipeGroupId Id { get; }

  public int FakeItemId { get; }

  public string DefaultCombineFormat { get; }

  public ImmutableArray<int> ItemTypeIds { get; }

  public ImmutableHashSet<int> ValidItemTypeIds { get; }

  public int DecraftItemTypeId { get; }

  public int? RegisteredId { get; }
}
