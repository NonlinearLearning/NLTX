using System.Collections.ObjectModel;
using Terraria.Items;

namespace Terraria.WorldInteraction.Structures;

public sealed class MultiSlotItemPayloadComponent
{
  private readonly List<ItemState> _items = new();
  private readonly ReadOnlyCollection<ItemState> _itemsView;

  public MultiSlotItemPayloadComponent()
  {
    _itemsView = _items.AsReadOnly();
  }

  public IReadOnlyList<ItemState> Items => _itemsView;

  public int ItemCapacity { get; internal set; }
}
