using MinimalImplementation.Server.Inventory.Content;
using MinimalImplementation.Server.Inventory.State;

namespace MinimalImplementation.Server.Inventory.Rules;

public sealed class ItemValidationPolicy
{
  public bool IsValid(InventoryItemState item)
  {
    ArgumentNullException.ThrowIfNull(item);
    if (item.IsEmpty)
    {
      return item.Type == 0 && item.Stack == 0 && item.Prefix == 0 && !item.Favorited;
    }

    return ItemContentFacts.TryGetMaximumStack(item.Type, out int maximumStack)
      && item.Stack <= maximumStack
      && ItemContentFacts.IsValidPrefixForItem(item.Type, item.Prefix);
  }

  public bool CanStack(InventoryItemState destination, InventoryItemState source)
  {
    ArgumentNullException.ThrowIfNull(destination);
    ArgumentNullException.ThrowIfNull(source);
    return !destination.IsEmpty
      && !source.IsEmpty
      && destination.Type == source.Type
      && destination.Prefix == source.Prefix;
  }

  public bool CanQuickStackSource(InventoryItemState item)
  {
    ArgumentNullException.ThrowIfNull(item);
    return !item.IsEmpty && !item.Favorited && !ItemContentFacts.IsCoin(item.Type);
  }

  public bool IsEmpty(InventoryItemState item)
  {
    ArgumentNullException.ThrowIfNull(item);
    return item.IsEmpty;
  }
}
