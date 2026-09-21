namespace NLTX.PlayerInputGameplay.Pickup;

public sealed class PlayerItemPickupLogComponent
{
  private readonly List<PlayerItemPickupLogEntry> _entries = new();

  public bool Enabled { get; private set; }

  public IReadOnlyList<PlayerItemPickupLogEntry> Entries => _entries;

  public void SetEnabled(bool enabled)
  {
    Enabled = enabled;
  }

  public void Add(PlayerItemPickupLogEntry entry)
  {
    if (Enabled)
    {
      _entries.Add(entry);
    }
  }

  public void Clear()
  {
    _entries.Clear();
  }
}
