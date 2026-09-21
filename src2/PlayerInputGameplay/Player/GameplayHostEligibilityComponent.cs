namespace NLTX.PlayerInputGameplay.Player;

public sealed class GameplayHostEligibilityComponent
{
  private bool[] _eligible = Array.Empty<bool>();

  public IReadOnlyList<bool> Values => _eligible;

  public void Resize(int capacity)
  {
    if (capacity <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(capacity));
    }

    _eligible = new bool[capacity];
  }

  public void Set(int slot, bool countsAsHost)
  {
    ValidateSlot(slot);
    _eligible[slot] = countsAsHost;
  }

  public bool CountsAsHost(int slot)
  {
    ValidateSlot(slot);
    return _eligible[slot];
  }

  private void ValidateSlot(int slot)
  {
    if (slot < 0 || slot >= _eligible.Length)
    {
      throw new ArgumentOutOfRangeException(nameof(slot));
    }
  }
}
