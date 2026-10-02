using System.Collections.ObjectModel;
using Terraria.WorldInteraction.Tiles;

namespace Terraria.WorldInteraction.Wiring;

public sealed class WiringMechanismScheduleComponent
{
  public const int MaximumEntryCount = 1000;

  private readonly MechanismCooldownEntry[] _entries =
      new MechanismCooldownEntry[MaximumEntryCount];
  private int _count;

  public WiringMechanismScheduleComponent()
  {
  }

  public IReadOnlyList<MechanismCooldownEntry> Entries => SnapshotEntries();

  public int Capacity => MaximumEntryCount;

  public int Count
  {
    get => _count;
    internal set
    {
      if (value < 0 || value > MaximumEntryCount)
      {
        throw new ArgumentOutOfRangeException(nameof(value));
      }

      _count = value;
    }
  }

  internal void Reset()
  {
    Array.Clear(_entries);
    Count = 0;
  }

  internal bool TrySchedule(MechanismCooldownEntry entry)
  {
    if (_count >= MaximumEntryCount || ContainsPosition(entry.Position))
    {
      return false;
    }

    _entries[_count] = entry;
    _count++;
    return true;
  }

  internal bool ContainsPosition(TileCoordinate position)
  {
    for (int index = 0; index < _count; index++)
    {
      if (_entries[index].Position == position)
      {
        return true;
      }
    }

    return false;
  }

  internal MechanismCooldownEntry GetEntry(int index)
  {
    if (index < 0 || index >= _count)
    {
      throw new ArgumentOutOfRangeException(nameof(index));
    }

    return _entries[index];
  }

  internal void SetEntry(int index, MechanismCooldownEntry entry)
  {
    if (index < 0 || index >= _count)
    {
      throw new ArgumentOutOfRangeException(nameof(index));
    }

    _entries[index] = entry;
  }

  internal MechanismCooldownEntry RemoveAt(int index)
  {
    if (index < 0 || index >= _count)
    {
      throw new ArgumentOutOfRangeException(nameof(index));
    }

    MechanismCooldownEntry removed = _entries[index];
    for (int moveIndex = index; moveIndex < _count - 1; moveIndex++)
    {
      _entries[moveIndex] = _entries[moveIndex + 1];
    }

    _entries[_count - 1] = default;
    _count--;
    return removed;
  }

  private IReadOnlyList<MechanismCooldownEntry> SnapshotEntries()
  {
    MechanismCooldownEntry[] snapshot = new MechanismCooldownEntry[_count];
    Array.Copy(_entries, snapshot, _count);
    return Array.AsReadOnly(snapshot);
  }
}
