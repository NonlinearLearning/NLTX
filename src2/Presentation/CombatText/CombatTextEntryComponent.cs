using System.Collections.ObjectModel;

namespace Terraria.Presentation.CombatText;

public sealed class CombatTextEntryComponent
{
  private readonly CombatTextEntry[] _entries =
    new CombatTextEntry[CombatTextPoolPolicy.Capacity];
  private readonly ReadOnlyCollection<CombatTextEntry> _readOnlyEntries;

  public CombatTextEntryComponent()
  {
    _readOnlyEntries = Array.AsReadOnly(_entries);
    for (int slot = 0; slot < _entries.Length; slot++)
    {
      _entries[slot] = CombatTextEntry.Empty;
    }
  }

  public IReadOnlyList<CombatTextEntry> Entries => _readOnlyEntries;

  internal void ClearSlot(int slot)
  {
    ValidateSlot(slot);
    _entries[slot] = CombatTextEntry.Empty;
  }

  internal CombatTextEntry ReadEntry(int slot)
  {
    ValidateSlot(slot);
    return _entries[slot];
  }

  internal void WriteEntry(int slot, CombatTextEntry entry)
  {
    ValidateSlot(slot);
    _entries[slot] = entry;
  }

  private static void ValidateSlot(int slot)
  {
    if (slot < 0 || slot >= CombatTextPoolPolicy.Capacity)
    {
      throw new ArgumentOutOfRangeException(nameof(slot));
    }
  }
}
