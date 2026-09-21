namespace NLTX.PlayerInputGameplay.Player;

public sealed class PlayerRegistryAdapter
{
  private readonly object?[] _players;
  private readonly bool[] _active;

  public PlayerRegistryAdapter(int capacity = 256)
  {
    if (capacity <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(capacity));
    }

    _players = new object?[capacity];
    _active = new bool[capacity];
  }

  public int Capacity => _players.Length;

  public void Bind(int slot, object player)
  {
    ValidateSlot(slot);
    ArgumentNullException.ThrowIfNull(player);
    _players[slot] = player;
    _active[slot] = true;
  }

  public bool Unbind(int slot)
  {
    ValidateSlot(slot);
    var wasBound = _active[slot];
    _players[slot] = null;
    _active[slot] = false;
    return wasBound;
  }

  public bool TryGet(int slot, out object? player)
  {
    ValidateSlot(slot);
    player = _players[slot];
    return _active[slot];
  }

  public bool IsActive(int slot)
  {
    ValidateSlot(slot);
    return _active[slot];
  }

  private void ValidateSlot(int slot)
  {
    if (slot < 0 || slot >= _players.Length)
    {
      throw new ArgumentOutOfRangeException(nameof(slot));
    }
  }
}
