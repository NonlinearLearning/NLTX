using System.Collections.ObjectModel;

namespace Terraria.WorldInteraction.TileEntities;

public sealed class HatRackComponent
{
  private readonly StoredItemState[] _items = new StoredItemState[2];
  private readonly StoredItemState[] _dyes = new StoredItemState[2];
  private readonly ReadOnlyCollection<StoredItemState> _itemsView;
  private readonly ReadOnlyCollection<StoredItemState> _dyesView;

  public HatRackComponent()
  {
    _itemsView = Array.AsReadOnly(_items);
    _dyesView = Array.AsReadOnly(_dyes);
  }

  public IReadOnlyList<StoredItemState> Items => _itemsView;
  public IReadOnlyList<StoredItemState> Dyes => _dyesView;
  public bool ContainsItems =>
      _items.Any(item => !item.IsEmpty) ||
      _dyes.Any(item => !item.IsEmpty);
}
