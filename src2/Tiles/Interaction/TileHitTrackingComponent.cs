using System.Collections.ObjectModel;

namespace Terraria.Tiles.Interaction;

public sealed class TileHitTrackingComponent
{
  private readonly TileHitEntry[] _entries = new TileHitEntry[TileHitTrackingPolicy.ArraySize];
  private readonly int[] _order = new int[TileHitTrackingPolicy.ArraySize];
  private readonly ReadOnlyCollection<TileHitEntry> _readOnlyEntries;
  private readonly ReadOnlyCollection<int> _readOnlyOrder;

  public TileHitTrackingComponent()
  {
    _readOnlyEntries = Array.AsReadOnly(_entries);
    _readOnlyOrder = Array.AsReadOnly(_order);
    Reset();
  }

  public IReadOnlyList<TileHitEntry> Entries => _readOnlyEntries;

  public IReadOnlyList<int> Order => _readOnlyOrder;

  internal void ClearSlot(int slot)
  {
    ValidateSlot(slot);
    _entries[slot] = TileHitEntry.Empty;
  }

  internal void Reset()
  {
    for (int slot = 0; slot < TileHitTrackingPolicy.ArraySize; slot++)
    {
      _entries[slot] = TileHitEntry.Empty;
      _order[slot] = slot;
    }
  }

  internal TileHitEntry ReadEntry(int slot)
  {
    ValidateSlot(slot);
    return _entries[slot];
  }

  internal int ReadOldestSlot()
  {
    return _order[0];
  }

  internal void Touch(int slot)
  {
    ValidateSlot(slot);
    int orderIndex = Array.IndexOf(_order, slot, 0, TileHitTrackingPolicy.Capacity);
    if (orderIndex < 0 || orderIndex == TileHitTrackingPolicy.Capacity - 1)
    {
      return;
    }

    for (int index = orderIndex; index < TileHitTrackingPolicy.Capacity - 1; index++)
    {
      _order[index] = _order[index + 1];
    }

    _order[TileHitTrackingPolicy.Capacity - 1] = slot;
    _order[TileHitTrackingPolicy.SentinelSlotIndex] = TileHitTrackingPolicy.SentinelSlotIndex;
  }

  internal void WriteEntry(int slot, TileHitEntry entry)
  {
    ValidateSlot(slot);
    _entries[slot] = entry;
  }

  private static void ValidateSlot(int slot)
  {
    if (slot < 0 || slot >= TileHitTrackingPolicy.Capacity)
    {
      throw new ArgumentOutOfRangeException(nameof(slot));
    }
  }
}
