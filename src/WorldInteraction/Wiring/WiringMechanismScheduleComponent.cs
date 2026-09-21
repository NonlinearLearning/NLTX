using System.Collections.ObjectModel;

namespace Terraria.WorldInteraction.Wiring;

public sealed class WiringMechanismScheduleComponent
{
  public const int MaximumEntryCount = 1000;

  private readonly MechanismCooldownEntry[] _entries =
      new MechanismCooldownEntry[MaximumEntryCount];
  private readonly ReadOnlyCollection<MechanismCooldownEntry> _entriesView;
  private int _count;

  public WiringMechanismScheduleComponent()
  {
    _entriesView = Array.AsReadOnly(_entries);
  }

  public IReadOnlyList<MechanismCooldownEntry> Entries => _entriesView;

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
}
