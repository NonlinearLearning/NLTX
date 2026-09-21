using System.Collections.Immutable;

namespace NLTX.EconomyCraftingFishingLoot.Loot;

public sealed class DropOptionsWithoutRepeatsState
{
  private readonly List<int> _availableItems;

  public DropOptionsWithoutRepeatsState(IEnumerable<int> itemIds)
  {
    ArgumentNullException.ThrowIfNull(itemIds);
    _availableItems = itemIds.ToList();
    if (_availableItems.Any(itemId => itemId < 0))
    {
      throw new ArgumentException(
        "Drop option item IDs cannot be negative.",
        nameof(itemIds));
    }
  }

  public int AvailableItemCount => _availableItems.Count;

  public ImmutableArray<int> AvailableItems => _availableItems.ToImmutableArray();

  public bool TryTake(IDropRandomSource random, out int itemId)
  {
    ArgumentNullException.ThrowIfNull(random);
    if (_availableItems.Count == 0)
    {
      itemId = default;
      return false;
    }

    int selectedIndex = random.Next(_availableItems.Count);
    if ((uint)selectedIndex >= (uint)_availableItems.Count)
    {
      throw new InvalidOperationException(
        "The drop random source returned an index outside the option range.");
    }

    itemId = _availableItems[selectedIndex];
    _availableItems.RemoveAt(selectedIndex);
    return true;
  }
}
