using System.Collections.ObjectModel;

namespace Terraria.WorldInteraction.TileEntities;

public sealed class DisplayDollComponent
{
  private readonly StoredItemState[] _equipment = new StoredItemState[9];
  private readonly StoredItemState[] _dyes = new StoredItemState[9];
  private readonly StoredItemState[] _miscellaneous = new StoredItemState[1];
  private readonly ReadOnlyCollection<StoredItemState> _equipmentView;
  private readonly ReadOnlyCollection<StoredItemState> _dyesView;
  private readonly ReadOnlyCollection<StoredItemState> _miscellaneousView;

  public DisplayDollComponent()
  {
    _equipmentView = Array.AsReadOnly(_equipment);
    _dyesView = Array.AsReadOnly(_dyes);
    _miscellaneousView = Array.AsReadOnly(_miscellaneous);
  }

  public IReadOnlyList<StoredItemState> Equipment => _equipmentView;
  public IReadOnlyList<StoredItemState> Dyes => _dyesView;
  public IReadOnlyList<StoredItemState> Miscellaneous => _miscellaneousView;
  public byte Pose { get; internal set; }
  public bool ContainsItems =>
      _equipment.Any(item => !item.IsEmpty) ||
      _dyes.Any(item => !item.IsEmpty) ||
      _miscellaneous.Any(item => !item.IsEmpty);
}
