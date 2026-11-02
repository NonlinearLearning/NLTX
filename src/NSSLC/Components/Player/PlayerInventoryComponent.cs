namespace Terraria.Player;

/// <summary>Minimal authoritative item slots needed by the Version4 drop and pickup path.</summary>
public sealed class PlayerInventoryComponent
{
  private readonly Dictionary<int, PlayerInventorySlot> _slots = [];

  public bool TryGet(int slot, out PlayerInventorySlot item)
  {
    return _slots.TryGetValue(slot, out item);
  }

  public void Set(int slot, PlayerInventorySlot item)
  {
    if (slot < 0 || slot > short.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(slot));
    }

    _slots[slot] = item;
  }
}

/// <summary>One network-visible player inventory slot.</summary>
public readonly record struct PlayerInventorySlot(
    int ItemType,
    int Stack,
    byte Prefix,
    bool Favorited)
{
  public bool IsEmpty => ItemType <= 0 || Stack <= 0;
}
